using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Controls;
using Windows.UI;

namespace Pso2Tools.Defrost.ViewModels;

public class SkinTexture
{
	public required string Name { get; set; }
	public required string File { get; set; }
}

public partial class SettingsModel : ObservableObject
{
	public readonly SkinTexture[] SkinTextureT1Items =
	[
		new() { Name = "Base Body T1", File = "f65a2a073f5550ddf04717ccd5267ccc" },
		new() { Name = "Base Body T1 (HQ)", File = "195fac68420e7a08fb37ae36403a419b" },
		new() { Name = "Base Body T1/B", File = "6d7573b27fb42b35ab828b628cda64bb" },
		new() { Name = "Base Body T1/B (HQ)", File = "ee351ec0b3f2fcd9bf5fc9175a81425f" },
		new() { Name = "Buff Body", File = "508482e9d5b01bfbd7d3d61e1363d8bb" },
		new() { Name = "Buff Body (HQ)", File = "a783f6d313241bd887568eec4b028197" },
		new() { Name = "Muscular Body", File = "d6a02afee00ae667f119c1e2a7628c01" },
		new() { Name = "Muscular Body (HQ)", File = "de995dbb0d2c09bffb00893b66dfd6ac" },
	];

	public readonly SkinTexture[] SkinTextureT2Items =
	[
		new() { Name = "Base Body T2", File = "af90e2fdc4e355ecb5e868a04e0e5491" },
		new() { Name = "Base Body T2 (HQ)", File = "be23da464641f6ea102f4366095fa5eb" },
		new() { Name = "Lindsey Body", File = "aa7d45056f43a39309b7896fcdd98649" },
		new() { Name = "Lindsey Body (HQ)", File = "751ca397e062cac5947e901554b87712" },
		new() { Name = "Ripped Muscular Body T2", File = "1606b901c12f1b96dce8712a926de63d" },
		new() { Name = "Ripped Muscular Body T2 (HQ)", File = "df75371a7b9d2972f6f5b527808c64fc" },
	];

	private readonly ISettingsService settings;

	public bool IsCustomExtractFolderVisible =>
		settings.DefaultExtractLocation == DefaultExtractLocation.CustomFolder;

	public string? CustomExtractFolderDescription =>
		IsCustomExtractFolderVisible ? settings.CustomExtractFolder ?? "No path set" : null;

	public string Version =>
		FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion ?? "";

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(IsCustomExtractFolderVisible))]
	[NotifyPropertyChangedFor(nameof(CustomExtractFolderDescription))]
	public partial DefaultExtractLocation DefaultExtractLocation { get; set; }

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(CustomExtractFolderDescription))]
	public partial string? CustomExtractFolder { get; set; }

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(ProtocolRegisterStatus))]
	[NotifyPropertyChangedFor(nameof(ProtocolRegisterAction))]
	public partial bool IsProtocolRegistered { get; set; }

	[ObservableProperty]
	public partial SkinTexture SkinTextureT1 { get; set; }

	[ObservableProperty]
	public partial SkinTexture SkinTextureT2 { get; set; }

	public string ProtocolRegisterStatus => IsProtocolRegistered ? "Registered" : "Unregistered";

	public string ProtocolRegisterAction =>
		IsProtocolRegistered ? "Unregister link handler" : "Register link handler";

	public SettingsModel(ISettingsService settings)
	{
		this.settings = settings;

		DefaultExtractLocation = settings.DefaultExtractLocation;
		CustomExtractFolder = settings.CustomExtractFolder;

		SkinTextureT1 =
			SkinTextureT1Items.FirstOrDefault(x => x.File == settings.SkinTextureT1File)
			?? SkinTextureT1Items[0];

		SkinTextureT2 =
			SkinTextureT2Items.FirstOrDefault(x => x.File == settings.SkinTextureT2File)
			?? SkinTextureT2Items[0];

		IsProtocolRegistered = ProtocolHandler.IsRegistered(ProtocolActivationHelper.Scheme);
	}

	partial void OnCustomExtractFolderChanged(string? value)
	{
		settings.CustomExtractFolder = value;
	}

	partial void OnDefaultExtractLocationChanged(DefaultExtractLocation value)
	{
		settings.DefaultExtractLocation = value;
	}

	partial void OnSkinTextureT1Changed(SkinTexture value)
	{
		settings.SkinTextureT1File = value.File;
	}

	partial void OnSkinTextureT2Changed(SkinTexture value)
	{
		settings.SkinTextureT2File = value.File;
	}
}

public class SkinToneColorPalette : IColorPalette
{
	public int ColorCount => colorChart.GetLength(0);

	public int ShadeCount => colorChart.GetLength(1);

	public Color GetColor(int colorIndex, int shadeIndex)
	{
		return colorChart[
			Math.Clamp(colorIndex, 0, colorChart.GetLength(0)),
			Math.Clamp(shadeIndex, 0, colorChart.GetLength(1))
		];
	}

	private static readonly Color[,] colorChart = new Color[,]
	{
		{
			Color.FromArgb(255, 255, 238, 215),
			Color.FromArgb(255, 245, 221, 186),
			Color.FromArgb(255, 227, 188, 134),
			Color.FromArgb(255, 207, 164, 103),
			Color.FromArgb(255, 255, 232, 215),
			Color.FromArgb(255, 245, 211, 186),
			Color.FromArgb(255, 227, 173, 134),
			Color.FromArgb(255, 206, 146, 103),
		},
		{
			Color.FromArgb(255, 255, 228, 215),
			Color.FromArgb(255, 245, 209, 186),
			Color.FromArgb(255, 226, 164, 134),
			Color.FromArgb(255, 207, 138, 103),
			Color.FromArgb(255, 255, 221, 215),
			Color.FromArgb(255, 245, 196, 186),
			Color.FromArgb(255, 227, 149, 134),
			Color.FromArgb(255, 207, 120, 103),
		},
		{
			Color.FromArgb(255, 187, 169, 152),
			Color.FromArgb(255, 191, 160, 140),
			Color.FromArgb(255, 194, 152, 127),
			Color.FromArgb(255, 198, 143, 115),
			Color.FromArgb(255, 186, 156, 151),
			Color.FromArgb(255, 187, 149, 142),
			Color.FromArgb(255, 190, 143, 134),
			Color.FromArgb(255, 192, 136, 125),
		},
		{
			Color.FromArgb(255, 38, 35, 33),
			Color.FromArgb(255, 50, 40, 35),
			Color.FromArgb(255, 61, 45, 36),
			Color.FromArgb(255, 73, 50, 38),
			Color.FromArgb(255, 54, 50, 49),
			Color.FromArgb(255, 62, 48, 45),
			Color.FromArgb(255, 69, 47, 42),
			Color.FromArgb(255, 77, 45, 38),
		},
	};
}
