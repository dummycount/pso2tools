using System;
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
	Arms,
	Body,
	Legs,
}

public struct MaskColors
{
	public Rgba32 R;
	public Rgba32 G;
	public Rgba32 B;
	public Rgba32 A;
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
			CastTextureShift.Arms => 0,
			CastTextureShift.Body => usedWidth,
			CastTextureShift.Legs => usedWidth * 2,
			_ => 0,
		};

		using var newImage = new Image<Rgba32>(
			image.Width * 2,
			image.Height,
			new Rgba32(0, 0, 0, 0)
		);

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

	public static RgbaVector GetUsedMaskChannels(Image<Rgba32> image)
	{
		var mask = new Rgba32(255, 255, 255, 255);

		image.ProcessPixelRows(accessor =>
		{
			for (int y = 0; y < accessor.Height; y++)
			{
				foreach (ref var pixel in accessor.GetRowSpan(y))
				{
					mask.PackedValue &= pixel.PackedValue;
				}
			}
		});

		return new RgbaVector(
			mask.R < 255 ? 1 : 0,
			mask.G < 255 ? 1 : 0,
			mask.B < 255 ? 1 : 0,
			mask.A < 255 ? 1 : 0
		);
	}

	public static Image<Rgba32> ColorizeDiffuseTexture(
		Image<Rgba32> diffuseImage,
		Image<Rgba32> maskImage,
		MaskColors colors,
		PixelBlender<Rgba32> blender,
		CancellationToken token = default
	)
	{
		if (diffuseImage.Size != maskImage.Size)
		{
			throw new ArgumentException("Images must be the same size");
		}

		var used = GetUsedMaskChannels(maskImage);

		// TODO: there are probably much faster ways to do this
		diffuseImage.ProcessPixelRows(
			maskImage,
			(destAccessor, maskAccessor) =>
			{
				for (int y = 0; y < destAccessor.Height; y++)
				{
					token.ThrowIfCancellationRequested();

					var dest = destAccessor.GetRowSpan(y);
					var mask = maskAccessor.GetRowSpan(y);

					for (int x = 0; x < dest.Length; x++)
					{
						dest[x] = blender.Blend(dest[x], colors.R, used.R * mask[x].R / 255f);
						dest[x] = blender.Blend(dest[x], colors.G, used.G * mask[x].G / 255f);
						dest[x] = blender.Blend(dest[x], colors.B, used.B * mask[x].B / 255f);
						dest[x] = blender.Blend(dest[x], colors.A, used.A * mask[x].A / 255f);
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
		PixelColorBlendingMode colorMode = PixelColorBlendingMode.Normal,
		CancellationToken token = default
	)
	{
		var blender = new PixelOperations<Rgba32>().GetPixelBlender(
			colorMode,
			PixelAlphaCompositionMode.SrcAtop
		);
		return ColorizeDiffuseTexture(diffuseImage, maskImage, colors, blender, token);
	}

	public static Image<Rgba32> ColorizeDiffuseTexture(
		Image<Rgba32> diffuseImage,
		Image<Rgba32> maskImage,
		MaskColors colors,
		CancellationToken token = default
	)
	{
		return ColorizeDiffuseTexture(
			diffuseImage,
			maskImage,
			colors,
			PixelColorBlendingMode.Normal,
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
