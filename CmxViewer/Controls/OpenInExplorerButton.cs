using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Windows.System;

namespace Pso2Tools.CmxViewer.Controls;

public sealed partial class OpenInExplorerButton : Button
{
	public static readonly DependencyProperty FilePathProperty = DependencyProperty.Register(
		nameof(FilePath),
		typeof(string),
		typeof(OpenInExplorerButton),
		new PropertyMetadata("")
	);

	public string FilePath
	{
		get { return (string)GetValue(FilePathProperty); }
		set { SetValue(FilePathProperty, value); }
	}

	public OpenInExplorerButton()
	{
		DefaultStyleKey = typeof(OpenInExplorerButton);
	}

	private async void OpenInExplorerButton_Click(object sender, RoutedEventArgs e)
	{
		var file = await StorageFile.GetFileFromPathAsync(FilePath);
		if (file is null)
		{
			return;
		}

		var folder = await file?.GetParentAsync();
		if (folder is null)
		{
			return;
		}

		var options = new FolderLauncherOptions();
		options.ItemsToSelect.Add(file);

		await Launcher.LaunchFolderAsync(folder, options);
	}

	protected override void OnApplyTemplate()
	{
		Click -= OpenInExplorerButton_Click;
		base.OnApplyTemplate();
		Click += OpenInExplorerButton_Click;
	}
}
