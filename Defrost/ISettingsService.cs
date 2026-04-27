using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.WinUI.Helpers;
using Config.Net;
using HelixToolkit.SharpDX;
using Microsoft.UI.Xaml;
using Windows.UI;
using Color = Windows.UI.Color;

namespace Pso2Tools.Defrost;

public enum DefaultExtractLocation
{
	[Display(Name = "Same folder as file")]
	SameFolder,

	[Display(Name = "Custom folder")]
	CustomFolder,
}

public enum ExtractGroupMode
{
	[Display(Name = "If both groups present")]
	Auto,

	[Display(Name = "Always")]
	Always,

	[Display(Name = "Never")]
	Never,
}

public enum ImagePreviewMode
{
	[Display(Name = "RGBA")]
	ColorAndAlpha,

	[Display(Name = "RGB")]
	Color,

	[Display(Name = "R")]
	Red,

	[Display(Name = "G")]
	Green,

	[Display(Name = "B")]
	Blue,

	[Display(Name = "A")]
	Alpha,
}

public interface ISettingsService : INotifyPropertyChanged
{
	[DefaultValue(ElementTheme.Default)]
	public ElementTheme AppTheme { get; set; }

	[DefaultValue(CollisionOption.Ask)]
	CollisionOption CollisionOption { get; set; }

	[DefaultValue(DefaultExtractLocation.SameFolder)]
	DefaultExtractLocation DefaultExtractLocation { get; set; }

	[DefaultValue(null)]
	string? CustomExtractFolder { get; set; }

	[DefaultValue(ExtractGroupMode.Auto)]
	ExtractGroupMode ExtractGroupMode { get; set; }

	[DefaultValue(true)]
	bool OpenFolderWhenDone { get; set; }

	[DefaultValue(true)]
	bool ShowPreviewPanel { get; set; }

	[DefaultValue(ImagePreviewMode.ColorAndAlpha)]
	ImagePreviewMode ImagePreviewMode { get; set; }

	[DefaultValue(FXAALevel.Medium)]
	FXAALevel ModelPreviewFXAALevel { get; set; }

	[DefaultValue("#f5c4bA")]
	Color SkinColor { get; set; }
}

class ColorParser : ITypeParser
{
	public IEnumerable<Type> SupportedTypes => [typeof(Color)];

	public string? ToRawString(object? value)
	{
		return ((Color?)value)?.ToHex();
	}

	public bool TryParse(string? value, Type t, out object? result)
	{
		try
		{
			result = ColorHelper.ToColor(value ?? "#000000");
			return true;
		}
		catch (FormatException)
		{
			result = Color.FromArgb(255, 0, 0, 0);
			return false;
		}
	}
}
