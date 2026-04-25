using System;
using System.IO;
using Config.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Pso2Tools.CmxViewer.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pso2Tools.CmxViewer;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
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

		services.AddSingleton<MainWindow>();

		// Services
		services.AddSingleton(x =>
		{
			var settingsFilePath = Path.Join(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"PSO2CMXViewer",
				"settings.ini"
			);

			var settings = SettingsServiceBuilder.Build<ISettingsService>(settingsFilePath);

			settings.Pso2BinPath ??= GameFinder.FindPso2BinPath();

			return settings;
		});
		services.AddSingleton<ICmxDatabase>(x =>
		{
			var settings = x.GetRequiredService<ISettingsService>();

			return new CmxDatabase(settings.Pso2BinPath);
		});
		services.AddSingleton<PagePersistenceService>();
		services.AddSingleton<ThemeService>();

		// ViewModels
		services.AddTransient<CmxEntryListModel>();
		services.AddTransient<CmxEntryModel>();
		services.AddTransient<ColorSetsModel>();

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
		Window = Services.GetRequiredService<MainWindow>();
		Window.Activate();

		Services.GetRequiredService<ThemeService>().Initialize();
	}
}
