using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pso2Tools.Defrost.Controls;

public sealed partial class DataRow : Panel
{
	// TODO: use a WeakReference
	private DataTable? parentTable;

	private Thickness CellPadding => parentTable?.CellPadding ?? new Thickness();

	internal double[] ColumnWidths = [];

	public DataRow()
	{
		Unloaded += DataRow_Unloaded;
	}

	private void DataRow_Unloaded(object sender, RoutedEventArgs e)
	{
		parentTable?.Rows.Remove(this);
		parentTable = null;
	}

	private void InitializeParentConnection()
	{
		if (parentTable is null)
		{
			parentTable = this.FindAscendant<DataTable>();
			parentTable?.Rows.Add(this);
		}
	}

	protected override Size MeasureOverride(Size availableSize)
	{
		InitializeParentConnection();

		if (Children.Count == 0)
		{
			return availableSize;
		}

		var padding = CellPadding;
		var padWidth = padding.Left + padding.Right;
		var padHeight = padding.Top + padding.Bottom;

		foreach (var child in Children)
		{
			child.Measure(availableSize);
		}

		var newWidths = Children.Select(child => child.DesiredSize.Width + padWidth).ToArray();
		var maxHeight = Children.Max(child => child.DesiredSize.Height + padHeight);

		if (!newWidths.SequenceEqual(ColumnWidths))
		{
			ColumnWidths = newWidths;
			// Debug.WriteLine($"Row widths = [{string.Join(", ", ColumnWidths)}] {GetHashCode()}");

			parentTable?.CellColumnResized();
		}

		return new(parentTable?.DesiredSize.Width ?? availableSize.Width, maxHeight);
	}

	protected override Size ArrangeOverride(Size finalSize)
	{
		if (parentTable is null)
		{
			return finalSize;
		}

		var header = parentTable.Header;
		var padding = CellPadding;

		double padWidth = padding.Left + padding.Right;
		double padHeight = padding.Top + padding.Bottom;
		double x = 0;

		for (int i = 0; i < Children.Count; i++)
		{
			double width = 0;

			if (header is not null)
			{
				if (i < header.Children.Count && header.Children[i] is DataColumn col)
				{
					width = Math.Max(0, col.ActualWidth - padWidth);
				}
				else
				{
					width = 0;
				}
			}

			Children[i]
				.Arrange(
					new Rect(
						x: x + padding.Left,
						y: padding.Top,
						width: width,
						height: finalSize.Height - padHeight
					)
				);

			x += width + padWidth;
		}

		return new(x, finalSize.Height);
	}
}
