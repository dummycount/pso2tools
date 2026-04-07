using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.Collections;
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
public sealed partial class CmxEntryListPage : Page
{
	private readonly CmxEntryListModel? viewModel;

	public CmxEntryListPage()
	{
		viewModel = App.Current.Services.GetService<CmxEntryListModel>();

		InitializeComponent();

		// TODO: persist sort property and order in settings
		// TODO: persist filter toggles in settings, but only apply if object type uses them
	}

	protected override async void OnNavigatedTo(NavigationEventArgs e)
	{
		base.OnNavigatedTo(e);

		if (viewModel is not null)
		{
			if (e.Parameter is CmxObjectType objectType)
			{
				viewModel.ObjectType = objectType;
			}

			await viewModel.LoadAsync();

			Progress.IsActive = false;
		}
	}

	// Can't use two-way binding with Segmented because it reports SelectedValue as null sometimes
	// https://github.com/microsoft/microsoft-ui-xaml/issues/3268
	private void SortProperty_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (sender is Segmented control && control.SelectedValue is CmxEntrySort sort)
		{
			viewModel?.SortProperty = sort;
		}
	}

	private void SortDirection_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (sender is Segmented control && control.SelectedValue is SortDirection directon)
		{
			viewModel?.SortDirection = directon;
		}
	}

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
