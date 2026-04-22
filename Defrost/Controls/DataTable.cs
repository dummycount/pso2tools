using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Pso2Tools.Defrost.Controls;

public sealed partial class DataTable : ContentControl
{
	internal HashSet<DataRow> Rows { get; private set; } = [];

	public Thickness CellPadding
	{
		get { return (Thickness)GetValue(CellPaddingProperty); }
		set { SetValue(CellPaddingProperty, value); }
	}

	public static readonly DependencyProperty CellPaddingProperty = DependencyProperty.Register(
		nameof(CellPadding),
		typeof(Thickness),
		typeof(DataTable),
		new PropertyMetadata(new Thickness(), OnCellPaddingChanged)
	);

	public DataTableHeader Header
	{
		get { return (DataTableHeader)GetValue(HeaderProperty); }
		set { SetValue(HeaderProperty, value); }
	}

	public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
		nameof(Header),
		typeof(DataTableHeader),
		typeof(DataTable),
		new PropertyMetadata(null)
	);

	public DataTable()
	{
		DefaultStyleKey = typeof(DataTable);
	}

	internal void CellColumnResized()
	{
		Header?.InvalidateMeasure();
	}

	internal void HeaderColumnResized()
	{
		Header?.InvalidateArrange();

		foreach (var row in Rows)
		{
			row.InvalidateMeasure();
			row.InvalidateArrange();
		}
	}

	internal double GetCellColumnWidth(int index)
	{
		if (Rows.Count == 0)
		{
			return 0;
		}

		return Rows.Max(row => index < row.ColumnWidths.Length ? row.ColumnWidths[index] : 0);
	}

	private static void OnCellPaddingChanged(
		DependencyObject d,
		DependencyPropertyChangedEventArgs e
	)
	{
		var table = (DataTable)d;

		table.Header?.InvalidateMeasure();

		foreach (var row in table.Rows)
		{
			row.InvalidateMeasure();
		}
	}
}
