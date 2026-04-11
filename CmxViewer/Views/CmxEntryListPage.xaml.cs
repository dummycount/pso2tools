using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Collections;
using CommunityToolkit.WinUI.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Pso2Tools.CmxViewer.ViewModels;

namespace Pso2Tools.CmxViewer.Views;

public sealed partial class CmxEntryListPage : Page
{
	private readonly CmxEntryListModel viewModel;

	public CmxObjectType ObjectType => viewModel.ObjectType;

	public CmxEntryListPage()
	{
		viewModel = App.Current.Services.GetRequiredService<CmxEntryListModel>();

		InitializeComponent();

		EntryList.ItemClick += EntryList_ItemClick;
	}

	private void EntryList_ItemClick(object sender, ItemClickEventArgs e)
	{
		if (e.ClickedItem is ICmxEntry entry)
		{
			App.MainWindow.Navigate(
				typeof(CmxEntryPage),
				new CmxEntryPageParam(viewModel.ObjectType, entry),
				new SlideNavigationTransitionInfo()
				{
					Effect = SlideNavigationTransitionEffect.FromRight,
				}
			);
		}
	}

	protected override async void OnNavigatedTo(NavigationEventArgs e)
	{
		base.OnNavigatedTo(e);

		if (e.Parameter is CmxObjectType objectType)
		{
			viewModel.ObjectType = objectType;
		}

		App.MainWindow.EnsureNavigationSelection(typeof(CmxEntryListPage), viewModel.ObjectType);

		await viewModel.LoadAsync();

		Progress.IsActive = false;

		if (e.NavigationMode == NavigationMode.Back)
		{
			// TODO: restore scroll position and filter text
			// restore last-selected item to have keyboard focus
		}
	}

	protected override void OnNavigatedFrom(NavigationEventArgs e)
	{
		base.OnNavigatedFrom(e);

		var scroll = EntryList.FindDescendant<ScrollViewer>();
		var lastScrollOffset = scroll?.VerticalOffset;
		// TODO: save this and restore above
		// Also save other properties like filter text
	}

	// Can't use two-way binding with Segmented because it reports SelectedValue as null sometimes
	// https://github.com/microsoft/microsoft-ui-xaml/issues/3268
	private void SortProperty_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (sender is Segmented control && control.SelectedValue is SortProperty sort)
		{
			viewModel.Settings.SortProperty = sort;
		}
	}

	private void SortDirection_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (sender is Segmented control && control.SelectedValue is SortDirection directon)
		{
			viewModel.Settings.SortDirection = directon;
		}
	}

	private void GameVersion_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (sender is Segmented control && control.SelectedValue is GameVersion version)
		{
			viewModel.Settings.GameVersionFilter = version;
		}
	}

	private void BodyType_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (sender is Segmented control && control.SelectedValue is BodyType bodyType)
		{
			viewModel.Settings.BodyTypeFilter = bodyType;
		}
	}
}
