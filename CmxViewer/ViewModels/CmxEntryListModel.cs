using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.Collections;
using Reloaded.Memory.Extensions;

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
	public partial GameVersionFilter GameVersion { get; set; } = GameVersionFilter.All;

	[ObservableProperty]
	public partial GenderFilter Gender { get; set; } = GenderFilter.All;

	public CmxEntryListModel(ICmxDatabase database)
	{
		this.database = database;

		Objects.Filter = x => FilterItem((ICmxEntry)x);

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

	private bool FilterItem(ICmxEntry item)
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

		return FilterTextMatches(trimmedFilterText, item.Names.En)
			|| FilterTextMatches(trimmedFilterText, item.Names.Jp);
	}

	private static bool FilterTextMatches(string filterText, string? value)
	{
		if (value == null)
		{
			return false;
		}

		return value.Contains(filterText, StringComparison.InvariantCultureIgnoreCase);
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
