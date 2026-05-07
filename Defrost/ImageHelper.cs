using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.ImageSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.ImageSharp.Processing.Processors.Filters;
using Windows.Graphics.Imaging;

namespace Pso2Tools.Defrost;

public enum CastTextureShift
{
	NgsArms,
	NgsBody,
	NgsLegs,
	ClassicArms,
	ClassicBody,
	ClassicLegs,
}

public struct MaskColors
{
	public Rgba32 R;
	public Rgba32 G;
	public Rgba32 B;
	public Rgba32 A;
}

public struct MaskUsedChannels
{
	public bool R;
	public bool G;
	public bool B;
	public bool A;

	public readonly bool Any => R || G || B || A;
}

public static class ImageHelper
{
	public static async Task<Image<Rgba32>> DdsBufferToImageAsync(
		byte[] fileBytes,
		CancellationToken token = default
	)
	{
		using var stream = new MemoryStream(fileBytes);
		var decoder = new BcDecoder();

		return await decoder.DecodeToImageRgba32Async(stream, token);
	}

	public static async Task<byte[]> ImageToDdsBufferAsync(
		Image<Rgba32> image,
		CancellationToken token = default
	)
	{
		var encoder = new BcEncoder(BCnEncoder.Shared.CompressionFormat.Rgba);
		encoder.OutputOptions.FileFormat = BCnEncoder.Shared.OutputFileFormat.Dds;

		using var stream = new MemoryStream();
		await encoder.EncodeToStreamAsync(image, stream, token);

		return stream.ToArray();
	}

	public static SoftwareBitmap ImageToSoftwareBitmap(Image<Bgra32> image)
	{
		var bitmap = new SoftwareBitmap(
			BitmapPixelFormat.Bgra8,
			image.Width,
			image.Height,
			BitmapAlphaMode.Premultiplied
		);

		var pixels = new byte[image.Width * image.Height * 4];
		image.CopyPixelDataTo(pixels);
		bitmap.CopyFromBuffer(pixels.AsBuffer());

		return bitmap;
	}

	public static Image<Bgra32> ApplyImagePreviewMode(Image image, ImagePreviewMode mode)
	{
		var clone = image.CloneAs<Bgra32>();

		clone.Mutate(x =>
			x.ApplyProcessor(
				mode switch
				{
					// If alpha channel is used, colors need alpha premultiplied
					ImagePreviewMode.ColorAndAlpha => ChannelProcessors.PremultiplyAlpha,
					ImagePreviewMode.Color => ChannelProcessors.Color,
					ImagePreviewMode.Red => ChannelProcessors.Red,
					ImagePreviewMode.Green => ChannelProcessors.Green,
					ImagePreviewMode.Blue => ChannelProcessors.Blue,
					ImagePreviewMode.Alpha => ChannelProcessors.Alpha,
					_ => throw new NotImplementedException(),
				}
			)
		);

		return clone;
	}

	public static Image<Rgba32> ShiftCastPartTexture(Image<Rgba32> image, CastTextureShift shift)
	{
		var usedWidth = (int)((double)image.Width * 2 / 3);
		var offset = shift switch
		{
			CastTextureShift.NgsArms => 0,
			CastTextureShift.NgsBody => usedWidth,
			CastTextureShift.NgsLegs => usedWidth * 2,
			CastTextureShift.ClassicArms => usedWidth * 2,
			CastTextureShift.ClassicBody => usedWidth,
			CastTextureShift.ClassicLegs => 0,
			_ => 0,
		};

		var newImage = new Image<Rgba32>(image.Width * 2, image.Height, new Rgba32(0, 0, 0, 0));

		newImage.Mutate(x =>
			x.DrawImage(
				image,
				backgroundLocation: new Point(offset, 0),
				foregroundRectangle: new Rectangle(0, 0, usedWidth, image.Height),
				opacity: 1
			)
		);

		return newImage;
	}

