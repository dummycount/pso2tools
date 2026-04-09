using System;
using CommunityToolkit.WinUI.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Storage.Pickers;
using Pso2Tools.CmxViewer.ViewModels;

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

	public SettingsPage()
	{
		database = App.Current.Services.GetRequiredService<ICmxDatabase>();
		settings = App.Current.Services.GetRequiredService<ISettingsService>();

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
}
