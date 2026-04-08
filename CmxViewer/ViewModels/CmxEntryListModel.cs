using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.Collections;

namespace Pso2Tools.CmxViewer.ViewModels;

public enum CmxEntrySort
{
	[Display(Name = "ID")]
	Id,

	[Display(Name = "Name (EN)")]
	NameEn,

	[Display(Name = "Name (JP)")]
	NameJp,
}

public partial class CmxEntryListModel : ObservableObject
{
	private readonly ICmxDatabase database;

	// TODO: make this a list of view models instead of a list of ICmxEntry?
	[ObservableProperty]
	public partial AdvancedCollectionView Objects { get; private set; } = [];

	[ObservableProperty]
	public partial CmxObjectType ObjectType { get; set; } = CmxObjectType.Basewear;

	[ObservableProperty]
	public partial CmxEntrySort SortProperty { get; set; } = CmxEntrySort.Id;

	[ObservableProperty]
	public partial SortDirection SortDirection { get; set; } = SortDirection.Ascending;

	[ObservableProperty]
	public partial string FilterText { get; set; } = "";

	private string trimmedFilterText = "";

	[ObservableProperty]
	public partial bool UsesGameVersion { get; private set; }

	[ObservableProperty]
	public partial GameVersionFilter GameVersion { get; set; } = GameVersionFilter.All;

	[ObservableProperty]
	public partial bool UsesGender { get; private set; }

	[ObservableProperty]
	public partial GenderFilter Gender { get; set; } = GenderFilter.All;

	public CmxEntryListModel(ICmxDatabase database)
	{
		this.database = database;

		Objects.Filter = x => FilterItem((ICmxEntry)x);

		UpdateUsedFilters();
		UpdateSort();
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
		UsesGender = Filters.UsesGender(ObjectType);
	}

	private bool FilterItem(ICmxEntry item)
	{
		if (UsesGameVersion && !Filters.MatchesVersion(GameVersion, item.Id))
		{
			return false;
		}

		if (UsesGender && !Filters.MatchesGender(Gender, item.Id))
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
		switch (SortProperty)
		{
			case CmxEntrySort.Id:
				Objects.SortDescriptions.Add(new(SortDirection, IdComparer.Instance));
				break;

			case CmxEntrySort.NameEn:
				Objects.SortDescriptions.Add(new(SortDirection, NameEnComparer.Instance));
				Objects.SortDescriptions.Add(new(SortDirection, IdComparer.Instance));
				break;

			case CmxEntrySort.NameJp:
				Objects.SortDescriptions.Add(new(SortDirection, NameJpComparer.Instance));
				Objects.SortDescriptions.Add(new(SortDirection, IdComparer.Instance));
				break;
		}

		Objects.RefreshSorting();
	}

	partial void OnFilterTextChanged(string value)
	{
		trimmedFilterText = value.Trim();
		Objects.RefreshFilter();
	}

	partial void OnGameVersionChanged(GameVersionFilter value) => Objects.RefreshFilter();

	partial void OnGenderChanged(GenderFilter value) => Objects.RefreshFilter();

	partial void OnObjectTypeChanged(CmxObjectType value) => UpdateUsedFilters();

	partial void OnSortPropertyChanged(CmxEntrySort value) => UpdateSort();

	partial void OnSortDirectionChanged(SortDirection value) => UpdateSort();

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
