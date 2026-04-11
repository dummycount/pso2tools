using System.Diagnostics;
using System.IO;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

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

	private void OpenInExplorerButton_Click(object sender, RoutedEventArgs e)
	{
		var process = new ProcessStartInfo
		{
			FileName = "explorer",
			Arguments = $"/e, /select, \"{Path.GetFullPath(FilePath)}\"",
		};

		Process.Start(process);
	}

	protected override void OnApplyTemplate()
	{
		Click -= OpenInExplorerButton_Click;
		base.OnApplyTemplate();
		Click += OpenInExplorerButton_Click;
	}
}
