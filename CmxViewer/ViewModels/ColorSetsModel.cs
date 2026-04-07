using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.Collections;

namespace Pso2Tools.CmxViewer.ViewModels;

// TODO: add classic/NGS filter
// TODO: add gender filter

public partial class ColorSetsModel : ObservableObject
{
	private readonly ICmxDatabase database;

	[ObservableProperty]
	public partial AdvancedCollectionView ColorSets { get; private set; } = [];

	[ObservableProperty]
	public partial string FilterText { get; set; } = "";

	private string trimmedFilterText = "";

	[ObservableProperty]
	public partial GameVersionFilter GameVersion { get; set; } = GameVersionFilter.All;

	[ObservableProperty]
	public partial GenderFilter Gender { get; set; } = GenderFilter.All;

	public ColorSetsModel(ICmxDatabase database)
	{
		this.database = database;

		ColorSets.Filter = x => FilterItem((CmxColorSet)x);

		ColorSets.SortDescriptions.Add(new(SortDirection.Ascending, IdComparer.Instance));
	}

	[RelayCommand]
	public async Task LoadAsync()
	{
		var colorSets = await database.GetColorsAsync();

		using (ColorSets.DeferRefresh())
		{
			ColorSets.Clear();
			foreach (var set in colorSets)
			{
				ColorSets.Add(set);
			}
		}
	}

	private bool FilterItem(CmxColorSet item)
	{
		if (!Filters.MatchesVersion(GameVersion, item.Id))
		{
			return false;
		}

		if (!Filters.MatchesGender(Gender, item.Id))
		{
			return false;
		}

		if (trimmedFilterText == string.Empty)
		{
			return true;
		}

		return item.Names.Any(
			(names) =>
				FilterTextMatches(trimmedFilterText, names.En)
				|| FilterTextMatches(trimmedFilterText, names.Jp)
		);
	}

	private static bool FilterTextMatches(string filterText, string? value)
	{
		if (value == null)
		{
			return false;
		}

		return value.Contains(filterText, StringComparison.InvariantCultureIgnoreCase);
	}

	partial void OnFilterTextChanged(string value)
	{
		trimmedFilterText = value.Trim();
		ColorSets.RefreshFilter();
	}

	partial void OnGameVersionChanged(GameVersionFilter value) => ColorSets.RefreshFilter();

	partial void OnGenderChanged(GenderFilter value) => ColorSets.RefreshFilter();

	private class IdComparer : IComparer
	{
		public static readonly IComparer Instance = new IdComparer();

		public int Compare(object? x, object? y)
		{
			var idx = (x as CmxColorSet)?.Id ?? 0;
			var idy = (y as CmxColorSet)?.Id ?? 0;

			return idx - idy;
		}
	}
}
