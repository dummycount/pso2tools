using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

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
}
