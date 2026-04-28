using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using SixLabors.ImageSharp.PixelFormats;

namespace Pso2Tools.Defrost;

public static class ColorExtensions
{
	extension(Windows.UI.Color color)
	{
		public Rgba32 ToRgba32() => new(color.R, color.G, color.B, color.A);

		public Vector4 ToVector() =>
			new(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
	}
}
