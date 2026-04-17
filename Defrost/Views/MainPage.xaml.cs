using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Pso2Tools.Defrost.ViewModels;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace Pso2Tools.Defrost.Views;

// TODO: add previews for image files in a right side panel, summary data for other file types?
// TODO: click in blank space below items should deselect items

public sealed partial class MainPage : Page
{
	private readonly IceArchiveModel viewModel;

	public MainPage()
	{
		viewModel = App.Current.Services.GetRequiredService<IceArchiveModel>();

		InitializeComponent();
	}

	private void FileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		viewModel.SelectedCount = FileList.SelectedItems.Count;

		viewModel.SelectedTotalFileSize = FileList
			.SelectedItems.Cast<IceFileModel>()
			.Aggregate(0, (total, file) => total + file.Size);
	}

	private async void Page_Drop(object sender, Microsoft.UI.Xaml.DragEventArgs e)
	{
		var item = (await e.DataView.GetStorageItemsAsync()).FirstOrDefault(item =>
			item.IsOfType(StorageItemTypes.File)
		);
		if (item is IStorageFile file)
		{
			await viewModel.LoadAsync(file);
		}
	}

	private void Page_DragEnter(object sender, Microsoft.UI.Xaml.DragEventArgs e)
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
	}

	private async void Copy_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
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
		return await Task.WhenAll(items.Select(CreateStreamedFileASync));
	}

	private static async Task<StorageFile> CreateStreamedFileASync(IceFileModel file)
	{
		return await StorageFile.CreateStreamedFileAsync(
			file.Name,
			(request) => OnStreamedDataRequested(request, file),
			null
		);
	}

	private static async void OnStreamedDataRequested(
		StreamedFileDataRequest request,
		IceFileModel file
	)
	{
		try
		{
			using var outputStream = request.AsStreamForWrite();
			await outputStream.WriteAsync(file.Data.ToArray());
			await outputStream.FlushAsync();
		}
		catch (Exception)
		{
			request.FailAndClose(StreamedFileFailureMode.Failed);
		}
	}
}
