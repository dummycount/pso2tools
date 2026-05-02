using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AquaModelLibrary.Data.PSO2.Aqua;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Collections;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

public class Pso2TextRowModel(IEnumerable<PSO2Text.TextPair> items)
{
	public Pso2TextItemModel[] Items { get; set; } =
	[.. items.Select(item => new Pso2TextItemModel(item))];
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
	public partial bool IsLoading { get; set; }

	public async Task LoadFileAsync(IceDataFile file)
	{
		try
		{
			IsLoading = true;

			var sw = Stopwatch.StartNew();

			var data = file.Data.ToArray();
			var rawText = await Task.Run(() => new PSO2Text(data));

			Debug.WriteLine($"PSO2Text loaded in {sw.ElapsedMilliseconds} ms");

			sw = Stopwatch.StartNew();

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

			Debug.WriteLine($"Collection loaded in {sw.ElapsedMilliseconds} ms");

			SelectedCategory = Categories.FirstOrDefault() as Pso2TextCategoryModel;
		}
		finally
		{
			IsLoading = false;
		}
	}
}
