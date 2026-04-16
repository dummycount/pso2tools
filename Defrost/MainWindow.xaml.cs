using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Pso2Tools.Defrost.ViewModels;
using Windows.Graphics;
using Windows.Win32;
using WinRT.Interop;

namespace Pso2Tools.Defrost;

public sealed partial class MainWindow : Window
{
	private readonly IceArchiveModel viewModel;
	private readonly NotificationService notificationService;

	public MainWindow()
	{
		viewModel = App.Current.Services.GetRequiredService<IceArchiveModel>();
		notificationService = App.Current.Services.GetRequiredService<NotificationService>();

		InitializeComponent();
		SetInitialSize();

		SetTitleBar(TitleBar);
		ExtendsContentIntoTitleBar = true;

		notificationService.NotificationQueue = NotificationQueue;
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
