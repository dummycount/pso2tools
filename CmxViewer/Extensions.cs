using Windows.Storage.Pickers;

namespace Pso2Tools.CmxViewer;

internal static class Extensions
{
	public static void Initialize(this FolderPicker picker)
	{
		InitializeWithWindow(picker);
	}

	private static void InitializeWithWindow(object target)
	{
		var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Current.Window);
		WinRT.Interop.InitializeWithWindow.Initialize(target, hwnd);
	}
}
