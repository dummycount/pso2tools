using CommunityToolkit.Mvvm.ComponentModel;

namespace Pso2Tools.Defrost.ViewModels.Preview;

public partial class PreviewModelDds : ObservableObject
{
	[ObservableProperty]
	public partial bool IsLoading { get; set; }
}
