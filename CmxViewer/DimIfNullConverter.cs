using System;
using Microsoft.UI.Xaml.Data;

namespace Pso2Tools.CmxViewer;

internal partial class DimIfNullConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		if (value is null)
		{
			return (parameter is double opacity) ? opacity : 0.36;
		}

		return 1.0;
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		throw new NotImplementedException();
	}
}
