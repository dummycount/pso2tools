using HelixToolkit.SharpDX.Model.Scene;
using HelixToolkit.WinUI.SharpDX;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Pso2Tools.Defrost.ViewModels.Preview;

namespace Pso2Tools.Defrost.Controls;

public partial class HighlightController : Control
{
	public Viewport3DX? Viewport
	{
		get { return (Viewport3DX)GetValue(ViewportProperty); }
		set { SetValue(ViewportProperty, value); }
	}

	public static readonly DependencyProperty ViewportProperty = DependencyProperty.Register(
		nameof(Viewport),
		typeof(Viewport3DX),
		typeof(HighlightController),
		new PropertyMetadata(null, new PropertyChangedCallback(OnViewportChanged))
	);

	public SceneNode? Root
	{
		get { return (SceneNode?)GetValue(RootProperty); }
		set { SetValue(RootProperty, value); }
	}

	public static readonly DependencyProperty RootProperty = DependencyProperty.Register(
		nameof(Root),
		typeof(SceneNode),
		typeof(HighlightController),
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
		typeof(HighlightController),
		new PropertyMetadata(null, new PropertyChangedCallback(OnSelectedItemChanged))
	);

	public HighlightController()
	{
		DefaultStyleKey = typeof(HighlightController);

		var cancelSelection = new KeyboardAccelerator { Key = Windows.System.VirtualKey.Escape };
		cancelSelection.Invoked += CancelSelection_Invoked;
		KeyboardAccelerators.Add(cancelSelection);
	}

	private void CancelSelection_Invoked(
		KeyboardAccelerator sender,
		KeyboardAcceleratorInvokedEventArgs args
	)
	{
		if (SelectedItem is not null)
		{
			SelectedItem = null;
			args.Handled = true;
		}
	}

	private static void OnSelectedItemChanged(
		DependencyObject d,
		DependencyPropertyChangedEventArgs e
	)
	{
		if (e.OldValue is SceneNode oldNode)
		{
			AttachedNodeViewModel.SetHighlighted(oldNode, false);
		}

		if (e.NewValue is SceneNode newNode)
		{
			AttachedNodeViewModel.SetHighlighted(newNode, true);
			AttachedNodeViewModel.ExpandNode(newNode);
		}
	}

	private static void OnViewportChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((HighlightController)d).OnViewportChanged(e);
	}

	private void OnViewportChanged(DependencyPropertyChangedEventArgs e)
	{
		if (e.OldValue is Viewport3DX oldViewport)
		{
			oldViewport.OnMouse3DDown -= NewViewport_OnMouse3DDown;
			oldViewport.OnMouse3DUp -= NewViewport_OnMouse3DUp;
			oldViewport.OnMouse3DMove -= NewViewport_OnMouse3DMove;
		}

		if (e.NewValue is Viewport3DX newViewport)
		{
			newViewport.OnMouse3DDown += NewViewport_OnMouse3DDown;
			newViewport.OnMouse3DUp += NewViewport_OnMouse3DUp;
			newViewport.OnMouse3DMove += NewViewport_OnMouse3DMove;
		}
	}

	bool isClickingToSelect = false;

	private void NewViewport_OnMouse3DMove(object? sender, MouseMove3DEventArgs e)
	{
		isClickingToSelect = false;
	}

	private void NewViewport_OnMouse3DDown(object? sender, MouseDown3DEventArgs e)
	{
		var p = e.OriginalInputEventArgs?.GetCurrentPoint(Viewport).Properties;

		if (p is null || !p.IsLeftButtonPressed)
		{
			return;
		}

		isClickingToSelect = true;
	}

	private void NewViewport_OnMouse3DUp(object? sender, MouseUp3DEventArgs e)
	{
		if (isClickingToSelect)
		{
			SelectedItem = e.HitTestResult?.ModelHit as SceneNode;
		}
	}
}
