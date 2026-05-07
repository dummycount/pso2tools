using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AquaModelLibrary.Data.PSO2.Aqua;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Collections;

namespace Pso2Tools.Defrost.ViewModels.Preview;

public class Pso2TextItemModel
{
	public string Name { get; set; } = "";
	public string Text { get; set; } = "";

	public Pso2TextItemModel() { }

	public Pso2TextItemModel(PSO2Text.TextPair pair)
	{
		Name = pair.name;
		Text = pair.str;
	}
}

public partial class Pso2TextRowModel(IEnumerable<PSO2Text.TextPair> items) : ObservableObject
{
	public const int PageColumns = 2;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(PageItems))]
	public partial int CurrentPage { get; set; } = 0;

	public Pso2TextItemModel[] Items { get; set; } =
	[.. items.Select(item => new Pso2TextItemModel(item))];

	public IEnumerable<Pso2TextItemModel> PageItems
	{
		get
		{
			var start = CurrentPage * PageColumns;
			var end = start + PageColumns;

			return Items[start..end];
		}
	}
}

public class Pso2TextCategoryModel(string category, List<List<PSO2Text.TextPair>> items)
{
	public string Category { get; set; } = category;

	public Pso2TextRowModel[] Items { get; set; } = [.. ToRows(items)];

	private static IEnumerable<Pso2TextRowModel> ToRows(List<List<PSO2Text.TextPair>> subcategories)
	{
		var columns = subcategories.Count;
		var maxRows = subcategories.Max(cat => cat.Count);

		for (int row = 0; row < maxRows; row++)
		{
			var items = subcategories.Select(item => item.ElementAtOrDefault(row));

			yield return new Pso2TextRowModel(items);
		}
	}
}

public partial class PreviewModelText : ObservableObject
{
	public AdvancedCollectionView Categories { get; set; } = [];

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(TextRows))]
	public partial Pso2TextCategoryModel? SelectedCategory { get; set; }

	public IEnumerable<Pso2TextRowModel> TextRows => SelectedCategory?.Items ?? [];

	[ObservableProperty]
	public partial int CurrentPage { get; set; } = 0;

	[ObservableProperty]
	public partial int[] Pages { get; set; } = [];

	[ObservableProperty]
	public partial bool IsLoading { get; set; }

	public async Task LoadFileAsync(IceDataFile file)
	{
		try
		{
			IsLoading = true;

			var data = file.Data.ToArray();
			var rawText = await Task.Run(() => new PSO2Text(data));

			using (Categories.DeferRefresh())
			{
				Categories.Clear();
				foreach (
					var (category, text) in Enumerable.Zip(rawText.categoryNames, rawText.text)
				)
				{
					Categories.Add(new Pso2TextCategoryModel(category, text));
				}
			}

			SelectedCategory = Categories.FirstOrDefault() as Pso2TextCategoryModel;
		}
		finally
		{
			IsLoading = false;
		}
	}

	private void UpdateRowPages()
	{
		foreach (var row in TextRows)
		{
			row.CurrentPage = CurrentPage;
		}
	}

	partial void OnCurrentPageChanged(int value)
	{
		UpdateRowPages();
	}

	partial void OnSelectedCategoryChanged(Pso2TextCategoryModel? value)
	{
		if (TextRows.Count() == 0)
		{
			Pages = [0];
		}
		else
		{
			var pageCount = TextRows.Max(row =>
				(
					(row.Items.Length + Pso2TextRowModel.PageColumns - 1)
					/ Pso2TextRowModel.PageColumns
				)
			);

			Pages = Enumerable.Range(0, pageCount).ToArray();
		}

		CurrentPage = 0;

		UpdateRowPages();
	}
}
