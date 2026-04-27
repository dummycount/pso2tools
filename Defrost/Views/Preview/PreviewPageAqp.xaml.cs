using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pso2Tools.Defrost.ViewModels;
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
	}

	protected override async void OnNavigatedTo(NavigationEventArgs e)
	{
		base.OnNavigatedTo(e);

		if (e.Parameter is IceFileModel file)
		{
			await viewModel.LoadModelCommand.ExecuteAsync(file);
		}
	}
}
