using System;
using System.Collections;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.Collections;

namespace Pso2Tools.CmxViewer.ViewModels;

public partial class CmxEntryListModel : ObservableObject
{
	private readonly ICmxDatabase database;

	public ISettingsService Settings { get; }

	// TODO: make this a list of view models instead of a list of ICmxEntry?
	[ObservableProperty]
	public partial AdvancedCollectionView Objects { get; private set; } = [];

	[ObservableProperty]
	public partial CmxObjectType ObjectType { get; set; } = CmxObjectType.Basewear;

	[ObservableProperty]
	public partial string FilterText { get; set; } = "";

	private string trimmedFilterText = "";

	[ObservableProperty]
	public partial bool UsesGameVersion { get; private set; }

	[ObservableProperty]
	public partial bool UsesBodyType { get; private set; }

	public CmxEntryListModel(ICmxDatabase database, ISettingsService settings)
	{
		this.database = database;
		Settings = settings;

		Settings.PropertyChanged += Settings_PropertyChanged;

		Objects.Filter = x => FilterItem((ICmxEntry)x);

		UpdateUsedFilters();
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
				Objects.RefreshFilter();
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
		var objects = await database.GetObjectsAsync(ObjectType);

		using (Objects.DeferRefresh())
		{
			Objects.Clear();
			foreach (var obj in objects)
			{
				Objects.Add(obj);
			}
		}
	}

	private void UpdateUsedFilters()
	{
		UsesGameVersion = Filters.UsesVersion(ObjectType);
		UsesBodyType = Filters.UsesBodyType(ObjectType);
	}

	private bool FilterItem(ICmxEntry item)
	{
		if (UsesGameVersion && !Filters.MatchesGameVersion(Settings.GameVersionFilter, item.Id))
		{
			return false;
		}

		if (UsesBodyType && !Filters.MatchesBodyType(Settings.BodyTypeFilter, item.Id))
		{
			return false;
		}

		if (trimmedFilterText == string.Empty)
		{
			return true;
		}

		// TODO: how much of a performance hit is Id.ToString()?

		return Filters.MatchesString(trimmedFilterText, item.Names.En, item.Names.Jp)
			|| Filters.MatchesString(trimmedFilterText, item.Id.ToString());
	}

	private void UpdateSort()
	{
		Objects.SortDescriptions.Clear();

		// Can't use SortDescription(propertyName, sortDirection) due to
		// https://github.com/CommunityToolkit/Windows/issues/642
		switch (Settings.SortProperty)
		{
			case SortProperty.Id:
				Objects.SortDescriptions.Add(new(Settings.SortDirection, IdComparer.Instance));
				break;

			case SortProperty.NameEn:
				Objects.SortDescriptions.Add(new(Settings.SortDirection, NameEnComparer.Instance));
				Objects.SortDescriptions.Add(new(Settings.SortDirection, IdComparer.Instance));
				break;

			case SortProperty.NameJp:
				Objects.SortDescriptions.Add(new(Settings.SortDirection, NameJpComparer.Instance));
				Objects.SortDescriptions.Add(new(Settings.SortDirection, IdComparer.Instance));
				break;
		}

		Objects.RefreshSorting();
	}

	partial void OnFilterTextChanged(string value)
	{
		trimmedFilterText = value.Trim();
		Objects.RefreshFilter();
	}

	partial void OnObjectTypeChanged(CmxObjectType value) => UpdateUsedFilters();

	private class IdComparer : IComparer
	{
		public static readonly IComparer Instance = new IdComparer();

		public int Compare(object? x, object? y)
		{
			var idx = (x as ICmxEntry)?.Id ?? 0;
			var idy = (y as ICmxEntry)?.Id ?? 0;

			return idx - idy;
		}
	}

	private class NameEnComparer : IComparer
	{
		public static readonly IComparer Instance = new NameEnComparer();

		public int Compare(object? x, object? y)
		{
			var namex = (x as ICmxEntry)?.Names.En;
			var namey = (y as ICmxEntry)?.Names.En;

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
			var namex = (x as ICmxEntry)?.Names.Jp;
			var namey = (y as ICmxEntry)?.Names.Jp;

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
