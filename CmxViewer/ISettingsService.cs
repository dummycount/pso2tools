using System.ComponentModel;
using CommunityToolkit.WinUI.Collections;
using Microsoft.UI.Xaml;

namespace Pso2Tools.CmxViewer;

public interface ISettingsService : INotifyPropertyChanged
{
	public string? Pso2BinPath { get; set; }

	public string? LastPage { get; set; }

	[DefaultValue(ElementTheme.Default)]
	public ElementTheme AppTheme { get; set; }

	[DefaultValue(BodyType.All)]
	public BodyType BodyTypeFilter { get; set; }

	[DefaultValue(GameVersion.All)]
	public GameVersion GameVersionFilter { get; set; }

	[DefaultValue(SortProperty.Id)]
	public SortProperty SortProperty { get; set; }

	[DefaultValue(SortDirection.Ascending)]
	public SortDirection SortDirection { get; set; }
}
