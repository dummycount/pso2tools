using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pso2Tools.CmxViewer.Controls;

public sealed partial class ColorBlock : UserControl, INotifyPropertyChanged
{
	private Color color;
	public Color Color
	{
		get => color;
		set
		{
			if (value != color)
			{
				color = value;
				NotifyPropertyChanged();
				NotifyPropertyChanged(nameof(Hex));
			}
		}
	}

	public string Hex => $"#{Color.R:X2}{Color.G:X2}{Color.B:X2}";

	public event PropertyChangedEventHandler? PropertyChanged;

	public ColorBlock()
	{
		InitializeComponent();
	}

	private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	private void Button_Click(object sender, RoutedEventArgs e)
	{
		if (!DataPopup.IsOpen)
		{
			DataPopup.IsOpen = true;
		}
	}

	private void CopyButton_Click(object sender, RoutedEventArgs e)
	{
		var package = new DataPackage();
		package.SetText(Hex);
		Clipboard.SetContent(package);
	}
}
