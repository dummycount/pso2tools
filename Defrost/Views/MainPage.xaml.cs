using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Pso2Tools.Defrost.ViewModels;

namespace Pso2Tools.Defrost.Views;

// TODO: drag and drop from list to explorer should copy selected files
// TODO: drag and drop from explorer to window should open ice file
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
}
