using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.Behaviors;
using CommunityToolkit.WinUI.Collections;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Storage.Pickers;

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

	public string FileName => Path.GetFileName(FilePath) ?? "";

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
		try
		{
			NotificationService.Clear();
			Files.Clear();
			IsLoading = true;

			Archive = await IceWrapper.LoadAsync(path);
			FilePath = path;

			using (Files.DeferRefresh())
			{
				foreach (var file in Archive.GroupOne)
				{
					Files.Add(new IceFileModel(file, 1));
				}
				foreach (var file in Archive.GroupTwo)
				{
					Files.Add(new IceFileModel(file, 2));
				}
			}
		}
		catch (Exception ex)
		{
			NotificationService.ShowNotification(
				new Notification
				{
					Title = $"Failed to open {Path.GetFileName(path)}",
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

	partial void OnFilePathChanged(string? value)
	{
		OnPropertyChanged(nameof(FileName));
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
