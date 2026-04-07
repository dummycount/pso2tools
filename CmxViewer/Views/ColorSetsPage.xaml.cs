using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using CommunityToolkit.WinUI.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Pso2Tools.CmxViewer.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pso2Tools.CmxViewer.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ColorSetsPage : Page
{
	private readonly ColorSetsModel? viewModel;

	private readonly Brush alternateRowColorBrush;

	public ColorSetsPage()
	{
		viewModel = App.Current.Services.GetService<ColorSetsModel>();
		alternateRowColorBrush = (Brush)App.Current.Resources["AlternateRowColorBrush"];

		InitializeComponent();

		ColorSetList.ContainerContentChanging += ColorSetList_ContainerContentChanging;
	}

	private void ColorSetList_ContainerContentChanging(
		ListViewBase sender,
		ContainerContentChangingEventArgs args
	)
	{
		((Grid)args.ItemContainer.ContentTemplateRoot).Background =
			args.ItemIndex % 2 == 0 ? null : alternateRowColorBrush;
	}

	protected override async void OnNavigatedTo(NavigationEventArgs e)
	{
		base.OnNavigatedTo(e);

		if (viewModel is not null)
		{
			await viewModel.LoadAsync();

			Progress.IsActive = false;
		}
	}

	// Can't use two-way binding with Segmented because it reports SelectedValue as null sometimes
	// https://github.com/microsoft/microsoft-ui-xaml/issues/3268
	private void GameVersion_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (sender is Segmented control && control.SelectedValue is GameVersionFilter version)
		{
			viewModel?.GameVersion = version;
		}
	}

	private void Gender_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (sender is Segmented control && control.SelectedValue is GenderFilter gender)
		{
			viewModel?.Gender = gender;
		}
	}
}
