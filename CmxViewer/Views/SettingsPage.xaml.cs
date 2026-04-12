using System;
using System.Diagnostics;
using System.Reflection;
using CommunityToolkit.WinUI.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Storage.Pickers;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pso2Tools.CmxViewer.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SettingsPage : Page
{
	private readonly ICmxDatabase database;
	private readonly ISettingsService settings;
	private readonly ThemeService theme;

	public string Version =>
		FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion ?? "";

	public SettingsPage()
	{
		database = App.Current.Services.GetRequiredService<ICmxDatabase>();
		settings = App.Current.Services.GetRequiredService<ISettingsService>();
		theme = App.Current.Services.GetRequiredService<ThemeService>();

		InitializeComponent();

		Loaded += SettingsPage_Loaded;
		settings.PropertyChanged += Settings_PropertyChanged;

		App.MainWindow.EnsureNavigationSelection(typeof(SettingsPage));
	}

	private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
	{
		ThemeMode.SelectedIndex = theme.RootTheme switch
		{
			ElementTheme.Default => 0,
			ElementTheme.Light => 1,
			ElementTheme.Dark => 2,
			_ => 0,
		};
	}

	private void ThemeMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		theme.RootTheme = ThemeMode.SelectedIndex switch
		{
			0 => ElementTheme.Default,
			1 => ElementTheme.Light,
			2 => ElementTheme.Dark,
			_ => ElementTheme.Default,
		};
	}

	private void Settings_PropertyChanged(
		object? sender,
		System.ComponentModel.PropertyChangedEventArgs e
	)
	{
		switch (e.PropertyName)
		{
			case nameof(settings.Pso2BinPath):
				database.Pso2BinPath = settings.Pso2BinPath;

				// Start loading immediately so you get notified if the path is wrong.
				database.LoadAsync();
				break;
		}
	}

	private void DataPathResetButton_Click(object sender, RoutedEventArgs e)
	{
		settings.Pso2BinPath = GameFinder.FindPso2BinPath();

		if (settings.Pso2BinPath is null)
		{
			App.MainWindow.ShowNotification(
				new Notification
				{
					Title = "Failed to find pso2_bin folder",
					Message = "Set the path to your PSO2 installation manually",
					Severity = InfoBarSeverity.Error,
					Duration = TimeSpan.FromSeconds(10),
				}
			);
		}
	}

	private async void DataPathButton_Click(object sender, RoutedEventArgs e)
	{
		var openPicker = new FolderPicker(App.MainWindow.AppWindow.Id)
		{
			SuggestedStartLocation = PickerLocationId.ComputerFolder,
		};

		var folder = await openPicker.PickSingleFolderAsync();
		if (folder != null)
		{
			settings.Pso2BinPath = folder.Path;
		}
	}

	private async void OpenRepoCard_Click(object sender, RoutedEventArgs e)
	{
		await Launcher.LaunchUriAsync(new Uri("https://github.com/dummycount/pso2tools"));
	}
}
