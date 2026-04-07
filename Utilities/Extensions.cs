using System;
using System.Collections.Generic;
using System.Text;

namespace Pso2Tools;

public static class Extensions
{
	extension(string str)
	{
		public string RemovePrefix(string prefix) =>
			str.StartsWith(prefix) ? str[prefix.Length..] : str;
	}

	public static void Update<K, V>(this IDictionary<K, V> dict, IDictionary<K, V> other)
	{
		foreach (var (k, v) in other)
		{
			dict[k] = v;
		}
	}
}
