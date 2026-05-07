using System;
using CommunityToolkit.WinUI.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pso2Tools.Defrost.ViewModels.Preview;

namespace Pso2Tools.Defrost.Views.Preview;

public sealed partial class PreviewPageText : Page
{
	private readonly PreviewModelText viewModel;

	public PreviewPageText()
	{
		viewModel = App.Current.Services.GetRequiredService<PreviewModelText>();

		InitializeComponent();

		UpdateSizeState();

		viewModel.PropertyChanged += ViewModel_PropertyChanged;
	}

	private void ViewModel_PropertyChanged(
		object? sender,
		System.ComponentModel.PropertyChangedEventArgs e
	)
	{
		switch (e.PropertyName)
		{
			case nameof(viewModel.Pages):
				UpdatePagesState();
				break;
		}
	}

	protected override async void OnNavigatedTo(NavigationEventArgs e)
	{
		base.OnNavigatedTo(e);

		if (e.Parameter is IceDataFile file)
		{
			await viewModel.LoadFileAsync(file);

			UpdateSizeState();
		}
	}

	protected override void OnNavigatedFrom(NavigationEventArgs e)
	{
		base.OnNavigatedFrom(e);

		viewModel.PropertyChanged -= ViewModel_PropertyChanged;
	}

	private void Category_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (sender is Segmented segmented && segmented.SelectedItem is Pso2TextCategoryModel cat)
		{
			viewModel.SelectedCategory = cat;
		}
	}

	private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		UpdateSizeState();
	}

	private void UpdateSizeState()
	{
		var state =
			viewModel.Categories.Count >= 8
				? "Collapsed"
				: ActualWidth switch
				{
					(< 600) => "Collapsed",
					_ => "Expanded",
				};

		VisualStateManager.GoToState(this, state, true);
	}

	private void UpdatePagesState()
	{
		var state = viewModel.Pages.Length <= 1 ? "SinglePage" : "MultiplePages";

		VisualStateManager.GoToState(this, state, true);

		PageTabs.ItemsSource = viewModel.Pages;
		PageTabs.SelectedItem = viewModel.Pages[viewModel.CurrentPage];
	}

	private void PageTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (PageTabs.SelectedItem is int index)
		{
			viewModel.CurrentPage = index;
		}
	}
}
