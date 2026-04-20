using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pso2Tools.Defrost.Views
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class ExtractConfirmDialog : Page
	{
		public bool DoForAll
		{
			get { return (bool)GetValue(DoForAllProperty); }
			set { SetValue(DoForAllProperty, value); }
		}

		public static readonly DependencyProperty DoForAllProperty = DependencyProperty.Register(
			nameof(DoForAll),
			typeof(bool),
			typeof(ExtractConfirmDialog),
			new PropertyMetadata(false)
		);

		public string FilePath
		{
			get { return (string)GetValue(FilePathProperty); }
			set { SetValue(FilePathProperty, value); }
		}

		public static readonly DependencyProperty FilePathProperty = DependencyProperty.Register(
			nameof(FilePath),
			typeof(string),
			typeof(ExtractConfirmDialog),
			new PropertyMetadata("")
		);

		public ExtractConfirmDialog(string path)
		{
			FilePath = path;

			InitializeComponent();
		}
	}
}
