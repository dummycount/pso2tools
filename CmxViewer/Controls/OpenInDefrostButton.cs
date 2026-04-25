using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.System;

namespace Pso2Tools.CmxViewer.Controls;

public sealed partial class OpenInDefrostButton : Button
{
	public string FilePath
	{
		get { return (string)GetValue(FilePathProperty); }
		set { SetValue(FilePathProperty, value); }
	}

	public static readonly DependencyProperty FilePathProperty = DependencyProperty.Register(
		nameof(FilePath),
		typeof(string),
		typeof(OpenInDefrostButton),
		new PropertyMetadata("")
	);

	public string ExtractSuffix
	{
		get { return (string)GetValue(ExtractSuffixProperty); }
		set { SetValue(ExtractSuffixProperty, value); }
	}

	public static readonly DependencyProperty ExtractSuffixProperty = DependencyProperty.Register(
		nameof(ExtractSuffix),
		typeof(string),
		typeof(OpenInDefrostButton),
		new PropertyMetadata("")
	);

	public OpenInDefrostButton()
	{
		DefaultStyleKey = typeof(OpenInDefrostButton);
	}

	private async void OpenInDefrostButton_Click(object sender, RoutedEventArgs e)
	{
		if (!ProtocolHandler.IsRegistered("pso2defrost"))
		{
			await ShowNotRegisteredError();
			return;
		}

		var uri = $"pso2defrost://file/{FilePath}";

		if (!string.IsNullOrEmpty(ExtractSuffix))
		{
			uri += $"?suffix= {ExtractSuffix}";
		}

		await Launcher.LaunchUriAsync(new Uri(uri, UriKind.Absolute));
	}

	protected override void OnApplyTemplate()
	{
		Click -= OpenInDefrostButton_Click;
		base.OnApplyTemplate();
		Click += OpenInDefrostButton_Click;
	}

	private async Task ShowNotRegisteredError()
	{
		var dialog = new ContentDialog
		{
			Title = "Cannot open Defrost",
			Content =
				"The Defrost link handler is not registered. Open Defrost, open settings, and select \"Register link handler\" to enable opening files in Defrost.",
			CloseButtonText = "OK",
			XamlRoot = XamlRoot,
		};

		await dialog.ShowAsync();
	}
}
