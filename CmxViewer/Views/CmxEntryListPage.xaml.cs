using CommunityToolkit.WinUI.Collections;
using CommunityToolkit.WinUI.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Pso2Tools.CmxViewer.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pso2Tools.CmxViewer.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class CmxEntryListPage : Page
{
	private readonly CmxEntryListModel viewModel;

	public CmxEntryListPage()
	{
		viewModel = App.Current.Services.GetRequiredService<CmxEntryListModel>();

		InitializeComponent();

		EntryList.ItemClick += EntryList_ItemClick;

		// TODO: persist sort property and order in settings
		// TODO: persist filter toggles in settings, but only apply if object type uses them
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

	protected override async void OnNavigatedTo(NavigationEventArgs e)
	{
		base.OnNavigatedTo(e);

		if (e.Parameter is CmxObjectType objectType)
		{
			viewModel.ObjectType = objectType;
		}

		await viewModel.LoadAsync();

		Progress.IsActive = false;
	}

	// Can't use two-way binding with Segmented because it reports SelectedValue as null sometimes
	// https://github.com/microsoft/microsoft-ui-xaml/issues/3268
	private void SortProperty_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (sender is Segmented control && control.SelectedValue is CmxEntrySort sort)
		{
			viewModel.SortProperty = sort;
		}
	}

	private void SortDirection_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (sender is Segmented control && control.SelectedValue is SortDirection directon)
		{
			viewModel.SortDirection = directon;
		}
	}

	private void GameVersion_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (sender is Segmented control && control.SelectedValue is GameVersion version)
		{
			viewModel.GameVersion = version;
		}
	}

	private void BodyType_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (sender is Segmented control && control.SelectedValue is BodyType bodyType)
		{
			viewModel.BodyType = bodyType;
		}
	}
}
