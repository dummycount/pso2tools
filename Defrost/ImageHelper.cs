using System;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Pfim;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.ImageSharp.Processing.Processors.Filters;
using Windows.Graphics.Imaging;

namespace Pso2Tools.Defrost;

// TODO: remove Pfim once https://github.com/SixLabors/ImageSharp.Textures has a release
// and ImageSharp supports DDS natively.

public static class ImageHelper
{
	public static Image DdsBufferToImage(byte[] fileBytes)
	{
		using var stream = new MemoryStream(fileBytes);
		using var dds = Pfimage.FromStream(stream);

		return PfimageToImage(dds);
	}

	// Based on https://github.com/nickbabcock/Pfim/tree/master/src/Pfim.ImageSharp
	// MIT License
	public static Image PfimageToImage(IImage image)
	{
		byte[] data = image.Data;

		// Remove padding if needed
		var tightStride = image.Width * image.BitsPerPixel / 8;
		if (image.Stride != tightStride)
		{
			data = new byte[image.Height * tightStride];

			for (int i = 0; i < image.Height; i++)
			{
				Buffer.BlockCopy(image.Data, i * image.Stride, data, i * tightStride, tightStride);
			}
		}

		switch (image.Format)
		{
			case ImageFormat.Rgba32:
				return Image.LoadPixelData<Bgra32>(data, image.Width, image.Height);

			case ImageFormat.Rgb24:
				return Image.LoadPixelData<Bgr24>(data, image.Width, image.Height);

			case ImageFormat.Rgba16:
				return Image.LoadPixelData<Bgra4444>(data, image.Width, image.Height);

			case ImageFormat.R5g5b5:
			{
				for (int i = 1; i < data.Length; i += 2)
				{
					data[i] |= 128;
				}
				return Image.LoadPixelData<Bgra5551>(data, image.Width, image.Height);
			}

			case ImageFormat.R5g5b5a1:
				return Image.LoadPixelData<Bgra5551>(data, image.Width, image.Height);

			case ImageFormat.R5g6b5:
				return Image.LoadPixelData<Bgr565>(data, image.Width, image.Height);

			case ImageFormat.Rgb8:
				return Image.LoadPixelData<L8>(data, image.Width, image.Height);

			default:
				throw new NotImplementedException($"Unsupported format {image.Format}");
		}
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
