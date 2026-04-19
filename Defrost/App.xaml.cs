using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Pso2Tools.Defrost.ViewModels;

namespace Pso2Tools.Defrost;

public partial class App : Application
{
	public static new App Current => (App)Application.Current;

	internal static MainWindow MainWindow
	{
		get
		{
			var mainWindow = Current.Window as MainWindow;
			ArgumentNullException.ThrowIfNull(mainWindow);
			return mainWindow;
		}
	}

	public IServiceProvider Services { get; }

	public Window? Window { get; set; }

	private static ServiceProvider ConfigureServices()
	{
		var services = new ServiceCollection();

		// Services
		services.AddSingleton<NotificationService>();

		// ViewModels
		services.AddSingleton<IceArchiveModel>(); // shared state between all windows
		services.AddTransient<ExtractModel>();

		return services.BuildServiceProvider();
	}

	/// <summary>
	/// Initializes the singleton application object.  This is the first line of authored code
	/// executed, and as such is the logical equivalent of main() or WinMain().
	/// </summary>
	public App()
	{
		Services = ConfigureServices();
		InitializeComponent();
	}

	/// <summary>
	/// Invoked when the application is launched.
	/// </summary>
	/// <param name="args">Details about the launch request and process.</param>
	protected override void OnLaunched(LaunchActivatedEventArgs args)
	{
		Window = new MainWindow();
		Window.Activate();

		// Close all remaining active windows when the main window is closed
		Window.Closed += (s, e) =>
		{
			var activeWindows = new List<Window>(WindowHelper.ActiveWindows);
			foreach (var window in activeWindows)
			{
				// Don't try to close the window that's already closing
				if (!window.Equals(s))
				{
					try
					{
						window.Close();
					}
					catch
					{
						// Ignore any exceptions during cleanup
					}
				}
			}
		};
	}
}
