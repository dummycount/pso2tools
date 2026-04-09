using System;
using System.Linq;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Pso2Tools.CmxViewer.Views;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pso2Tools.CmxViewer;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
	public NavigationView NavigationView => NavView;

	public MainWindow()
	{
		InitializeComponent();

		ExtendsContentIntoTitleBar = true;
		SetTitleBar(TitleBar);

		var database = App.Current.Services.GetRequiredService<ICmxDatabase>();
		database.LoadFailed += Database_LoadFailed;
	}

	public void Navigate(
		Type pageType,
		object? parameter = null,
		NavigationTransitionInfo? infoOverride = null
	)
	{
		NavFrame.Navigate(pageType, parameter, infoOverride);
	}

	public void GoBack()
	{
		NavFrame.GoBack();
	}

	private NavigationViewItem? FindNavViewItem(string tag)
	{
		return NavView.FindDescendant<NavigationViewItem>(item => tag.Equals(item.Tag));
	}

	private void TitleBar_BackRequested(TitleBar sender, object args)
	{
		GoBack();
	}

	private void NavView_SelectionChanged(
		NavigationView sender,
		NavigationViewSelectionChangedEventArgs args
	)
	{
		if (args.IsSettingsSelected)
		{
			NavFrame.Navigate(typeof(SettingsPage));
		}
		else
		{
			var selectedItem = (NavigationViewItem)args.SelectedItem;
			NavView_Navigate(selectedItem);
		}
	}

	private void NavView_Navigate(NavigationViewItem item)
	{
		switch (item.Tag)
		{
			case "Colors":
				Navigate(typeof(ColorSetsPage));
				break;

			case string value:
				if (Enum.TryParse<CmxObjectType>(value, out var objectType))
				{
					Navigate(typeof(CmxEntryListPage), objectType);
				}
				break;
		}
	}

	private void Database_LoadFailed(object? sender, System.IO.ErrorEventArgs e)
	{
		DispatcherQueue.TryEnqueue(() =>
		{
			var button = new Button { Content = "Settings" };
			button.Click += SettingsButton_Click;

			var notification = new Notification
			{
				Title = "Failed to load CMX",
				Message = e.GetException().Message,
				Severity = InfoBarSeverity.Error,
				ActionButton = button,
			};

			NotificationQueue.Show(notification);
		});
	}

	private void SettingsButton_Click(object sender, RoutedEventArgs e)
	{
		NavView.SelectedItem = NavView.SettingsItem;

		NotificationQueue.Clear();
	}

	private void NavView_Loaded(object sender, RoutedEventArgs e)
	{
		NavView.SelectedItem = FindNavViewItem(CmxObjectType.Basewear.ToString());
	}
}
