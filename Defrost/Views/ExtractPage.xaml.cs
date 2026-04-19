using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Storage.Pickers;
using Pso2Tools.Defrost.ViewModels;

namespace Pso2Tools.Defrost.Views;

public sealed partial class ExtractPage : Page
{
	private Window? window;
	public Window? Window
	{
		get => window;
		set
		{
			window = value;
			UpdateTitle();
		}
	}

	private readonly ExtractModel viewModel;

	private CancellationTokenSource? cancellationTokenSource;

	public ExtractPage()
	{
		viewModel = App.Current.Services.GetRequiredService<ExtractModel>();

		InitializeViewModel();
		InitializeComponent();

		viewModel.PropertyChanged += ViewModel_PropertyChanged;
	}

	private void InitializeViewModel()
	{
		var archiveModel = App.Current.Services.GetRequiredService<IceArchiveModel>();
		viewModel.FileName = archiveModel.FileName;
		viewModel.Archive = archiveModel.Archive;

		// TODO: add a way on launch to set the extraction suffix, e.g. " Model (HQ)"
		// so opening an ICE file from CMX Viewer and then extracting it gives it a
		// more identifiable name.
		var destName = archiveModel.FileName + ".ice";
		var destDir = GetDestinationDirectory(archiveModel.FilePath);

		viewModel.DestinationPath = Path.Join(destDir, destName);
	}

	private void UpdateTitle()
	{
		window?.AppWindow?.Title = viewModel.Title;
	}

	private static string GetDestinationDirectory(string? archivePath)
	{
		var result = Path.GetDirectoryName(archivePath);

		if (string.IsNullOrEmpty(result))
		{
			return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
		}

		return result;
	}

	private void ViewModel_PropertyChanged(
		object? sender,
		System.ComponentModel.PropertyChangedEventArgs e
	)
	{
		switch (e.PropertyName)
		{
			case nameof(viewModel.Title):
				UpdateTitle();
				break;
		}
	}

	private async void Cancel_Click(object sender, RoutedEventArgs e)
	{
		if (cancellationTokenSource is not null)
		{
			VisualStateManager.GoToState(this, "CancelDisabled", true);
			await cancellationTokenSource.CancelAsync();
		}

		Window?.Close();
		App.MainWindow.Activate();
	}

	private async void PickDestPath_Click(object sender, RoutedEventArgs e)
	{
		ArgumentNullException.ThrowIfNull(Window);

		var picker = new FolderPicker(Window.AppWindow.Id);

		var result = await picker.PickSingleFolderAsync();

		if (result is not null)
		{
			viewModel.DestinationPath = result.Path;
		}
	}

	private async void Confirm_Click(object sender, RoutedEventArgs e)
	{
		if (viewModel.IsExtracting)
		{
			return;
		}

		viewModel.IsExtracting = true;
		VisualStateManager.GoToState(this, "Extracting", true);

		await ExtractAsync();

		if (viewModel.OpenFolderWhenDone)
		{
			Explorer.OpenFolder(viewModel.DestinationPath);
		}

		Window?.Close();
		App.MainWindow.Activate();
	}

	private async Task ExtractAsync()
	{
		// TODO: set progress in taskbar
		viewModel.ProgressPercent = 0;

		var destFolder = await FileOperations.GetOrCreateStorageFolderAsync(
			viewModel.DestinationPath
		);
		var items = GetExtractItems();
		var progress = new Progress<ExtractProgress>(progress =>
		{
			viewModel.ProgressPercent = progress.ProgressPercent;
			viewModel.ExtractingFile = progress.CurrentFileName;
			viewModel.TotalFileCount = progress.TotalCount;
			viewModel.CurrentFileIndex = Math.Min(progress.ExtractedCount + 1, progress.TotalCount);
		});

		cancellationTokenSource = new CancellationTokenSource();

		await FileOperations.ExtractAsync(
			destFolder,
			items,
			progress,
			viewModel.CollisionOption,
			ShowCollisionPrompt,
			cancellationTokenSource.Token
		);
	}

	private IEnumerable<ExtractItem> GetExtractItems()
	{
		ArgumentNullException.ThrowIfNull(viewModel.Archive);

		return viewModel.Archive.Files.Select(f => new ExtractItem
		{
			FileName = FileOperations.GetExtractPath(f.Name, f.Group, viewModel.UseGroupFolders),
			Data = f.Data.ToArray(),
		});
	}

	private async Task<CollisionResponse> ShowCollisionPrompt(
		CollisionData collision,
		CancellationToken cancellationToken
	)
	{
		var content = new ExtractConfirmDialog(collision.FileName);
		var dialog = new ContentDialog
		{
			XamlRoot = XamlRoot,
			Title = "Overwrite file?",
			PrimaryButtonText = "Overwrite",
			CloseButtonText = "Skip",
			DefaultButton = ContentDialogButton.None,
			Content = content,
		};

		var result = await dialog.ShowAsync();

		if (result == ContentDialogResult.Primary)
		{
			return content.DoForAll ? CollisionResponse.OverwriteAll : CollisionResponse.Overwrite;
		}
		else
		{
			return content.DoForAll ? CollisionResponse.SkipAll : CollisionResponse.Skip;
		}
	}
}
