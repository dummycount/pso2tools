using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace Pso2Tools.CmxViewer;

internal partial class ColorToBrushConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		var color = (ArgbColor)value;

		return new SolidColorBrush(Windows.UI.Color.FromArgb(color.A, color.R, color.G, color.B));
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		throw new NotImplementedException();
	}
}
