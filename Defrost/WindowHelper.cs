using System.Collections.Generic;
using Microsoft.UI.Xaml;
using WinUIEx;

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

	public static void CenterWindow(Window window, Window parent)
	{
		// TODO: location is inaccurate when on secondary monitor at different display scale.
		var parentSize = parent.AppWindow.Size;
		var parentPos = parent.AppWindow.Position;

		var windowSize = window.AppWindow.Size;

		int x = parentPos.X + (parentSize.Width - windowSize.Width) / 2;
		int y = parentPos.Y + (parentSize.Height - windowSize.Height) / 2;

		window.Move(x, y);
	}
}
