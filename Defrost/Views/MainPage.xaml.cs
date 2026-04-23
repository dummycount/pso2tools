using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
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

public sealed partial class MainPage : Page
{
	private const string WindowIdProperty = "SourceWindowId";

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

	private void FileList_SelectionChanged(object sender, ItemsViewSelectionChangedEventArgs e)
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
		// Don't accept drops from this window
		if (
			e.DataView.Properties.TryGetValue(WindowIdProperty, out var value)
			&& value is WindowId id
			&& id == App.MainWindow.AppWindow.Id
		)
		{
			return;
		}

		// Accept file drops from other windows
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

	private void FileList_ItemInvoked(ItemsView sender, ItemsViewItemInvokedEventArgs args)
	{
		// TODO: open the file somehow
	}

	private void FileList_PointerPressed(
		object sender,
		Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e
	)
	{
		// Clicking blank space in the list deselects everything.
		// TODO: delay the deselect until pointer release and cancel if pointer moved?
		if (
			e.OriginalSource is UIElement element
			&& element.FindAscendantOrSelf<ItemContainer>() is null
		)
		{
			FileList.DeselectAll();
		}
	}

	private void Item_PointerPressed(
		object sender,
		Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e
	)
	{
		// Workaround for issue where clicking a selected item doesn't reset the selection
		// to just that item.
		if (e.KeyModifiers == Windows.System.VirtualKeyModifiers.None)
		{
			if (sender is ItemContainer container && container.Tag is IceFileModel item)
			{
				if (FileList.SelectedItems.Contains(item))
				{
					FileList.DeselectAll();
					FileList.Select(viewModel.Files.IndexOf(item));
				}
			}
		}
	}

	private async void Item_DragStarting(UIElement sender, DragStartingEventArgs e)
	{
		var items = FileList.SelectedItems;

		// If the item being dragged isn't part of the selection, select it.
		if (sender is ItemContainer container && container.Tag is IceFileModel dragItem)
		{
			if (!items.Contains(dragItem))
			{
				FileList.DeselectAll();
				FileList.Select(viewModel.Files.IndexOf(dragItem));
				items = [dragItem];
			}
		}

		if (items.Count == 0)
		{
			return;
		}

		e.AllowedOperations = DataPackageOperation.Copy;
		e.Data.RequestedOperation = DataPackageOperation.Copy;
		e.Data.Properties[WindowIdProperty] = App.MainWindow.AppWindow.Id;

		// Drag UI must be set before awaiting anything or it won't work.
		e.DragUI.SetContentFromDataPackage();

		var files = await CreateStreamedFilesForItemsAsync(items.Cast<IceFileModel>());

		e.Data.SetStorageItems(files);
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
