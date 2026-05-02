using System;
using CommunityToolkit.WinUI.Collections;
using CommunityToolkit.WinUI.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pso2Tools.CmxViewer.ViewModels;
using UnluacNET;

namespace Pso2Tools.CmxViewer.Views;

public sealed partial class ColorSetsPage : Page
{
	private readonly ColorSetsModel viewModel;
	private readonly PagePersistenceService persistenceService;

	private readonly AlternatingRowColor alternatingRowColor = new();

	public ColorSetsPage()
	{
		viewModel = App.Current.Services.GetRequiredService<ColorSetsModel>();
		persistenceService = App.Current.Services.GetRequiredService<PagePersistenceService>();

		var persistedData = persistenceService.Get("Colors");
		viewModel.FilterText = persistedData?.FilterText ?? "";

		InitializeComponent();

		App.MainWindow.EnsureNavigationSelection(typeof(ColorSetsPage));
	}

	private void ColorSetList_ContainerContentChanging(
		ListViewBase sender,
		ContainerContentChangingEventArgs args
	)
	{
		alternatingRowColor.Apply(args);
	}

	protected override async void OnNavigatedTo(NavigationEventArgs e)
	{
		base.OnNavigatedTo(e);

		VisualStateManager.GoToState(this, "Loading", true);

		await viewModel.LoadAsync();

		VisualStateManager.GoToState(this, "Ready", true);
	}

	protected override void OnNavigatedFrom(NavigationEventArgs e)
	{
		base.OnNavigatedFrom(e);

		persistenceService.Set(
			"Colors",
			new PagePersistenceData { FilterText = viewModel.FilterText }
		);
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
