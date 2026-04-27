using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Config.Net;
using HelixToolkit.SharpDX;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Pso2Tools.Defrost.ViewModels;
using Pso2Tools.Defrost.ViewModels.Preview;

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
		services.AddSingleton(x =>
		{
			var settingsFilePath = Path.Join(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"PSO2Defrost",
				"settings.ini"
			);

			return new ConfigurationBuilder<ISettingsService>()
				.UseIniFile(settingsFilePath)
				.UseTypeParser(new ColorParser())
				.BuildWithFixedNotifications();
		});

		services.AddSingleton<NotificationService>();
		services.AddSingleton<ThemeService>();
		services.AddSingleton<EffectsManagerService>();

		// ViewModels
		services.AddSingleton<IceArchiveModel>(); // shared state between all windows
		services.AddTransient<ExtractModel>();
		services.AddTransient<SettingsModel>();
		services.AddTransient<PreviewModelAqp>();
		services.AddTransient<PreviewModelDds>();

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
		Window.Closed += Window_Closed;
		Window.Activate();

		WindowHelper.TrackWindow(Window);

		Services.GetRequiredService<ThemeService>().Initialize();
	}

	private async void Window_Closed(object sender, WindowEventArgs args)
	{
		// Close all remaining active windows when the main window is closed
		var activeWindows = new List<Window>(WindowHelper.ActiveWindows);
		foreach (var window in activeWindows)
		{
			// Don't try to close the window that's already closing
			if (!window.Equals(sender))
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

		await Services.GetRequiredService<EffectsManagerService>().DisposeAsync();
	}
}
