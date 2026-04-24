using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;
using Pso2Tools.Defrost.ViewModels;
using Pso2Tools.Defrost.Views;
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

		RootGrid.ActualThemeChanged += (_, _) =>
			ThemeService.ApplySystemThemeToCaptionButtons(this, RootGrid.ActualTheme);

		viewModel.PropertyChanged += ViewModel_PropertyChanged;

		NavFrame.Navigate(typeof(MainPage));
	}

	public void OpenSettings()
	{
		NavFrame.Navigate(typeof(SettingsPage));
	}

	public void CloseSettings()
	{
		NavFrame.GoBack();
	}

	private void SetWindowProperties()
	{
		this.SetWindowSize(800, 600);

		manager.PersistenceId = "MainWindow";
		manager.MinWidth = 500;
		manager.MinHeight = 400;

		SetTitleBar(TitleBar);
		ExtendsContentIntoTitleBar = true;

		SetTitle();
	}

	private void SetTitle()
	{
		if (string.IsNullOrEmpty(viewModel.FileName))
		{
			AppWindow.Title = "Defrost";
		}
		else
		{
			AppWindow.Title = $"Defrost - {viewModel.FileName}";
		}
	}

	private void ViewModel_PropertyChanged(
		object? sender,
		System.ComponentModel.PropertyChangedEventArgs e
	)
	{
		switch (e.PropertyName)
		{
			case nameof(viewModel.FileName):
				SetTitle();
				break;
		}
	}
}
