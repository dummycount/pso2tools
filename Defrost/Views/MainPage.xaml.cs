using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Pso2Tools.Defrost.ViewModels;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using WinUIEx;

namespace Pso2Tools.Defrost.Views;

// TODO: add previews for image files in a right side panel, summary data for other file types?
// TODO: click in blank space below items should deselect items

public sealed partial class MainPage : Page
{
	private readonly IceArchiveModel viewModel;
	private Window? extractWindow;

	public MainPage()
	{
		viewModel = App.Current.Services.GetRequiredService<IceArchiveModel>();
		viewModel.PropertyChanged += ViewModel_PropertyChanged;

		InitializeComponent();
	}

	private void ViewModel_PropertyChanged(
		object? sender,
		System.ComponentModel.PropertyChangedEventArgs e
	)
	{
		switch (e.PropertyName)
		{
			case nameof(viewModel.Archive):
				// Close any open windows if the user loads a new archive.
				extractWindow?.Close();
				break;
		}
	}

	private void FileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		viewModel.SelectedCount = FileList.SelectedItems.Count;

		viewModel.SelectedTotalFileSize = FileList
			.SelectedItems.Cast<IceFileModel>()
			.Aggregate(0, (total, file) => total + file.Size);
	}

	private async void Page_Drop(object sender, DragEventArgs e)
	{
		var item = (await e.DataView.GetStorageItemsAsync()).FirstOrDefault(item =>
			item.IsOfType(StorageItemTypes.File)
		);
		if (item is IStorageFile file)
		{
			await viewModel.LoadAsync(file);
		}
	}

	private void Page_DragEnter(object sender, DragEventArgs e)
	{
		if (
			e.DataView.Contains(StandardDataFormats.StorageItems)
			&& e.DataView.GetStorageItemsAsync()
				.GetAwaiter()
				.GetResult()
				.Any(item => item.IsOfType(StorageItemTypes.File))
		)
		{
			e.AcceptedOperation = DataPackageOperation.Copy;
			e.DragUIOverride.Caption = "Open";
		}
	}

	// TODO: this is only triggered when dragging on the right side of items for some reason
	private async void FileList_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
	{
		var files = await CreateStreamedFilesForItemsAsync(e.Items.Cast<IceFileModel>());

		e.Data.SetStorageItems(files);
		e.Data.RequestedOperation = DataPackageOperation.Copy;
	}

	private async void Copy_Click(object sender, RoutedEventArgs e)
	{
		var files = await CreateStreamedFilesForItemsAsync(
			FileList.SelectedItems.Cast<IceFileModel>()
		);

		var package = new DataPackage();
		package.SetStorageItems(files);
		Clipboard.SetContent(package);
	}

	private static async Task<StorageFile[]> CreateStreamedFilesForItemsAsync(
		IEnumerable<IceFileModel> items
	)
	{
		return await Task.WhenAll(items.Select(item => item.CreateStreamedFileAsync()));
	}

	private void Extract_Click(object sender, RoutedEventArgs e)
	{
		if (extractWindow is null)
		{
			extractWindow = CreateExtractWindow();
			extractWindow.Closed += (s, e) =>
			{
				extractWindow = null;
			};
		}

		extractWindow.Activate();
	}

	private Window CreateExtractWindow()
	{
		var page = new ExtractPage();
		var window = new Window()
		{
			SystemBackdrop = new MicaBackdrop(),
			Content = page,
			Title = $"Extract {viewModel.FileName}",
			ExtendsContentIntoTitleBar = true,
		};

		page.Window = window;

		var presenter = OverlappedPresenter.CreateForDialog();
		window.AppWindow.SetPresenter(presenter);

		window.SetWindowSize(600, 382);
		WindowHelper.TrackWindow(window);

		return window;
	}
}
