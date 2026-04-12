using System;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Behaviors;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Pso2Tools.CmxViewer.Views;

namespace Pso2Tools.CmxViewer;

public sealed partial class MainWindow : Window
{
	public NavigationView NavigationView => NavView;

	private ICmxDatabase Database { get; }
	private ISettingsService Settings { get; }

	public MainWindow(ICmxDatabase database, ISettingsService settings)
	{
		Database = database;
		Settings = settings;

		InitializeComponent();

		ExtendsContentIntoTitleBar = true;
		SetTitleBar(TitleBar);
		AppWindow.SetIcon("Assets/Toolbox.ico");

		Database.LoadFailed += Database_LoadFailed;

		if (Database.Pso2BinPath is null)
		{
			Database_Pso2BinPathNotSet();
		}
	}

	public void Navigate(
		Type pageType,
		object? parameter = null,
		NavigationTransitionInfo? infoOverride = null
	)
	{
		NavFrame.Navigate(pageType, parameter, infoOverride);
	}

	public void Navigate(string tag, NavigationTransitionInfo? infoOverride = null)
	{
		switch (tag)
		{
			case "Settings":
				Navigate(typeof(SettingsPage), null, infoOverride);
				break;

			case "Colors":
				Navigate(typeof(ColorSetsPage), null, infoOverride);
				break;

			case string value:
				if (Enum.TryParse<CmxObjectType>(value, out var objectType))
				{
					Navigate(typeof(CmxEntryListPage), objectType, infoOverride);
				}
				break;
		}
	}

	public void GoBack()
	{
		NavFrame.GoBack();
	}

	public void EnsureNavigationSelection(Type pageType, object? parameter = null)
	{
		EnsureNavigationSelection(GetTagFromPageData(pageType, parameter));
	}

	public void EnsureNavigationSelection(string tag)
	{
		var item = FindNavViewItem(tag);
		if (item is null)
		{
			return;
		}

		NavView.SelectedItem = item;

		var parent = item.FindAscendant<NavigationViewItem>();
		parent?.IsExpanded = true;

		if (tag != "Settings")
		{
			Settings.LastPage = tag;
		}
	}

	public void ShowNotification(Notification notification)
	{
		NotificationQueue.Clear();
		NotificationQueue.Show(notification);
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
			if (selectedItem.Tag is string tag)
			{
				Navigate(tag);
			}
		}
	}

	private static string GetTagFromPageData(Type type, object? parameter = null)
	{
		if (type == typeof(SettingsPage))
		{
			return "Settings";
		}

		if (type == typeof(ColorSetsPage))
		{
			return "Colors";
		}

		if (parameter is CmxObjectType objectType)
		{
			return objectType.ToString();
		}

		return "";
	}

	private Button GetSettingsButton()
	{
		var button = new Button { Content = "Settings" };
		button.Click += SettingsButton_Click;
		return button;
	}

	private void Database_LoadFailed(object? sender, System.IO.ErrorEventArgs e)
	{
		DispatcherQueue.TryEnqueue(() =>
		{
			ShowNotification(
				new Notification
				{
					Title = "Failed to load CMX",
					Message = e.GetException().Message,
					Severity = InfoBarSeverity.Error,
					ActionButton = GetSettingsButton(),
				}
			);
		});
	}

	private void Database_Pso2BinPathNotSet()
	{
		ShowNotification(
			new Notification
			{
				Title = "Failed to find pso2_bin folder",
				Message = "Go to settings and set the path to your PSO2 installation",
				Severity = InfoBarSeverity.Warning,
				ActionButton = GetSettingsButton(),
			}
		);
	}

	private void SettingsButton_Click(object sender, RoutedEventArgs e)
	{
		NavView.SelectedItem = NavView.SettingsItem;

		NotificationQueue.Clear();
	}

	private void NavView_Loaded(object sender, RoutedEventArgs e)
	{
		if (Settings.LastPage is null)
		{
			Navigate(typeof(CmxEntryListPage), CmxObjectType.Basewear);
		}
		else
		{
			Navigate(Settings.LastPage);
		}
	}
}
