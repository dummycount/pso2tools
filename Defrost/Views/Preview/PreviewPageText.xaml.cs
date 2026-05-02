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

		UpdateVisualState();
	}

	protected override async void OnNavigatedTo(NavigationEventArgs e)
	{
		base.OnNavigatedTo(e);

		if (e.Parameter is IceDataFile file)
		{
			await viewModel.LoadFileAsync(file);

			UpdateVisualState();
		}
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
		UpdateVisualState();
	}

	private void UpdateVisualState()
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
}
