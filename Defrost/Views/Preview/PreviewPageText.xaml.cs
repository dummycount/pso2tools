using Microsoft.Extensions.DependencyInjection;
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
	}

	protected override async void OnNavigatedTo(NavigationEventArgs e)
	{
		base.OnNavigatedTo(e);

		if (e.Parameter is IceDataFile file)
		{
			await viewModel.LoadFileAsync(file);
		}
	}
}
