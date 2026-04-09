using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pso2Tools.CmxViewer.ViewModels;
using Pso2Tools.CmxViewer.Views;

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

		// Services
		services.AddSingleton<SettingsService>();
		services.AddSingleton<ICmxDatabase>(x =>
		{
			var settings = x.GetRequiredService<SettingsService>();

			return new CmxDatabase(settings.Pso2BinPath);
		});

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
		Window = new MainWindow();
		Window.Activate();

		MainWindow.Navigate(typeof(CmxEntryListPage), CmxObjectType.Basewear);
		((NavigationViewItem)MainWindow.NavigationView.MenuItems[0]).IsSelected = true;
	}
}