	public static MaskUsedChannels GetUsedMaskChannels(Image<Rgba32> image)
	{
		var sw = Stopwatch.StartNew();

		long totalR = 0;
		long totalG = 0;
		long totalB = 0;
		long totalA = 0;

		image.ProcessPixelRows(accessor =>
		{
			for (int y = 0; y < accessor.Height; y++)
			{
				foreach (ref var pixel in accessor.GetRowSpan(y))
				{
					totalR += pixel.R;
					totalG += pixel.G;
					totalB += pixel.B;
					totalA += pixel.A;
				}
			}
		});

		var pixelCount = image.Width * image.Height;
		var averageR = totalR / pixelCount;
		var averageG = totalG / pixelCount;
		var averageB = totalB / pixelCount;
		var averageA = totalA / pixelCount;

		Debug.WriteLine($"GetMaskUsedChannels {sw.ElapsedMilliseconds} ms");

		return new MaskUsedChannels()
		{
			R = 0 < averageR && averageR < 252,
			G = 0 < averageG && averageG < 252,
			B = 0 < averageB && averageB < 252,
			A = 0 < averageA && averageA < 252,
		};
	}

	public static Image<Rgba32> ColorizeDiffuseTexture(
		Image<Rgba32> diffuseImage,
		Image<Rgba32> maskImage,
		MaskColors colors,
		PixelBlender<Rgba32> blender,
		out MaskUsedChannels usedChannels,
		CancellationToken token = default
	)
	{
		if (diffuseImage.Size != maskImage.Size)
		{
			throw new ArgumentException("Images must be the same size");
		}

		var used = GetUsedMaskChannels(maskImage);
		usedChannels = used;

		diffuseImage.ProcessPixelRows(
			maskImage,
			(destAccessor, maskAccessor) =>
			{
				var configuration = new Configuration();
				int width = destAccessor.Width;

				using var colorBuffer = configuration.MemoryAllocator.Allocate<Rgba32>(width * 4);
				using var floatBuffer = configuration.MemoryAllocator.Allocate<float>(width * 4);

				var colorR = colorBuffer.Memory.Span.Slice(width * 0, width);
				var colorG = colorBuffer.Memory.Span.Slice(width * 1, width);
				var colorB = colorBuffer.Memory.Span.Slice(width * 2, width);
				var colorA = colorBuffer.Memory.Span.Slice(width * 3, width);

				var maskR = floatBuffer.Memory.Span.Slice(width * 0, width);
				var maskG = floatBuffer.Memory.Span.Slice(width * 1, width);
				var maskB = floatBuffer.Memory.Span.Slice(width * 2, width);
				var maskA = floatBuffer.Memory.Span.Slice(width * 3, width);

				if (used.R)
				{
					colorR.Fill(colors.R);
				}
				if (used.G)
				{
					colorG.Fill(colors.G);
				}
				if (used.B)
				{
					colorB.Fill(colors.B);
				}
				if (used.A)
				{
					colorA.Fill(colors.A);
				}

				for (int y = 0; y < destAccessor.Height; y++)
				{
					token.ThrowIfCancellationRequested();

					var dest = destAccessor.GetRowSpan(y);
					var mask = maskAccessor.GetRowSpan(y);

					for (int x = 0; x < width; x++)
					{
						var maskVector = mask[x].ToScaledVector4();

						maskR[x] = maskVector.X;
						maskG[x] = maskVector.Y;
						maskB[x] = maskVector.Z;
						maskA[x] = maskVector.W;
					}

					// TODO: this would be faster if I could access the pixel blender's internal
					// BlendFunction() and do all four things as Vector4 without and only convert
					// from Rgba32 once at the start and back to Rgba32 once at the end.

					if (used.R)
					{
						blender.Blend(configuration, dest, dest, colorR, maskR);
					}
					if (used.G)
					{
						blender.Blend(configuration, dest, dest, colorG, maskG);
					}
					if (used.B)
					{
						blender.Blend(configuration, dest, dest, colorB, maskB);
					}
					if (used.A)
					{
						blender.Blend(configuration, dest, dest, colorA, maskA);
					}
				}
			}
		);

		return diffuseImage;
	}

