using System.Runtime.InteropServices;
using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Helpers.Readers;

namespace Pso2Tools;

public struct Color
{
	public byte B;
	public byte G;
	public byte R;
	public byte A;

	public static bool operator ==(Color left, Color right)
	{
		return left.R == right.R && left.G == right.G && left.B == right.B && left.A == right.A;
	}

	public static bool operator !=(Color left, Color right)
	{
		return !(left == right);
	}

	public override readonly bool Equals(object? obj)
	{
		return (obj is Color color) && this == color;
	}

	public override readonly int GetHashCode()
	{
		return HashCode.Combine(B, G, R, A);
	}
}

public struct ItemColors
{
	public int Id { get; set; }
	public Color Outerwear1 { get; set; }
	public Color Outerwear2 { get; set; }
	public Color Basewear1 { get; set; }
	public Color Basewear2 { get; set; }
	public Color Innerwear1 { get; set; }
	public Color Innerwear2 { get; set; }

	public static (Color, Color) GetOuterwearColors(ItemColors colors) =>
		(colors.Outerwear1, colors.Outerwear2);

	public static (Color, Color) GetBasewearColors(ItemColors colors) =>
		(colors.Basewear1, colors.Basewear2);

	public static (Color, Color) GetInnerwearColors(ItemColors colors) =>
		(colors.Innerwear1, colors.Innerwear2);
}

// Guessing what CCL stands for
public class CharacterColorList : AquaCommon
{
	public Dictionary<int, ItemColors> ColorSets { get; } = [];

	public override string[] GetEnvelopeTypes() => [];

	public CharacterColorList(byte[] file)
		: base(file) { }

	public CharacterColorList(BufferedStreamReaderBE<MemoryStream> streamReader)
		: base(streamReader) { }

	public override void ReadNIFLFile(BufferedStreamReaderBE<MemoryStream> sr, int offset)
	{
		sr.Seek(offset + 0x14, SeekOrigin.Begin);

		var arrayBytes = rel0.REL0DataStart - 0x14;
		var itemBytes = Marshal.SizeOf<ItemColors>();

		if (arrayBytes % itemBytes != 0)
		{
			throw new InvalidDataException("Array size is incorrect");
		}

		var arrayCount = arrayBytes / itemBytes;

		for (int i = 0; i < arrayCount; i++)
		{
			var set = sr.Read<ItemColors>();
			ColorSets[set.Id] = set;
		}
	}

	public static CharacterColorList Load(string pso2BinPath)
	{
		var ice = IceWrapper.Load(
			Path.Join(pso2BinPath, "data/win32/11f916ecb1c7bddfb50ad879154e9e73")
		);

		var ccl = ice.FindByName("pl_default_color.ccl").First();

		return new CharacterColorList(ccl.Data.ToArray());
	}
}
