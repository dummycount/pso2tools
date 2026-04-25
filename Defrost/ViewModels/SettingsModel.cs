using System;
using System.Diagnostics;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;

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
