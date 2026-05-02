using System;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Collections;
using CommunityToolkit.WinUI.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Pso2Tools.CmxViewer.ViewModels;

namespace Pso2Tools.CmxViewer.Views;

public sealed partial class CmxEntryListPage : Page
{
	private readonly CmxEntryListModel viewModel;
	private readonly PagePersistenceService persistenceService;

	public CmxObjectType ObjectType => viewModel.ObjectType;

	public CmxEntryListPage()
	{
		viewModel = App.Current.Services.GetRequiredService<CmxEntryListModel>();
		persistenceService = App.Current.Services.GetRequiredService<PagePersistenceService>();

		InitializeComponent();
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

	private void EntryList_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
	{
		var persistedData = persistenceService.Get(ObjectType);
		var scrollOffset = persistedData?.ScrollOffset;

		if (scrollOffset is not null)
		{
			FindScrollViewer()
				.ChangeView(
					horizontalOffset: null,
					verticalOffset: scrollOffset,
					zoomFactor: null,
					disableAnimation: true
				);
		}
	}

	protected override async void OnNavigatedTo(NavigationEventArgs e)
	{
		base.OnNavigatedTo(e);

		if (e.Parameter is CmxObjectType objectType)
		{
			viewModel.ObjectType = objectType;

			var persistedData = persistenceService.Get(objectType);
			viewModel.FilterText = persistedData?.FilterText ?? "";
		}

		App.MainWindow.EnsureNavigationSelection(typeof(CmxEntryListPage), viewModel.ObjectType);

		VisualStateManager.GoToState(this, "Loading", true);

		await viewModel.LoadAsync();

		VisualStateManager.GoToState(this, "Ready", true);
	}

	protected override void OnNavigatedFrom(NavigationEventArgs e)
	{
		base.OnNavigatedFrom(e);

		persistenceService.Set(
			ObjectType.ToString(),
			new PagePersistenceData
			{
				FilterText = viewModel.FilterText,
				ScrollOffset = FindScrollViewer().VerticalOffset,
			}
		);
	}

	private ScrollViewer FindScrollViewer()
	{
		return EntryList.FindDescendant<ScrollViewer>() ?? throw new NullReferenceException();
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
