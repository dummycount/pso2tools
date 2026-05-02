using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pso2Tools.Defrost.ViewModels.Preview;

namespace Pso2Tools.Defrost.Views.Preview;

public sealed partial class PreviewPageAqp : Page
{
	private readonly PreviewModelAqp viewModel;
	private readonly ISettingsService settings;

	public PreviewPageAqp()
	{
		viewModel = App.Current.Services.GetRequiredService<PreviewModelAqp>();
		settings = App.Current.Services.GetRequiredService<ISettingsService>();

		InitializeComponent();

		// TODO: find a workaround for https://github.com/CommunityToolkit/Windows/issues/379
		// This doesn't work because ColorPicker is null if the picker is initially invisible.
		// SkinColor.ColorPicker.CustomPalette = new SkinToneColorPalette();
	}

	protected override async void OnNavigatedTo(NavigationEventArgs e)
	{
		base.OnNavigatedTo(e);

		if (e.Parameter is IceDataFile file)
		{
			await viewModel.LoadModelCommand.ExecuteAsync(file);
		}
	}

	private void UpdateVisualState()
	{
		var state = ActualWidth switch
		{
			(< 600) => "Collapsed",
			(< 800) => "Expanded",
			_ => "Expanded2",
		};

		VisualStateManager.GoToState(this, state, true);
	}

	private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		UpdateVisualState();
	}
}
