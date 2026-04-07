using System;
using CommunityToolkit.WinUI.Collections;
using FluentIcons.Common;
using Microsoft.UI.Xaml.Data;

namespace Pso2Tools.CmxViewer;

internal partial class SortDirectionIconConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		return value switch
		{
			SortDirection.Ascending => Symbol.TextSortAscending,
			SortDirection.Descending => Symbol.TextSortDescending,
			_ => Symbol.ArrowSort,
		};
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		throw new NotImplementedException();
	}
}
