using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Pso2Tools.Defrost.ViewModels;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace Pso2Tools.Defrost.Views;

// TODO: drag and drop from list to explorer should copy selected files
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
}
