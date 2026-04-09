using AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData;
using AquaModelLibrary.Data.Utility;

namespace Pso2Tools;

public class CmxDatabase : ICmxDatabase
{
	private string? _pso2BinPath;
	public string? Pso2BinPath
	{
		get => _pso2BinPath;
		set
		{
			if (value != _pso2BinPath)
			{
				_pso2BinPath = value;
				Reset();
			}
		}
	}

	public event EventHandler<ErrorEventArgs>? LoadFailed;

	private Task<IEnumerable<CmxColorSet>?>? colorsTask;
	private Task<CmxEntryFactory?>? factoryTask;

	private Task<IEnumerable<CmxEntry<ACCEObject>>?>? accessoriesTask;
	private Task<IEnumerable<CmxEntry<BODYObject>>?>? basewearTask;
	private Task<IEnumerable<CmxEntry<BBLYObject>>?>? bodypaintTask;
	private Task<IEnumerable<CmxEntry<BODYObject>>?>? castBodiesTask;
	private Task<IEnumerable<CmxEntry<BODYObject>>?>? castArmsTask;
	private Task<IEnumerable<CmxEntry<BODYObject>>?>? castLegsTask;
	private Task<IEnumerable<CmxEntry<BODYObject>>?>? costumesTask;
	private Task<IEnumerable<CmxEntry<NGS_EarObject>>?>? earsTask;
	private Task<IEnumerable<CmxEntry<EYEObject>>?>? eyesTask;
	private Task<IEnumerable<CmxEntry<EYEBObject>>?>? eyebrowsTask;
	private Task<IEnumerable<CmxEntry<EYEBObject>>?>? eyelashesTask;
	private Task<IEnumerable<CmxEntry<FACEObject>>?>? facesTask;
	private Task<IEnumerable<CmxEntry<FaceTextureObject>>?>? faceTexturesTask;
	private Task<IEnumerable<CmxEntry<FCPObject>>?>? facePaintTask;
	private Task<IEnumerable<CmxEntry<HAIRObject>>?>? hairTask;
	private Task<IEnumerable<CmxEntry<NGS_HornObject>>?>? hornsTask;
	private Task<IEnumerable<CmxEntry<BBLYObject>>?>? innerwearTask;
	private Task<IEnumerable<CmxEntry<BODYObject>>?>? outerwearTask;
	private Task<IEnumerable<CmxEntry<NGS_SKINObject>>?>? skinsTask;
	private Task<IEnumerable<CmxEntry<StickerObject>>?>? stickersTask;
	private Task<IEnumerable<CmxEntry<NGS_TeethObject>>?>? teethTask;

	public CmxDatabase(string? pso2BinPath = null)
	{
		Pso2BinPath = pso2BinPath ?? GameFinder.FindPso2BinPath();
	}

	protected virtual void OnLoadFailed(ErrorEventArgs e)
	{
		LoadFailed?.Invoke(this, e);
	}

	private void Reset()
	{
		// TODO: what if this is called while one of the tasks is running?

		colorsTask = null;
		factoryTask = null;

		accessoriesTask = null;
		basewearTask = null;
		bodypaintTask = null;
		castBodiesTask = null;
		castArmsTask = null;
		castLegsTask = null;
		costumesTask = null;
		earsTask = null;
		eyesTask = null;
		eyebrowsTask = null;
		eyelashesTask = null;
		facesTask = null;
		faceTexturesTask = null;
		facePaintTask = null;
		hairTask = null;
		hornsTask = null;
		innerwearTask = null;
		outerwearTask = null;
		skinsTask = null;
		stickersTask = null;
		teethTask = null;
	}

	public async Task LoadAsync()
	{
		await GetFactoryAsync();
	}

