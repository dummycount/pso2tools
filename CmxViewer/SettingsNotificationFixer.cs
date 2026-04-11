using System.ComponentModel;
using CommunityToolkit.WinUI.Collections;

namespace Pso2Tools.CmxViewer;

// Workaround for https://github.com/aloneguid/config/issues/151
public partial class SettingsNotificationFixer : ISettingsService, INotifyPropertyChanged
{
	private ISettingsService Settings { get; }

	public string? Pso2BinPath
	{
		get => Settings.Pso2BinPath;
		set
		{
			if (Settings.Pso2BinPath != value)
			{
				Settings.Pso2BinPath = value;
			}
		}
	}

	public string? LastPage
	{
		get => Settings.LastPage;
		set
		{
			if (Settings.LastPage != value)
			{
				Settings.LastPage = value;
			}
		}
	}

	public BodyType BodyTypeFilter
	{
		get => Settings.BodyTypeFilter;
		set
		{
			if (Settings.BodyTypeFilter != value)
			{
				Settings.BodyTypeFilter = value;
			}
		}
	}

	public GameVersion GameVersionFilter
	{
		get => Settings.GameVersionFilter;
		set
		{
			if (Settings.GameVersionFilter != value)
			{
				Settings.GameVersionFilter = value;
			}
		}
	}

	public SortProperty SortProperty
	{
		get => Settings.SortProperty;
		set
		{
			if (Settings.SortProperty != value)
			{
				Settings.SortProperty = value;
			}
		}
	}

	public SortDirection SortDirection
	{
		get => Settings.SortDirection;
		set
		{
			if (Settings.SortDirection != value)
			{
				Settings.SortDirection = value;
			}
		}
	}

	public SettingsNotificationFixer(ISettingsService settings)
	{
		Settings = settings;
		Settings.PropertyChanged += Settings_PropertyChanged;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	private void NotifyPropertyChanged(string? propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		NotifyPropertyChanged(e.PropertyName);
	}
}
