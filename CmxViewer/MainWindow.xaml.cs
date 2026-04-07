using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Pso2Tools.CmxViewer.Views;
using Windows.Foundation;
using Windows.Foundation.Collections;

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
	}

	private void TitleBar_BackRequested(TitleBar sender, object args)
	{
		NavFrame.GoBack();
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
		if (item.Tag is null)
		{
			return;
		}

		if ((string)item.Tag == "Colors")
		{
			NavFrame.Navigate(typeof(ColorSetsPage));
		}
		else if (Enum.TryParse<CmxObjectType>((string)item.Tag, out var objectType))
		{
			NavFrame.Navigate(typeof(CmxEntryListPage), objectType);
		}
	}
}
