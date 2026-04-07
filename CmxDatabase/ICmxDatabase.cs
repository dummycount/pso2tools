using AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData;

namespace Pso2Tools;

public interface ICmxDatabase
{
	public string? Pso2BinPath { get; set; }

	public Task LoadAsync();

	public Task<IEnumerable<CmxColorSet>> GetColorsAsync();

	public Task<IEnumerable<CmxEntry<ACCEObject>>> GetAccessoriesAsync();
	public Task<IEnumerable<CmxEntry<BODYObject>>> GetBasewearAsync();
	public Task<IEnumerable<CmxEntry<BBLYObject>>> GetBodypaintAsync();
	public Task<IEnumerable<CmxEntry<BODYObject>>> GetCastBodiesAsync();
	public Task<IEnumerable<CmxEntry<BODYObject>>> GetCastArmsAsync();
	public Task<IEnumerable<CmxEntry<BODYObject>>> GetCastLegsAsync();
	public Task<IEnumerable<CmxEntry<BODYObject>>> GetCostumesAsync();
	public Task<IEnumerable<CmxEntry<NGS_EarObject>>> GetEarsAsync();
	public Task<IEnumerable<CmxEntry<EYEObject>>> GetEyesAsync();
	public Task<IEnumerable<CmxEntry<EYEBObject>>> GetEyebrowsAsync();
	public Task<IEnumerable<CmxEntry<EYEBObject>>> GetEyelashesAsync();
	public Task<IEnumerable<CmxEntry<FACEObject>>> GetFacesAsync();
	public Task<IEnumerable<CmxEntry<FaceTextureObject>>> GetFaceTexturesAsync();
	public Task<IEnumerable<CmxEntry<FCPObject>>> GetFacepaintAsync();
	public Task<IEnumerable<CmxEntry<HAIRObject>>> GetHairAsync();
	public Task<IEnumerable<CmxEntry<NGS_HornObject>>> GetHornsAsync();
	public Task<IEnumerable<CmxEntry<BBLYObject>>> GetInnerwearAsync();
	public Task<IEnumerable<CmxEntry<BODYObject>>> GetOuterwearAsync();
	public Task<IEnumerable<CmxEntry<NGS_SKINObject>>> GetSkinsAsync();
	public Task<IEnumerable<CmxEntry<StickerObject>>> GetStickersAsync();
	public Task<IEnumerable<CmxEntry<NGS_TeethObject>>> GetTeethAsync();

	public Task<IEnumerable<ICmxEntry>> GetObjectsAsync(CmxObjectType objectType);
}
