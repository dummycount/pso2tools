using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace Pso2Tools.Defrost.Controls;

public sealed partial class DataTableHeader : Panel
{
	// TODO: use a WeakReference
	private DataTable? parentTable;

	private Thickness CellPadding => parentTable?.CellPadding ?? new Thickness();

	private double[] ColumnWidths = [];

	public DataTableHeader()
	{
		Unloaded += DataTableHeader_Unloaded;
	}

	private void DataTableHeader_Unloaded(object sender, RoutedEventArgs e)
	{
		parentTable = null;
	}

	private void InitializeParentConnection()
	{
		parentTable ??= this.FindAscendant<DataTable>();
	}

	protected override Size MeasureOverride(Size availableSize)
	{
		InitializeParentConnection();

		var columns = Children.Where(static e => e is DataColumn).Cast<DataColumn>().ToArray();
		var padding = CellPadding;

		double widthUsed = 0;
		double maxHeight = 0;

		if (parentTable is not null)
		{
			var maxCellWidths = Enumerable
				.Range(0, columns.Length)
				.Select(parentTable.GetCellColumnWidth)
				.ToArray();

			var newWidths = new double[columns.Length];

			foreach (var (i, column) in columns.Index())
			{
				column.Padding = padding;

				column.Measure(
					new Size(Math.Max(availableSize.Width - widthUsed, 0), availableSize.Height)
				);

				var width = Math.Max(column.DesiredSize.Width, maxCellWidths[i]);
				maxHeight = Math.Max(column.DesiredSize.Height, maxHeight);

				newWidths[i] = width;
				widthUsed += width;
			}

			if (!newWidths.SequenceEqual(ColumnWidths))
			{
				ColumnWidths = newWidths;
				// Debug.WriteLine($"Header widths = [{string.Join(", ", ColumnWidths)}]");

				parentTable.HeaderColumnResized();
			}
		}

		return new Size(widthUsed, maxHeight);
	}

	protected override Size ArrangeOverride(Size finalSize)
	{
		var columns = Children.Where(static e => e is DataColumn).Cast<DataColumn>();

		double x = 0;

		foreach (var (i, column) in columns.Index())
		{
			var width = i < ColumnWidths.Length ? ColumnWidths[i] : 0;
			width = Math.Max(column.DesiredSize.Width, width);

			column.Arrange(new Rect(x, 0, width, finalSize.Height));

			x += width;
		}

		return finalSize;
	}
}
