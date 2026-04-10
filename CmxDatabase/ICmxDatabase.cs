namespace Pso2Tools;

public interface ICmxDatabase
{
	public string? Pso2BinPath { get; set; }

	public event EventHandler<ErrorEventArgs>? LoadFailed;

	public Task LoadAsync();

	public Task<IEnumerable<CmxColorSet>> GetColorsAsync();

	public Task<IEnumerable<CmxAccessoryEntry>> GetAccessoriesAsync();
	public Task<IEnumerable<CmxBodyEntry>> GetBasewearAsync();
	public Task<IEnumerable<CmxBodypaintEntry>> GetBodypaintAsync();
	public Task<IEnumerable<CmxBodyEntry>> GetCastBodiesAsync();
	public Task<IEnumerable<CmxBodyEntry>> GetCastArmsAsync();
	public Task<IEnumerable<CmxBodyEntry>> GetCastLegsAsync();
	public Task<IEnumerable<CmxBodyEntry>> GetCostumesAsync();
	public Task<IEnumerable<CmxEarEntry>> GetEarsAsync();
	public Task<IEnumerable<CmxEyeEntry>> GetEyesAsync();
	public Task<IEnumerable<CmxEyebrowEntry>> GetEyebrowsAsync();
	public Task<IEnumerable<CmxEyebrowEntry>> GetEyelashesAsync();
	public Task<IEnumerable<CmxFaceEntry>> GetFacesAsync();
	public Task<IEnumerable<CmxFaceTextureEntry>> GetFaceTexturesAsync();
	public Task<IEnumerable<CmxFacepaintEntry>> GetFacepaintAsync();
	public Task<IEnumerable<CmxHairEntry>> GetHairAsync();
	public Task<IEnumerable<CmxHornEntry>> GetHornsAsync();
	public Task<IEnumerable<CmxBodypaintEntry>> GetInnerwearAsync();
	public Task<IEnumerable<CmxBodyEntry>> GetOuterwearAsync();
	public Task<IEnumerable<CmxSkinEntry>> GetSkinsAsync();
	public Task<IEnumerable<CmxStickerEntry>> GetStickersAsync();
	public Task<IEnumerable<CmxTeethEntry>> GetTeethAsync();

	public Task<IEnumerable<ICmxEntry>> GetObjectsAsync(CmxObjectType objectType);
}
