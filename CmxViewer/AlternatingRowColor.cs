using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Pso2Tools.CmxViewer;

public class AlternatingRowColor
{
	private readonly Brush alternateRowColorBrush;

	public AlternatingRowColor()
	{
		alternateRowColorBrush = (Brush)App.Current.Resources["AlternateRowColorBrush"];
	}

	public void Apply(ContainerContentChangingEventArgs args)
	{
		var root = args.ItemContainer.ContentTemplateRoot;
		var brush = args.ItemIndex % 2 == 0 ? null : alternateRowColorBrush;

		switch (root)
		{
			case Panel panel:
				panel.Background = brush;
				break;

			case Border border:
				border.Background = brush;
				break;
		}
	}
}
