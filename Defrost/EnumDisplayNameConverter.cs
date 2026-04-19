using System;
using Microsoft.UI.Xaml.Data;

namespace Pso2Tools.Defrost;

internal partial class EnumDisplayNameConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		if (value is Enum enumValue)
		{
			return enumValue.GetDisplayName();
		}

		return "(invalid)";
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		throw new NotImplementedException();
	}
}
