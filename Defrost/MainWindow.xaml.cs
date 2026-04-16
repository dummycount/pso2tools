using System;
using Microsoft.UI.Xaml;
using Pso2Tools.Defrost.ViewModels;
using Windows.Graphics;
using Windows.Win32;
using WinRT.Interop;

namespace Pso2Tools.Defrost;

// TODO: drag and drop from list to explorer should copy selected files
// TODO: drag and drop from explorer to window should open ice file
// TODO: add previews for image files in a right side panel, summary data for other file types?

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

		SetTitleBar(TitleBar);
		ExtendsContentIntoTitleBar = true;

		this.notificationService.NotificationQueue = NotificationQueue;
	}

	private void SetInitialSize()
	{
		Resize(800, 600);
	}

	private double GetDisplayScale()
	{
		var hwnd = WindowNative.GetWindowHandle(this);
		var dpi = PInvoke.GetDpiForWindow(new Windows.Win32.Foundation.HWND(hwnd));

		return dpi / 96.0;
	}

	private void Resize(int width, int height)
	{
		var scale = GetDisplayScale();

		AppWindow.Resize(GetSize(width, height, scale));
	}

	private static SizeInt32 GetSize(int width, int height, double scale)
	{
		return new SizeInt32(
			_Width: (int)Math.Round(width * scale),
			_Height: (int)Math.Round(height * scale)
		);
	}

	// TODO: set a minimum size
	// https://github.com/microsoft/WinUI-Gallery/blob/main/WinUIGallery/Helpers/Win32WindowHelper.cs#L25
}
