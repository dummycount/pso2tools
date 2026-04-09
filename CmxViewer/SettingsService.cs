using System;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Collections;
using Microsoft.Windows.Storage;

namespace Pso2Tools.CmxViewer.ViewModels;

public partial class SettingsService : ObservableObject, ISettingsService
{
	// Use GetForUnpackaged()
	// https://github.com/microsoft/WindowsAppSDK/pull/6277
	private readonly ApplicationDataContainer settings = ApplicationData.GetDefault().LocalSettings;

	public string? Pso2BinPath
	{
		get => GetString();
		set => SetString(value);
	}

	public BodyType BodyTypeFilter
	{
		get => GetEnum(defaultVal: BodyType.All);
		set => SetEnum(value);
	}

	public GameVersion GameVersionFilter
	{
		get => GetEnum(defaultVal: GameVersion.All);
		set => SetEnum(value);
	}

	public SortProperty SortProperty
	{
		get => GetEnum(defaultVal: SortProperty.Id);
		set => SetEnum(value);
	}

	public SortDirection SortDirection
	{
		get => GetEnum(defaultVal: SortDirection.Ascending);
		set => SetEnum(value);
	}

	public SettingsService()
	{
		Pso2BinPath ??= GameFinder.FindPso2BinPath();
	}

	private string? GetString([CallerMemberName] string property = "")
	{
		return settings.Values[property] as string;
	}

	private void SetString(string? value, [CallerMemberName] string property = "")
	{
		if (settings.Values[property] is string str && str == value)
		{
			return;
		}

		settings.Values[property] = value;
		OnPropertyChanged(property);
	}

	private T GetEnum<T>(T defaultVal, [CallerMemberName] string property = "")
		where T : struct
	{
		if (settings.Values[property] is string str)
		{
			return Enum.TryParse<T>(str, true, out var result) ? result : defaultVal;
		}

		return defaultVal;
	}

	private void SetEnum<T>(T value, [CallerMemberName] string property = "")
		where T : struct
	{
		var valueStr = value.ToString();

		if (settings.Values[property] is string str && str == valueStr)
		{
			return;
		}

		settings.Values[property] = valueStr;
		OnPropertyChanged(property);
	}
}
