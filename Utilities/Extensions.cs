using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Pso2Tools;

public static partial class Extensions
{
	extension(string str)
	{
		public string RemovePrefix(string prefix) =>
			str.StartsWith(prefix) ? str[prefix.Length..] : str;

		public string ToSentenceCaseLower() =>
			CapitalWordStartRegex().Replace(str, m => m.Groups[0].Value.ToLower());
	}

	[GeneratedRegex(@"\b[A-Z](?=[a-z])")]
	private static partial Regex CapitalWordStartRegex();

	extension(Enum value)
	{
		public string GetDisplayName()
		{
			if (value.ToString() is string name)
			{
				return value
						.GetType()
						.GetMember(name)[0]
						.GetCustomAttribute<DisplayAttribute>()
						?.Name
					?? name;
			}

			return "";
		}
	}

	public static void Update<K, V>(this IDictionary<K, V> dict, IDictionary<K, V> other)
	{
		foreach (var (k, v) in other)
		{
			dict[k] = v;
		}
	}
}
