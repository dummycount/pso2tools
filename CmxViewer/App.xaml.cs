using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Pso2Tools.CmxViewer.ViewModels;
using UnluacNET;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pso2Tools.CmxViewer;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
	public static new App Current => (App)Application.Current;

	public IServiceProvider Services { get; }

	public Window? Window { get; set; }

	private static ServiceProvider ConfigureServices()
	{
		var services = new ServiceCollection();

		// Services
		services.AddSingleton<SettingsService>();
		services.AddSingleton<ICmxDatabase>(x =>
		{
			var settings = x.GetService<SettingsService>();

			return new CmxDatabase(settings?.Pso2BinPath);
		});

		// ViewModels
		services.AddTransient<CmxEntryListModel>();
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
	protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
	{
		Window = new MainWindow();
		Window.Activate();
	}
}
