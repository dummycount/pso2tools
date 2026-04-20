using System.Collections.Generic;
using Microsoft.UI.Xaml;

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
