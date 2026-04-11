using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pso2Tools.CmxViewer.ViewModels;

namespace Pso2Tools.CmxViewer.Views;

public class CmxEntryPageParam(CmxObjectType type, ICmxEntry obj)
{
	public CmxObjectType ObjectType { get; set; } = type;
	public ICmxEntry Object { get; set; } = obj;
}

public sealed partial class CmxEntryPage : Page
{
	private readonly CmxEntryModel viewModel;

	private readonly AlternatingRowColor alternatingRowColor = new();

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

		App.MainWindow.EnsureNavigationSelection(typeof(CmxEntryPage), viewModel.ObjectType);
	}

	private void MemberList_ContainerContentChanging(
		ListViewBase sender,
		ContainerContentChangingEventArgs args
	)
	{
		alternatingRowColor.Apply(args);
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
