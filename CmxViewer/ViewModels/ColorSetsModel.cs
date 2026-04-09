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

	public ISettingsService Settings { get; }

	[ObservableProperty]
	public partial AdvancedCollectionView ColorSets { get; private set; } = [];

	[ObservableProperty]
	public partial string FilterText { get; set; } = "";

	private string trimmedFilterText = "";

	public ColorSetsModel(ICmxDatabase database, ISettingsService settings)
	{
		this.database = database;
		Settings = settings;

		Settings.PropertyChanged += Settings_PropertyChanged;

		ColorSets.Filter = x => FilterItem((CmxColorSet)x);

		UpdateSort();
	}

	private void Settings_PropertyChanged(
		object? sender,
		System.ComponentModel.PropertyChangedEventArgs e
	)
	{
		switch (e.PropertyName)
		{
			case nameof(Settings.BodyTypeFilter):
			case nameof(Settings.GameVersionFilter):
				ColorSets.RefreshFilter();
				break;

			case nameof(Settings.SortDirection):
			case nameof(Settings.SortProperty):
				UpdateSort();
				break;
		}
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
		if (!Filters.MatchesGameVersion(Settings.GameVersionFilter, item.Id))
		{
			return false;
		}

		if (!Filters.MatchesBodyType(Settings.BodyTypeFilter, item.Id))
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

	private void UpdateSort()
	{
		ColorSets.SortDescriptions.Clear();

		// Can't use SortDescription(propertyName, sortDirection) due to
		// https://github.com/CommunityToolkit/Windows/issues/642
		switch (Settings.SortProperty)
		{
			case SortProperty.Id:
				ColorSets.SortDescriptions.Add(new(Settings.SortDirection, IdComparer.Instance));
				break;

			case SortProperty.NameEn:
				ColorSets.SortDescriptions.Add(
					new(Settings.SortDirection, NameEnComparer.Instance)
				);
				ColorSets.SortDescriptions.Add(new(Settings.SortDirection, IdComparer.Instance));
				break;

			case SortProperty.NameJp:
				ColorSets.SortDescriptions.Add(
					new(Settings.SortDirection, NameJpComparer.Instance)
				);
				ColorSets.SortDescriptions.Add(new(Settings.SortDirection, IdComparer.Instance));
				break;
		}

		ColorSets.RefreshSorting();
	}

	partial void OnFilterTextChanged(string value)
	{
		trimmedFilterText = value.Trim();
		ColorSets.RefreshFilter();
	}

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

	private class NameEnComparer : IComparer
	{
		public static readonly IComparer Instance = new NameEnComparer();

		public int Compare(object? x, object? y)
		{
			var namex = (x as CmxColorSet)?.Names.FirstOrDefault()?.En;
			var namey = (y as CmxColorSet)?.Names.FirstOrDefault()?.En;

			if (string.IsNullOrEmpty(namex))
			{
				return string.IsNullOrEmpty(namey) ? 0 : 1;
			}

			if (string.IsNullOrEmpty(namey))
			{
				return -1;
			}

			return namex.CompareTo(namey, StringComparison.OrdinalIgnoreCase);
		}
	}

	private class NameJpComparer : IComparer
	{
		public static readonly IComparer Instance = new NameJpComparer();

		public int Compare(object? x, object? y)
		{
			var namex = (x as CmxColorSet)?.Names.FirstOrDefault()?.Jp;
			var namey = (y as CmxColorSet)?.Names.FirstOrDefault()?.Jp;

			if (string.IsNullOrEmpty(namex))
			{
				return string.IsNullOrEmpty(namey) ? 0 : 1;
			}

			if (string.IsNullOrEmpty(namey))
			{
				return -1;
			}

			return namex.CompareTo(namey, StringComparison.OrdinalIgnoreCase);
		}
	}
}
