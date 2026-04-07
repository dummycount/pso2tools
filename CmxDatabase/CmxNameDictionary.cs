using AquaModelLibrary.Data.PSO2.Aqua;

namespace Pso2Tools;

public class CmxNameDictionary : Dictionary<int, CmxNames>
{
	public new CmxNames this[int key]
	{
		get
		{
			if (TryGetValue(key, out var value))
			{
				return value;
			}

			value = new CmxNames();
			Add(key, value);
			return value;
		}
		set { base[key] = value; }
	}

	public static CmxNameDictionary GetItemNames(
		PSO2Text text,
		string category,
		Dictionary<string, int>? lookupDict = null
	)
	{
		CmxNameDictionary result = [];

		var index = text.categoryNames.IndexOf(category);
		if (index < 0)
		{
			return [];
		}

		var listsByLanguage = text.text[index];

		foreach (var (language, textList) in listsByLanguage.Index())
		{
			foreach (var item in textList)
			{
				var name = item.name.Trim();
				var value = item.str.Trim();

				// Name may be a key into a lookup table
				if (lookupDict != null && lookupDict.TryGetValue(name, out var itemId))
				{
					result[itemId].SetByLanguage(language, value);
				}

				// Otherwise it is "No ####"
				if (int.TryParse(name.ToLowerInvariant().RemovePrefix("no"), out itemId))
				{
					result[itemId].SetByLanguage(language, value);
				}
			}
		}

		return result;
	}
}
