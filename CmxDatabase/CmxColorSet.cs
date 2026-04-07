using System;
using System.Collections.Generic;
using System.Text;
using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData;

namespace Pso2Tools;

public class CmxColorSet(ItemColors colors)
{
	public ItemColors Colors { get; set; } = colors;
	public List<CmxNames> Names { get; set; } = [];

	public string NamesEn =>
		string.Join('\n', Names.Select(name => name.En ?? "").Where(name => name != string.Empty));

	public string NamesJp =>
		string.Join('\n', Names.Select(name => name.Jp ?? "").Where(name => name != string.Empty));

	public int Id => Colors.Id;

	public static IEnumerable<CmxColorSet> GetColorSets(
		CharacterColorList colors,
		IEnumerable<CmxNameDictionary> names
	)
	{
		foreach (var set in colors.ColorSets.Values)
		{
			var result = new CmxColorSet(set);

			foreach (var nameDict in names)
			{
				var name = nameDict[result.Id];
				if (name)
				{
					result.Names.Add(name);
				}
			}

			yield return result;
		}
	}

	private static readonly string[] NameCategories = ["basewear", "innerwear", "costume", "body"];

	public static IEnumerable<CmxColorSet> GetColorSets(CharacterColorList colors, PSO2Text text)
	{
		return GetColorSets(
			colors,
			NameCategories.Select(c => CmxNameDictionary.GetItemNames(text, c))
		);
	}
}
