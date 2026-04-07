// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace Pso2Tools.CmxViewer.Controls;

public sealed partial class CopyButton : Button
{
	public static readonly DependencyProperty CopiedMessageProperty = DependencyProperty.Register(
		"CopiedMessage",
		typeof(string),
		typeof(CopyButton),
		new PropertyMetadata("Copied to clipboard")
	);

	public string CopiedMessage
	{
		get { return (string)GetValue(CopiedMessageProperty); }
		set { SetValue(CopiedMessageProperty, value); }
	}

	public CopyButton()
	{
		DefaultStyleKey = typeof(CopyButton);
	}

	private void CopyButton_Click(object sender, RoutedEventArgs e)
	{
		if (GetTemplateChild("CopyToClipboardSuccessAnimation") is Storyboard storyBoard)
		{
			storyBoard.Begin();
		}
	}

	protected override void OnApplyTemplate()
	{
		Click -= CopyButton_Click;
		base.OnApplyTemplate();
		Click += CopyButton_Click;
	}
}
