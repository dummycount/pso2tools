using Microsoft.UI;
using Microsoft.UI.Xaml;
using Windows.UI;

namespace Pso2Tools.CmxViewer;

public class ThemeService(ISettingsService settings)
{
	public void Initialize()
	{
		RootTheme = settings.AppTheme;
	}

	public bool IsDarkTheme
	{
		get
		{
			var theme = RootTheme;
			return theme == ElementTheme.Default
				? Application.Current.RequestedTheme == ApplicationTheme.Dark
				: theme == ElementTheme.Dark;
		}
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

			if (App.MainWindow.Content is FrameworkElement rootElement)
			{
				rootElement.RequestedTheme = value;
			}

			// Workaround for title bar not updating button colors when theme is changed while app is running
			var isDark = IsDarkTheme;
			var foregroundColor = isDark ? Colors.White : Colors.Black;
			var backgroundHoverColor = isDark
				? Color.FromArgb(24, 255, 255, 255)
				: Color.FromArgb(24, 0, 0, 0);

			var titleBar = App.MainWindow.AppWindow.TitleBar;
			titleBar.ButtonForegroundColor = foregroundColor;
			titleBar.ButtonHoverForegroundColor = backgroundHoverColor;
			titleBar.ButtonHoverBackgroundColor = backgroundHoverColor;
		}
	}
}
