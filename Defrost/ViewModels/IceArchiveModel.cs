using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.Behaviors;
using CommunityToolkit.WinUI.Collections;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.Windows.Storage.Pickers;
using Windows.Storage;

namespace Pso2Tools.Defrost.ViewModels;

public partial class IceFileModel(IceDataFile file, int group) : ObservableObject
{
	public string Name => file.Name;
	public ReadOnlySpan<byte> Data => file.Data;
	public int Size => Data.Length;

	public int Group => group;

	public string SizeText => Data.Length.ToString("#,0");
}

public partial class IceArchiveModel : ObservableObject
{
	[ObservableProperty]
	public partial string? FilePath { get; set; }

	[ObservableProperty]
	public partial string FileName { get; set; } = "";

	public IceWrapper? Archive { get; set; }

	[ObservableProperty]
	public partial AdvancedCollectionView Files { get; private set; } = [];

	[ObservableProperty]
	public partial string FilterText { get; set; } = "";

	private string trimmedFilterText = "";

	[ObservableProperty]
	public partial bool IsLoading { get; private set; }

	[ObservableProperty]
	public partial int SelectedCount { get; set; }

	public bool HasSelection => SelectedCount > 0;

	[ObservableProperty]
	public partial long SelectedTotalFileSize { get; set; }

	private NotificationService NotificationService { get; }

	public IceArchiveModel(NotificationService notificationService)
	{
		NotificationService = notificationService;

		Files.Filter = x => FilterItem((IceFileModel)x);
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
				foreach (var f in Archive.GroupOne)
				{
					Files.Add(new IceFileModel(f, 1));
				}
				foreach (var f in Archive.GroupTwo)
				{
					Files.Add(new IceFileModel(f, 2));
				}
			}
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

	partial void OnSelectedCountChanged(int value)
	{
		OnPropertyChanged(nameof(HasSelection));
	}

	partial void OnFilterTextChanged(string value)
	{
		trimmedFilterText = value.Trim();
		Files.RefreshFilter();
	}

	private bool FilterItem(IceFileModel file)
	{
		if (trimmedFilterText == string.Empty)
		{
			return true;
		}

		return file.Name.Contains(FilterText, StringComparison.InvariantCultureIgnoreCase);
	}
}