	public async Task<IEnumerable<CmxColorSet>> GetColorsAsync()
	{
		colorsTask ??= Task.Run(async () =>
		{
			var factory = await GetFactoryAsync();
			if (factory is null || Pso2BinPath is null)
			{
				return null;
			}

			var outerwear = CmxNameDictionary.GetItemNames(factory.PartsText, "costume");
			var basewear = CmxNameDictionary.GetItemNames(factory.PartsText, "basewear");
			var innerwear = CmxNameDictionary.GetItemNames(factory.PartsText, "innerwear");

			try
			{
				return CmxColorSet.GetColorSets(
					CharacterColorList.Load(Pso2BinPath),
					[outerwear, basewear, innerwear]
				);
			}
			catch (IOException ex)
			{
				OnLoadFailed(new(ex));
				return null;
			}
		});

		return (await colorsTask) ?? [];
	}

	public async Task<IEnumerable<CmxEntry<ACCEObject>>> GetAccessoriesAsync()
	{
		accessoriesTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetAccessories());
		return (await accessoriesTask) ?? [];
	}

	public async Task<IEnumerable<CmxEntry<BODYObject>>> GetBasewearAsync()
	{
		basewearTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetBasewear());
		return (await basewearTask) ?? [];
	}

	public async Task<IEnumerable<CmxEntry<BBLYObject>>> GetBodypaintAsync()
	{
		bodypaintTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetBodypaint());
		return (await bodypaintTask) ?? [];
	}

	public async Task<IEnumerable<CmxEntry<BODYObject>>> GetCastBodiesAsync()
	{
		castBodiesTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetCastBodies());
		return (await castBodiesTask) ?? [];
	}

	public async Task<IEnumerable<CmxEntry<BODYObject>>> GetCastArmsAsync()
	{
		castArmsTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetCastArms());
		return (await castArmsTask) ?? [];
	}

	public async Task<IEnumerable<CmxEntry<BODYObject>>> GetCastLegsAsync()
	{
		castLegsTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetCastLegs());
		return (await castLegsTask) ?? [];
	}

	public async Task<IEnumerable<CmxEntry<BODYObject>>> GetCostumesAsync()
	{
		costumesTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetCostumes());
		return (await costumesTask) ?? [];
	}

	public async Task<IEnumerable<CmxEntry<NGS_EarObject>>> GetEarsAsync()
	{
		earsTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetEars());
		return (await earsTask) ?? [];
	}

	public async Task<IEnumerable<CmxEntry<EYEObject>>> GetEyesAsync()
	{
		eyesTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetEyes());
		return (await eyesTask) ?? [];
	}

	public async Task<IEnumerable<CmxEntry<EYEBObject>>> GetEyebrowsAsync()
	{
		eyebrowsTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetEyebrows());
		return (await eyebrowsTask) ?? [];
	}

	public async Task<IEnumerable<CmxEntry<EYEBObject>>> GetEyelashesAsync()
	{
		eyelashesTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetEyelashes());
		return (await eyelashesTask) ?? [];
	}

	public async Task<IEnumerable<CmxEntry<FACEObject>>> GetFacesAsync()
	{
		facesTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetFaces());
		return (await facesTask) ?? [];
	}

	public async Task<IEnumerable<CmxEntry<FaceTextureObject>>> GetFaceTexturesAsync()
	{
		faceTexturesTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetFaceTextures());
		return (await faceTexturesTask) ?? [];
	}

	public async Task<IEnumerable<CmxEntry<FCPObject>>> GetFacepaintAsync()
	{
		facePaintTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetFacepaint());
		return (await facePaintTask) ?? [];
	}

	public async Task<IEnumerable<CmxEntry<HAIRObject>>> GetHairAsync()
	{
		hairTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetHair());
		return (await hairTask) ?? [];
	}

	public async Task<IEnumerable<CmxEntry<NGS_HornObject>>> GetHornsAsync()
	{
		hornsTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetHorns());
		return (await hornsTask) ?? [];
	}

	public async Task<IEnumerable<CmxEntry<BBLYObject>>> GetInnerwearAsync()
	{
		innerwearTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetInnerwear());
		return (await innerwearTask) ?? [];
	}

	public async Task<IEnumerable<CmxEntry<BODYObject>>> GetOuterwearAsync()
	{
		outerwearTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetOuterwear());
		return (await outerwearTask) ?? [];
	}

	public async Task<IEnumerable<CmxEntry<NGS_SKINObject>>> GetSkinsAsync()
	{
		skinsTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetSkins());
		return (await skinsTask) ?? [];
	}

	public async Task<IEnumerable<CmxEntry<StickerObject>>> GetStickersAsync()
	{
		stickersTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetStickers());
		return (await stickersTask) ?? [];
	}

	public async Task<IEnumerable<CmxEntry<NGS_TeethObject>>> GetTeethAsync()
	{
		teethTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetTeeth());
		return (await teethTask) ?? [];
	}

	public Task<IEnumerable<ICmxEntry>> GetObjectsAsync(CmxObjectType objectType)
	{
		return objectType switch
		{
			CmxObjectType.Accessory => GetObjectsAsync(GetAccessoriesAsync),
			CmxObjectType.Basewear => GetObjectsAsync(GetBasewearAsync),
			CmxObjectType.Bodypaint => GetObjectsAsync(GetBodypaintAsync),
			CmxObjectType.CastArms => GetObjectsAsync(GetCastArmsAsync),
			CmxObjectType.CastBody => GetObjectsAsync(GetCastBodiesAsync),
			CmxObjectType.CastLegs => GetObjectsAsync(GetCastLegsAsync),
			CmxObjectType.Costume => GetObjectsAsync(GetCostumesAsync),
			CmxObjectType.Ear => GetObjectsAsync(GetEarsAsync),
			CmxObjectType.Eye => GetObjectsAsync(GetEyesAsync),
			CmxObjectType.Eyebrow => GetObjectsAsync(GetEyebrowsAsync),
			CmxObjectType.Eyelash => GetObjectsAsync(GetEyelashesAsync),
			CmxObjectType.Face => GetObjectsAsync(GetFacesAsync),
			CmxObjectType.FaceTexture => GetObjectsAsync(GetFaceTexturesAsync),
			CmxObjectType.Facepaint => GetObjectsAsync(GetFacepaintAsync),
			CmxObjectType.Hair => GetObjectsAsync(GetHairAsync),
			CmxObjectType.Horn => GetObjectsAsync(GetHornsAsync),
			CmxObjectType.Innerwear => GetObjectsAsync(GetInnerwearAsync),
			CmxObjectType.Outerwear => GetObjectsAsync(GetOuterwearAsync),
			CmxObjectType.Skin => GetObjectsAsync(GetSkinsAsync),
			CmxObjectType.Sticker => GetObjectsAsync(GetStickersAsync),
			CmxObjectType.Teeth => GetObjectsAsync(GetTeethAsync),
			_ => throw new ArgumentException("Invalid enum value", nameof(objectType)),
		};
	}

	private static async Task<IEnumerable<ICmxEntry>> GetObjectsAsync<T>(
		Func<Task<IEnumerable<CmxEntry<T>>>> getObjects
	)
		where T : BaseCMXObject
	{
		return await getObjects();
	}

	private async Task<CmxEntryFactory?> GetFactoryAsync()
	{
		factoryTask ??= Task.Run(() =>
		{
			if (Pso2BinPath is null)
			{
				return null;
			}

			try
			{
				var cmx = ReferenceGenerator.ExtractCMX(Pso2BinPath);
				ReferenceGenerator.ReadCMXText(
					Pso2BinPath,
					out var partsText,
					out var acceText,
					out _,
					out _
				);

				var faceVariationDict = FaceVariationDict.Load(Pso2BinPath);
				return new CmxEntryFactory(cmx, partsText, acceText, faceVariationDict);
			}
			catch (IOException ex)
			{
				OnLoadFailed(new(ex));
				return null;
			}
		});

		return await factoryTask;
	}
}
