using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Storage.Pickers;
using Pso2Tools.Defrost.ViewModels;
using Windows.System;

namespace Pso2Tools.Defrost.Views;

public sealed partial class SettingsPage : Page
{
	private readonly ISettingsService settings;
	private readonly SettingsModel viewModel;
	private readonly ThemeService theme;

	public SettingsPage()
	{
		settings = App.Current.Services.GetRequiredService<ISettingsService>();
		viewModel = App.Current.Services.GetRequiredService<SettingsModel>();
		theme = App.Current.Services.GetRequiredService<ThemeService>();

		InitializeComponent();

		Loaded += SettingsPage_Loaded;
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

	private void Close_Click(object sender, RoutedEventArgs e)
	{
		App.MainWindow.CloseSettings();
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

	private async void CustomExtractFolderBuffon_Click(object sender, RoutedEventArgs e)
	{
		var picker = new FolderPicker(App.MainWindow.AppWindow.Id);

		var result = await picker.PickSingleFolderAsync();

		if (result is not null)
		{
			settings.CustomExtractFolder = result.Path;
		}
	}

	private async void OpenRepoCard_Click(object sender, RoutedEventArgs e)
	{
		await Launcher.LaunchUriAsync(new Uri("https://github.com/dummycount/pso2tools"));
	}
}
