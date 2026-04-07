using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using Windows.Storage;

namespace Pso2Tools.CmxViewer.ViewModels;

public partial class SettingsService : ObservableObject
{
	private readonly ApplicationDataContainer settings = ApplicationData.Current.LocalSettings;

	public string? Pso2BinPath
	{
		get => Get<string>();
		set => Set(value);
	}

	public SettingsService()
	{
		Pso2BinPath ??= GameFinder.FindPso2BinPath();
	}

	private T? Get<T>([CallerMemberName] string property = "")
		where T : class
	{
		return settings.Values[property] as T;
	}

	private void Set<T>(T? value, [CallerMemberName] string property = "")
		where T : class
	{
		if (settings.Values[property] != value)
		{
			settings.Values[property] = value;
			OnPropertyChanged(property);
		}
	}
}
