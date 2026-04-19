using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Pso2Tools.Defrost.ViewModels;

public partial class ExtractModel : ObservableObject
{
	[ObservableProperty]
	public partial bool IsExtracting { get; set; }

	// Extract options

	[ObservableProperty]
	public partial string FileName { get; set; }

	public string Title =>
		IsExtracting ? $"{ProgressPercent:F0}% Extracting {FileName}" : $"Extract {FileName}";

	[ObservableProperty]
	public partial IceWrapper? Archive { get; set; }

	[ObservableProperty]
	public partial string DestinationPath { get; set; }

	public bool IsDestinationValid => !string.IsNullOrWhiteSpace(DestinationPath);

	// TODO: persist this?
	[ObservableProperty]
	public partial bool UseGroupFolders { get; set; } = true;

	[ObservableProperty]
	public partial bool IsGroupFoldersEnabled { get; private set; } = true;

	// TODO: persist this
	[ObservableProperty]
	public partial bool OpenFolderWhenDone { get; set; } = false;

	[ObservableProperty]
	public partial CollisionOption CollisionOption { get; set; } = CollisionOption.Ask;

	// Progress state
	[ObservableProperty]
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

		UseGroupFolders = value.GroupOne.Any() && value.GroupTwo.Any();

		// Force use group folders if there are files in both groups with the same name
		// (probably not a thing that can happen?)
		IsGroupFoldersEnabled = value.GroupOne.All(file1 =>
			!value.GroupTwo.Any(file2 => file1.Name == file2.Name)
		);
	}

	partial void OnDestinationPathChanged(string value)
	{
		OnPropertyChanged(nameof(IsDestinationValid));
	}

	partial void OnFileNameChanged(string value)
	{
		OnPropertyChanged(nameof(Title));
	}

	partial void OnIsExtractingChanged(bool oldValue, bool newValue)
	{
		OnPropertyChanged(nameof(Title));
	}

	partial void OnProgressPercentChanged(double value)
	{
		if (IsExtracting)
		{
			OnPropertyChanged(nameof(Title));
		}
	}
}
