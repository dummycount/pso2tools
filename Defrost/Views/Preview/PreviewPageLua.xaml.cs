using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pso2Tools.Defrost.ViewModels.Preview;

namespace Pso2Tools.Defrost.Views.Preview;

public sealed partial class PreviewPageLua : Page
{
	private readonly PreviewModelLua viewModel;

	public PreviewPageLua()
	{
		viewModel = App.Current.Services.GetRequiredService<PreviewModelLua>();

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
