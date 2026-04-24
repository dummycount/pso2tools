using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.UI.Xaml;

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

	[DefaultValue(false)]
	bool OpenFolderWhenDone { get; set; }
}
