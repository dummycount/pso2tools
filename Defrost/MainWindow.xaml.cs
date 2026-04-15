using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pso2Tools.Defrost.ViewModels;
using Windows.Win32;
using WinRT.Interop;

namespace Pso2Tools.Defrost;

// TODO: drag and drop from list to explorer should copy selected files
// TODO: drag and drop from explorer to window should open ice file

public sealed partial class MainWindow : Window
{
	private readonly IceArchiveModel viewModel;
	private readonly NotificationService notificationService;

	public MainWindow(IceArchiveModel viewModel, NotificationService notificationService)
	{
		this.viewModel = viewModel;
		this.notificationService = notificationService;

		InitializeComponent();
		SetInitialSize();

		ExtendsContentIntoTitleBar = true;
		SetTitleBar(TitleBar);

		this.notificationService.NotificationQueue = NotificationQueue;
	}

	private void SetInitialSize()
	{
		Resize(800, 640);
	}

	private void Resize(int width, int height)
	{
		var hwnd = WindowNative.GetWindowHandle(this);
		var dpi = PInvoke.GetDpiForWindow(new Windows.Win32.Foundation.HWND(hwnd));

		var scalingFactor = dpi / 96.0;

		width = (int)(width * scalingFactor);
		height = (int)(height * scalingFactor);

		AppWindow.Resize(new Windows.Graphics.SizeInt32(width, height));
	}

	private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
	{
		System.ArgumentNullException.ThrowIfNull(e);
		var textbox = (TextBox)sender;
		var scrollViewer = textbox.FindDescendant<ScrollViewer>();
		scrollViewer?.ChangeView(
			horizontalOffset: scrollViewer.ExtentWidth,
			verticalOffset: null,
			zoomFactor: null,
			disableAnimation: true
		);
	}

	// TODO: set a minimum size
	// https://github.com/microsoft/WinUI-Gallery/blob/main/WinUIGallery/Helpers/Win32WindowHelper.cs#L25
}
