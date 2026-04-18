using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Pso2Tools.Defrost.ViewModels;
using WinUIEx;

namespace Pso2Tools.Defrost;

public sealed partial class MainWindow : Window
{
	private readonly IceArchiveModel viewModel;
	private readonly NotificationService notificationService;
	private readonly WindowManager manager;

	public MainWindow()
	{
		viewModel = App.Current.Services.GetRequiredService<IceArchiveModel>();
		notificationService = App.Current.Services.GetRequiredService<NotificationService>();
		notificationService.NotificationQueue = NotificationQueue;
		manager = WindowManager.Get(this);

		InitializeComponent();
		SetWindowProperties();
	}

	private void SetWindowProperties()
	{
		this.SetWindowSize(800, 600);

		manager.PersistenceId = "MainWindow";
		manager.MinWidth = 500;
		manager.MinHeight = 400;

		SetTitleBar(TitleBar);
		ExtendsContentIntoTitleBar = true;
	}
}
