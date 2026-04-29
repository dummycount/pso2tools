using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using CommunityToolkit.WinUI.Helpers;
using Config.Net;
using HelixToolkit.SharpDX;
using Microsoft.UI.Xaml;
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
	// General settings

	public string? Pso2BinPath { get; set; }

	[DefaultValue(ElementTheme.Default)]
	public ElementTheme AppTheme { get; set; }

	[DefaultValue(true)]
	bool ShowPreviewPanel { get; set; }

	[DefaultValue(false)]
	bool RememberLastOpenedFile { get; set; }

	[DefaultValue(null)]
	string? LastOpenedFile { get; set; }

	// Extract settings

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

	// Texture preview settings

	[DefaultValue(ImagePreviewMode.ColorAndAlpha)]
	ImagePreviewMode ImagePreviewMode { get; set; }

	// Model preview settings

	[DefaultValue(FXAALevel.Medium)]
	FXAALevel ModelPreviewFXAALevel { get; set; }

	[DefaultValue(true)]
	bool ModelPreviewShowAxes { get; set; }

	[DefaultValue(true)]
	bool ModelPreviewShowGrid { get; set; }

	[DefaultValue(false)]
	bool ModelPreviewShowWireframe { get; set; }

	[DefaultValue("f65a2a073f5550ddf04717ccd5267ccc")]
	string SkinTextureT1File { get; set; }

	[DefaultValue("af90e2fdc4e355ecb5e868a04e0e5491")]
	string SkinTextureT2File { get; set; }

	[DefaultValue("#f5c4bA")]
	Color SkinColor { get; set; }

	[DefaultValue("#ff0000")]
	Color SubSkinColor { get; set; }

	[DefaultValue("#E81123")]
	Color RedColor { get; set; }

	[DefaultValue("#00CC6A")]
	Color GreenColor { get; set; }

	[DefaultValue("#0078d4")]
	Color BlueColor { get; set; }

	[DefaultValue("#FFB900")]
	Color AlphaColor { get; set; }
}

public class ColorParser : ITypeParser
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

public static class SettingsExtensions
{
	extension(ISettingsService settings)
	{
		public void UpdateLastOpenedFile(string? path)
		{
			settings.LastOpenedFile = settings.RememberLastOpenedFile ? path : null;
		}

		public string? GetSkinTextureT1Path()
		{
			if (settings.Pso2BinPath is null)
			{
				return null;
			}

			return Path.Join(settings.Pso2BinPath, "data", "win32", settings.SkinTextureT1File);
		}

		public string? GetSkinTextureT2Path()
		{
			if (settings.Pso2BinPath is null)
			{
				return null;
			}

			return Path.Join(settings.Pso2BinPath, "data", "win32", settings.SkinTextureT2File);
		}
	}
}
