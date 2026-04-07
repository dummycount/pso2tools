using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pso2Tools.CmxViewer.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pso2Tools.CmxViewer.Views;

public class CmxEntryPageParam(CmxObjectType type, ICmxEntry obj)
{
	public CmxObjectType ObjectType { get; set; } = type;
	public ICmxEntry Object { get; set; } = obj;
}

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class CmxEntryPage : Page
{
	private readonly CmxEntryModel viewModel;

	public CmxEntryPage()
	{
		viewModel = App.Current.Services.GetRequiredService<CmxEntryModel>();

		InitializeComponent();

		Breadcrumbs.ItemClicked += Breadcrumbs_ItemClicked;
	}

	protected override void OnNavigatedTo(NavigationEventArgs e)
	{
		base.OnNavigatedTo(e);

		if (e.Parameter is CmxEntryPageParam param)
		{
			viewModel.ObjectType = param.ObjectType;
			viewModel.Object = param.Object;

			Breadcrumbs.ItemsSource = new string[]
			{
				param.ObjectType.GetDisplayName(),
				param.Object.Name,
			};
		}
	}

	private void Breadcrumbs_ItemClicked(
		BreadcrumbBar sender,
		BreadcrumbBarItemClickedEventArgs args
	)
	{
		if (args.Index == 0)
		{
			App.MainWindow.GoBack();
		}
	}
}
