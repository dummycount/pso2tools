using CommunityToolkit.Mvvm.ComponentModel;

namespace Pso2Tools.Defrost.ViewModels;

public partial class PreviewDdsModel : ObservableObject
{
	[ObservableProperty]
	public partial bool IsLoading { get; set; }
}
