using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.Behaviors;
using CommunityToolkit.WinUI.Collections;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Storage.Pickers;
using Windows.Storage;

namespace Pso2Tools.Defrost.ViewModels;

public class IceFileModel(IceDataFile file)
{
	public string Name => file.Name;
	public ReadOnlySpan<byte> Data => file.Data;
	public int Size => Data.Length;
	public int Group => file.Group;

	public string SizeText => Data.Length.ToString("#,0");

	public Task<StorageFile> CreateStreamedFileAsync()
	{
		return FileOperations.CreateStreamedFileAsync(Name, Data.ToArray());
	}
}

public partial class IceArchiveModel : ObservableObject
{
	[ObservableProperty]
	public partial string? FilePath { get; set; }

	[ObservableProperty]
	public partial string FileName { get; set; } = "";

	[ObservableProperty]
	public partial IceWrapper? Archive { get; set; }

	[ObservableProperty]
	public partial AdvancedCollectionView Files { get; private set; } = [];

	[ObservableProperty]
	public partial bool IsLoading { get; private set; }

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(HasSelection))]
	public partial int SelectedCount { get; set; }

	public bool HasSelection => SelectedCount > 0;

	[ObservableProperty]
	public partial long SelectedTotalFileSize { get; set; }

	private NotificationService NotificationService { get; }

	public IceArchiveModel(NotificationService notificationService)
	{
		NotificationService = notificationService;
	}

	public async Task LoadAsync(string path)
	{
		var file = await StorageFile.GetFileFromPathAsync(path);
		if (file is not null)
		{
			await LoadAsync(file);
		}
		else
		{
			NotificationService.ShowNotification(
				new Notification
				{
					Title = $"Failed to open {path}",
					Message = "File does not exist",
					Severity = InfoBarSeverity.Error,
					Duration = TimeSpan.FromSeconds(10),
				}
			);
		}
	}

	public async Task LoadAsync(IStorageFile file)
	{
		try
		{
			NotificationService.Clear();
			Files.Clear();
			IsLoading = true;

			using (var stream = await file.OpenStreamForReadAsync())
			{
				Archive = await IceWrapper.LoadAsync(stream);
			}

			FilePath = file.Path;
			FileName = file.Name;

			using (Files.DeferRefresh())
			{
				foreach (var f in Archive.Files)
				{
					Files.Add(new IceFileModel(f));
				}
			}

			//Files = [.. Archive.Files.Select(f => new IceFileModel(f))];
		}
		catch (Exception ex)
		{
			NotificationService.ShowNotification(
				new Notification
				{
					Title = $"Failed to open {file.Name}",
					Message = ex.Message,
					Severity = InfoBarSeverity.Error,
					Duration = TimeSpan.FromSeconds(10),
				}
			);
		}
		finally
		{
			IsLoading = false;
		}
	}

	[RelayCommand]
	public async Task OpenAsync()
	{
		var picker = new FileOpenPicker(App.MainWindow.AppWindow.Id)
		{
			ViewMode = PickerViewMode.List,
		};

		var result = await picker.PickSingleFileAsync();
		if (result is null)
		{
			return;
		}

		await LoadAsync(result.Path);
	}
}
