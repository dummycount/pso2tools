using System;
using System.Diagnostics;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Controls;
using Windows.UI;

namespace Pso2Tools.Defrost.ViewModels;

public partial class SettingsModel : ObservableObject
{
	private readonly ISettingsService settings;

	public bool IsCustomExtractFolderVisible =>
		settings.DefaultExtractLocation == DefaultExtractLocation.CustomFolder;

	public string? CustomExtractFolderDescription =>
		IsCustomExtractFolderVisible ? settings.CustomExtractFolder ?? "No path set" : null;

	public string Version =>
		FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion ?? "";

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(ProtocolRegisterStatus))]
	[NotifyPropertyChangedFor(nameof(ProtocolRegisterAction))]
	public partial bool IsProtocolRegistered { get; set; }

	public string ProtocolRegisterStatus => IsProtocolRegistered ? "Registered" : "Unregistered";

	public string ProtocolRegisterAction =>
		IsProtocolRegistered ? "Unregister link handler" : "Register link handler";

	public SettingsModel(ISettingsService settings)
	{
		this.settings = settings;
		settings.PropertyChanged += Settings_PropertyChanged;

		IsProtocolRegistered = ProtocolHandler.IsRegistered(ProtocolActivationHelper.Scheme);
	}

	private void Settings_PropertyChanged(
		object? sender,
		System.ComponentModel.PropertyChangedEventArgs e
	)
	{
		switch (e.PropertyName)
		{
			case nameof(settings.DefaultExtractLocation):
				OnPropertyChanged(nameof(IsCustomExtractFolderVisible));
				OnPropertyChanged(nameof(CustomExtractFolderDescription));
				break;

			case nameof(settings.CustomExtractFolder):
				OnPropertyChanged(nameof(CustomExtractFolderDescription));
				break;
		}
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
