using System;
using CommunityToolkit.WinUI;
using HelixToolkit.SharpDX.Model.Scene;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;

namespace Pso2Tools.Defrost.Controls;

public sealed partial class SceneTree : UserControl
{
	public SceneNode? SceneRoot
	{
		get { return (SceneNode?)GetValue(SceneRootProperty); }
		set { SetValue(SceneRootProperty, value); }
	}

	public static readonly DependencyProperty SceneRootProperty = DependencyProperty.Register(
		nameof(SceneRoot),
		typeof(SceneNode),
		typeof(SceneTree),
		new PropertyMetadata(null)
	);

	public SceneNode? SelectedItem
	{
		get { return (SceneNode?)GetValue(SelectedItemProperty); }
		set { SetValue(SelectedItemProperty, value); }
	}

	public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
		nameof(SelectedItem),
		typeof(SceneNode),
		typeof(SceneTree),
		new PropertyMetadata(null, new PropertyChangedCallback(OnSelectedItemChanged))
	);

	public SceneTree()
	{
		InitializeComponent();
	}

	private static void OnSelectedItemChanged(
		DependencyObject d,
		DependencyPropertyChangedEventArgs e
	)
	{
		((SceneTree)d).OnSelectedItemChanged(e);
	}

	private void OnSelectedItemChanged(DependencyPropertyChangedEventArgs e)
	{
		if (SelectedItem is null)
		{
			return;
		}

		var container = TreeView.ContainerFromItem(SelectedItem);
		if (container is TreeViewItem item)
		{
			item.StartBringIntoView();
		}
	}

	private void TreeView_Loaded(object sender, RoutedEventArgs e)
	{
		// Workaround for https://github.com/microsoft/microsoft-ui-xaml/issues/8825

		var scrollViewer = TreeView.FindDescendant<ScrollViewer>();
		scrollViewer?.HorizontalScrollMode = ScrollMode.Auto;
		scrollViewer?.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
		scrollViewer?.Padding = new Thickness(0, 2, 0, 2);
	}
}

public partial class SceneNodeTemplateSelector : DataTemplateSelector
{
	public DataTemplate? DefaultTemplate { get; set; }

	protected override DataTemplate? SelectTemplateCore(object item)
	{
		return item switch
		{
			SceneNode => DefaultTemplate,
			_ => null,
		};
	}
}

public partial class SceneNodeToIconConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		return value switch
		{
			GroupNode group => GetGroupIcon(group),
			BoneSkinMeshNode => "\uE879",
			_ => "\uE897",
		};
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		throw new NotImplementedException();
	}

	public static string GetGroupIcon(GroupNode group)
	{
		if (group.Name.StartsWith("mesh"))
		{
			return "\uF158";
		}

		// Assume anything not a mesh is a skeleton node
		return "\uE776";
	}
}
