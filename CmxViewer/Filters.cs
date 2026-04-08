using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Pso2Tools.CmxViewer;

public enum GameVersionFilter
{
	[Display(Name = "All")]
	All,

	[Display(Name = "Classic")]
	Classic,

	[Display(Name = "NGS")]
	Ngs,
}

public enum GenderFilter
{
	[Display(Name = "All")]
	All,

	[Display(Name = "T1")]
	T1,

	[Display(Name = "T2")]
	T2,

	[Display(Name = "None")]
	NonGendered,
}

public static class Filters
{
	public static bool MatchesString(string filterText, string? value)
	{
		if (value == null)
		{
			return false;
		}

		return value.Contains(filterText, StringComparison.InvariantCultureIgnoreCase);
	}

	public static bool MatchesString(string filterText, IEnumerable<string?> values)
	{
		return values.Any(v => MatchesString(filterText, v));
	}

	public static bool MatchesString(string filterText, params string?[] values)
	{
		return MatchesString(filterText, values.AsEnumerable());
	}

	public static bool MatchesVersion(GameVersionFilter version, int itemId)
	{
		return version switch
		{
			GameVersionFilter.All => true,
			GameVersionFilter.Classic => !CmxObjectIds.IsNgs(itemId),
			GameVersionFilter.Ngs => CmxObjectIds.IsNgs(itemId),
			_ => true,
		};
	}

	public static bool MatchesGender(GenderFilter gender, int itemId)
	{
		return gender switch
		{
			GenderFilter.All => true,
			GenderFilter.T1 => CmxObjectIds.IsT1(itemId),
			GenderFilter.T2 => CmxObjectIds.IsT2(itemId),
			GenderFilter.NonGendered => CmxObjectIds.IsNonGendered(itemId),
			_ => true,
		};
	}

	public static bool UsesVersion(CmxObjectType objectType)
	{
		return objectType switch
		{
			CmxObjectType.Basewear => true,
			CmxObjectType.Innerwear => true,
			CmxObjectType.Outerwear => true,
			CmxObjectType.CastArms => true,
			CmxObjectType.CastBody => true,
			CmxObjectType.CastLegs => true,
			CmxObjectType.Face => true,
			CmxObjectType.Facepaint => true,
			CmxObjectType.Hair => true,
			CmxObjectType.Eye => true,
			CmxObjectType.Eyebrow => true,
			CmxObjectType.Eyelash => true,
			CmxObjectType.Bodypaint => true,
			CmxObjectType.Skin => true,
			CmxObjectType.Accessory => true,
			_ => false,
		};
	}

	public static bool UsesGender(CmxObjectType objectType)
	{
		return objectType switch
		{
			CmxObjectType.Basewear => true,
			CmxObjectType.Innerwear => true,
			CmxObjectType.Outerwear => true,
			CmxObjectType.Costume => true,
			CmxObjectType.CastArms => true,
			CmxObjectType.CastBody => true,
			CmxObjectType.CastLegs => true,
			CmxObjectType.Face => true,
			CmxObjectType.Bodypaint => true,
			CmxObjectType.Skin => true,
			_ => false,
		};
	}
}
