using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Pso2Tools.Defrost.Views
{
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
