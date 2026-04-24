using Microsoft.UI;
using Microsoft.UI.Xaml;
using Windows.UI;

namespace Pso2Tools.Defrost;

public class ThemeService(ISettingsService settings)
{
	public void Initialize()
	{
		RootTheme = settings.AppTheme;
	}

	public ElementTheme RootTheme
	{
		get
		{
			if (App.MainWindow.Content is FrameworkElement rootElement)
			{
				return rootElement.RequestedTheme;
			}

			return ElementTheme.Default;
		}
		set
		{
			settings.AppTheme = value;

			foreach (var window in WindowHelper.ActiveWindows)
			{
				if (window.Content is FrameworkElement rootElement)
				{
					rootElement.RequestedTheme = value;
				}
			}
		}
	}

	public static void ApplySystemThemeToCaptionButtons(Window window, ElementTheme theme)
	{
		// Workaround for title bar not updating button colors when theme is changed while app is running
		var isDark = theme == ElementTheme.Dark;
		var foregroundColor = isDark ? Colors.White : Colors.Black;
		var backgroundHoverColor = isDark
			? Color.FromArgb(24, 255, 255, 255)
			: Color.FromArgb(24, 0, 0, 0);

		var titleBar = window.AppWindow.TitleBar;
		titleBar.ButtonForegroundColor = foregroundColor;
		titleBar.ButtonHoverForegroundColor = backgroundHoverColor;
		titleBar.ButtonHoverBackgroundColor = backgroundHoverColor;
	}
}
