using System.Collections.ObjectModel;
using AquaModelLibrary.Data.PSO2.Aqua;

namespace Pso2Tools;

public class CmxColorSet
{
	public readonly ItemColors Colors;
	public readonly IEnumerable<CmxNames> Names;

	public readonly string? NamesEn;
	public readonly string? NamesJp;

	public int Id => Colors.Id;

	public CmxColorSet(ItemColors colors, IEnumerable<CmxNames> names)
	{
		Colors = colors;
		Names = names;

		NamesEn = GetCombinedNames(name => name.En);
		NamesJp = GetCombinedNames(name => name.Jp);
	}

	private string? GetCombinedNames(Func<CmxNames, string?> selector)
	{
		var names = Names.Select(selector).Where(name => name is not null);

		if (names.Any())
		{
			return string.Join('\n', names);
		}

		return null;
	}

	public static IEnumerable<CmxColorSet> GetColorSets(
		CharacterColorList colors,
		IEnumerable<CmxNameDictionary> names
	)
	{
		foreach (var set in colors.ColorSets.Values)
		{
			List<CmxNames> mappedNames = [];

			foreach (var nameDict in names)
			{
				var name = nameDict[set.Id];
				if (name)
				{
					mappedNames.Add(name);
				}
			}

			yield return new CmxColorSet(set, mappedNames);
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
