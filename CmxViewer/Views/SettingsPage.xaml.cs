using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pso2Tools.CmxViewer.ViewModels;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pso2Tools.CmxViewer.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SettingsPage : Page
{
	private readonly ICmxDatabase database;
	private readonly SettingsService settings;

	public SettingsPage()
	{
		database = App.Current.Services.GetRequiredService<ICmxDatabase>();
		settings = App.Current.Services.GetRequiredService<SettingsService>();

		InitializeComponent();

		settings.PropertyChanged += Settings_PropertyChanged;
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
				break;
		}
	}

	private void DataPathResetButton_Click(object sender, RoutedEventArgs e)
	{
		settings.Pso2BinPath = GameFinder.FindPso2BinPath();
		// TODO: show an error if it couldn't be found
	}

	private async void DataPathButton_Click(object sender, RoutedEventArgs e)
	{
		var openPicker = new FolderPicker
		{
			SuggestedStartLocation = PickerLocationId.ComputerFolder,
		};
		openPicker.FileTypeFilter.Add("*");

		openPicker.Initialize();

		var folder = await openPicker.PickSingleFolderAsync();
		if (folder != null)
		{
			settings.Pso2BinPath = folder.Path;
		}
	}
}
