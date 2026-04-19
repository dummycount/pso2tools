using System;
using System.Collections.Generic;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using WinRT.Interop;

namespace Pso2Tools.Defrost;

public static class WindowHelper
{
	public static List<Window> ActiveWindows { get; } = [];

	public static void TrackWindow(Window window)
	{
		window.Closed += (s, e) =>
		{
			ActiveWindows.Remove(window);
		};
		ActiveWindows.Add(window);
	}
}
