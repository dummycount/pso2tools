using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.Collections;

namespace Pso2Tools.CmxViewer.ViewModels;

public partial class ColorSetsModel : ObservableObject
{
	private readonly ICmxDatabase database;

	[ObservableProperty]
	public partial AdvancedCollectionView ColorSets { get; private set; } = [];

	[ObservableProperty]
	public partial string FilterText { get; set; } = "";

	private string trimmedFilterText = "";

	[ObservableProperty]
	public partial GameVersion GameVersion { get; set; } = GameVersion.All;

	[ObservableProperty]
	public partial BodyType BodyType { get; set; } = BodyType.All;

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
		if (!Filters.MatchesGameVersion(GameVersion, item.Id))
		{
			return false;
		}

		if (!Filters.MatchesBodyType(BodyType, item.Id))
		{
			return false;
		}

		if (trimmedFilterText == string.Empty)
		{
			return true;
		}

		// TODO: how much of a performance hit is Id.ToString()?

		return item.Names.Any(
				(names) => Filters.MatchesString(trimmedFilterText, names.En, names.Jp)
			) || Filters.MatchesString(trimmedFilterText, item.Id.ToString());
	}

	partial void OnFilterTextChanged(string value)
	{
		trimmedFilterText = value.Trim();
		ColorSets.RefreshFilter();
	}

	partial void OnGameVersionChanged(GameVersion value) => ColorSets.RefreshFilter();

	partial void OnBodyTypeChanged(BodyType value) => ColorSets.RefreshFilter();

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
