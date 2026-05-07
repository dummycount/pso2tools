using System;
using Microsoft.UI.Xaml.Data;

namespace Pso2Tools.Defrost;

public partial class IndexToPageConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		if (value is int x)
		{
			return $"{x + 1}";
		}
		throw new NotImplementedException();
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		throw new NotImplementedException();
	}
}
