using System;
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
	public MainWindow()
	{
		InitializeComponent();

		ExtendsContentIntoTitleBar = true;
		SetTitleBar(TitleBar);

		// Begin loading as soon as possible
		var database = App.Current.Services.GetRequiredService<ICmxDatabase>();
		database.LoadAsync();
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
}
