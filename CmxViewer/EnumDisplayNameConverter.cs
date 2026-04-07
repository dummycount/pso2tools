using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using Microsoft.UI.Xaml.Data;

namespace Pso2Tools.CmxViewer;

internal partial class EnumDisplayNameConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		if (value.ToString() is string name)
		{
			return value.GetType().GetMember(name)[0].GetCustomAttribute<DisplayAttribute>()?.Name
				?? name;
		}

		return "(invalid)";
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		throw new NotImplementedException();
	}
}
