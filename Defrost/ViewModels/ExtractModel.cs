using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Pso2Tools.Defrost.ViewModels;

public partial class ExtractModel(ISettingsService settings) : ObservableObject
{
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(Title))]
	public partial bool IsExtracting { get; set; }

	// Extract options

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(Title))]
	public partial string FileName { get; set; }

	public string Title =>
		IsExtracting ? $"{ProgressPercent:F0}% Extracting {FileName}" : $"Extract {FileName}";

	[ObservableProperty]
	public partial IceWrapper? Archive { get; set; }

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(IsDestinationValid))]
	public partial string DestinationPath { get; set; }

	public bool IsDestinationValid => !string.IsNullOrWhiteSpace(DestinationPath);

	[ObservableProperty]
	public partial bool UseGroupFolders { get; set; } = true;

	[ObservableProperty]
	public partial bool IsGroupFoldersEnabled { get; private set; } = true;

	// Progress state
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(Title))]
	public partial double ProgressPercent { get; set; }

	[ObservableProperty]
	public partial string ExtractingFile { get; set; }

	[ObservableProperty]
	public partial int CurrentFileIndex { get; set; }

	[ObservableProperty]
	public partial int TotalFileCount { get; set; }

	partial void OnArchiveChanged(IceWrapper? value)
	{
		if (value is null)
		{
			return;
		}

		// Force use group folders if there are files in both groups with the same name
		// (probably not a thing that can happen?)
		if (HaveFilesWithSameName())
		{
			IsGroupFoldersEnabled = false;
			UseGroupFolders = true;
		}
		else
		{
			IsGroupFoldersEnabled = true;
			UseGroupFolders = settings.ExtractGroupMode switch
			{
				ExtractGroupMode.Auto => value.GroupOne.Any() && value.GroupTwo.Any(),
				ExtractGroupMode.Always => true,
				ExtractGroupMode.Never => false,
				_ => false,
			};
		}
	}

	private bool HaveFilesWithSameName()
	{
		if (Archive is null)
		{
			return false;
		}

		return Archive.GroupOne.Any(file1 =>
			Archive.GroupTwo.Any(file2 => file1.Name == file2.Name)
		);
	}
}
