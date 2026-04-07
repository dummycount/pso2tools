using System;
using System.Collections.Generic;
using System.Text;
using Zamboni;

namespace Pso2Tools;

public class FaceVariationDict
{
	public static Dictionary<string, int> Load(string pso2BinPath)
	{
		Dictionary<string, int> result = [];

		foreach (var file in GetFaceVariationLua(pso2BinPath))
		{
			result.Update(ParseFaceVariationLua(file.Data));
		}

		return result;
	}

	public static IEnumerable<IceDataFile> GetFaceVariationLua(string pso2BinPath)
	{
		var icePath = new IceFileInfo("ui_character_making.ice").Win32Path(pso2BinPath);

		var ice = IceWrapper.Load(icePath);

		return ice.FindByName("face_variation.cmp.lua");
	}

	public static Dictionary<string, int> ParseFaceVariationLua(ReadOnlySpan<byte> data)
	{
		var src = Encoding.UTF8.GetString(data);

		Dictionary<string, int> result = [];
		string? language = null;

		foreach (var line in src.Split("\n"))
		{
			if (language == null)
			{
				if (line.Contains("language"))
				{
					language = GetFirstString(line).ToString();
				}
			}
			else
			{
				if (line.Contains("crop_name"))
				{
					var name = GetFirstString(line);
					if (!name.IsEmpty)
					{
						result[language] = int.Parse(name[7..]);
					}
				}
			}
		}

		return result;
	}

	private static ReadOnlySpan<char> GetFirstString(ReadOnlySpan<char> line)
	{
		var start = line.IndexOf('"');
		if (start < 0)
		{
			return [];
		}

		line = line[start..];

		var end = line.IndexOf('"');
		if (end < 0)
		{
			return [];
		}

		return line[..end];
	}
}
