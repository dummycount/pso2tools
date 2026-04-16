using System;
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
		services.AddSingleton<IceArchiveModel>(); // shared between MainWindow and MainPage

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
	}
}