	public static Image<Rgba32> ColorizeDiffuseTexture(
		Image<Rgba32> diffuseImage,
		Image<Rgba32> maskImage,
		MaskColors colors,
		PixelColorBlendingMode colorMode,
		out MaskUsedChannels usedChannels,
		CancellationToken token = default
	)
	{
		var blender = new PixelOperations<Rgba32>().GetPixelBlender(
			colorMode,
			PixelAlphaCompositionMode.SrcAtop
		);
		return ColorizeDiffuseTexture(
			diffuseImage,
			maskImage,
			colors,
			blender,
			out usedChannels,
			token
		);
	}
}

public static class ChannelProcessors
{
	public static IImageProcessor PremultiplyAlpha => new PremultiplyAlphaProcessor();
	public static IImageProcessor Color => new FilterProcessor(ColorFilter);
	public static IImageProcessor Red => new FilterProcessor(RedFilter);
	public static IImageProcessor Green => new FilterProcessor(GreenFilter);
	public static IImageProcessor Blue => new FilterProcessor(BlueFilter);
	public static IImageProcessor Alpha => new FilterProcessor(AlphaFilter);

	private static ColorMatrix ColorFilter { get; } =
		new ColorMatrix
		{
			M11 = 1,
			M22 = 1,
			M33 = 1,
			M54 = 1,
		};

	private static ColorMatrix RedFilter { get; } =
		new ColorMatrix
		{
			M11 = 1,
			M12 = 1,
			M13 = 1,
			M54 = 1,
		};

	private static ColorMatrix GreenFilter { get; } =
		new ColorMatrix
		{
			M21 = 1,
			M22 = 1,
			M23 = 1,
			M54 = 1,
		};

	private static ColorMatrix BlueFilter { get; } =
		new ColorMatrix
		{
			M31 = 1,
			M32 = 1,
			M33 = 1,
			M54 = 1,
		};

	private static ColorMatrix AlphaFilter { get; } =
		new ColorMatrix
		{
			M41 = 1,
			M42 = 1,
			M43 = 1,
			M54 = 1,
		};
}

// Based on various code from https://github.com/SixLabors/ImageSharp
public class PremultiplyAlphaProcessor : IImageProcessor
{
	public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(
		Configuration configuration,
		Image<TPixel> source,
		Rectangle sourceRectangle
	)
		where TPixel : unmanaged, IPixel<TPixel> =>
		new PremultiplyAlphaProcessor<TPixel>(configuration, source, sourceRectangle);
}

public partial class PremultiplyAlphaProcessor<TPixel>(
	Configuration configuration,
	Image<TPixel> source,
	Rectangle sourceRectangle
) : ImageProcessor<TPixel>(configuration, source, sourceRectangle)
	where TPixel : unmanaged, IPixel<TPixel>
{
	protected override void OnFrameApply(ImageFrame<TPixel> source)
	{
		var interest = Rectangle.Intersect(SourceRectangle, source.Bounds());
		var operation = new RowOperation(interest.X, source.PixelBuffer, Configuration);

		ParallelRowIterator.IterateRows<RowOperation, Vector4>(
			Configuration,
			interest,
			in operation
		);
	}

	private readonly struct RowOperation(
		int startX,
		Buffer2D<TPixel> source,
		Configuration configuration
	) : IRowOperation<Vector4>
	{
		public int GetRequiredBufferLength(Rectangle bounds) => bounds.Width;

		public void Invoke(int y, Span<Vector4> span)
		{
			var rowSpan = source.DangerousGetRowSpan(y).Slice(startX, span.Length);
			PixelOperations<TPixel>.Instance.ToVector4(configuration, rowSpan, span);

			Premultiply(span);

			PixelOperations<TPixel>.Instance.FromVector4Destructive(configuration, span, rowSpan);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Premultiply(ref Vector4 source)
	{
		Vector4 multiplied = source * source.W;
		multiplied.W = source.W;
		source = multiplied;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Premultiply(Span<Vector4> vectors)
	{
		ref Vector4 vectorsStart = ref MemoryMarshal.GetReference(vectors);
		ref Vector4 vectorsEnd = ref Unsafe.Add(ref vectorsStart, (uint)vectors.Length);

		while (Unsafe.IsAddressLessThan(ref vectorsStart, ref vectorsEnd))
		{
			Premultiply(ref vectorsStart);

			vectorsStart = ref Unsafe.Add(ref vectorsStart, 1);
		}
	}
}
