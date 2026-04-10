using System.Diagnostics;
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

	private Task<IEnumerable<CmxAccessoryEntry>?>? accessoriesTask;
	private Task<IEnumerable<CmxBodyEntry>?>? basewearTask;
	private Task<IEnumerable<CmxBodypaintEntry>?>? bodypaintTask;
	private Task<IEnumerable<CmxBodyEntry>?>? castBodiesTask;
	private Task<IEnumerable<CmxBodyEntry>?>? castArmsTask;
	private Task<IEnumerable<CmxBodyEntry>?>? castLegsTask;
	private Task<IEnumerable<CmxBodyEntry>?>? costumesTask;
	private Task<IEnumerable<CmxEarEntry>?>? earsTask;
	private Task<IEnumerable<CmxEyeEntry>?>? eyesTask;
	private Task<IEnumerable<CmxEyebrowEntry>?>? eyebrowsTask;
	private Task<IEnumerable<CmxEyebrowEntry>?>? eyelashesTask;
	private Task<IEnumerable<CmxFaceEntry>?>? facesTask;
	private Task<IEnumerable<CmxFaceTextureEntry>?>? faceTexturesTask;
	private Task<IEnumerable<CmxFacepaintEntry>?>? facePaintTask;
	private Task<IEnumerable<CmxHairEntry>?>? hairTask;
	private Task<IEnumerable<CmxHornEntry>?>? hornsTask;
	private Task<IEnumerable<CmxBodypaintEntry>?>? innerwearTask;
	private Task<IEnumerable<CmxBodyEntry>?>? outerwearTask;
	private Task<IEnumerable<CmxSkinEntry>?>? skinsTask;
	private Task<IEnumerable<CmxStickerEntry>?>? stickersTask;
	private Task<IEnumerable<CmxTeethEntry>?>? teethTask;

	public CmxDatabase(string? pso2BinPath = null)
	{
		Pso2BinPath = pso2BinPath;
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

	public async Task<IEnumerable<CmxAccessoryEntry>> GetAccessoriesAsync()
	{
		accessoriesTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetAccessories());
		return (await accessoriesTask) ?? [];
	}

	public async Task<IEnumerable<CmxBodyEntry>> GetBasewearAsync()
	{
		basewearTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetBasewear());
		return (await basewearTask) ?? [];
	}

	public async Task<IEnumerable<CmxBodypaintEntry>> GetBodypaintAsync()
	{
		bodypaintTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetBodypaint());
		return (await bodypaintTask) ?? [];
	}

	public async Task<IEnumerable<CmxBodyEntry>> GetCastBodiesAsync()
	{
		castBodiesTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetCastBodies());
		return (await castBodiesTask) ?? [];
	}

	public async Task<IEnumerable<CmxBodyEntry>> GetCastArmsAsync()
	{
		castArmsTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetCastArms());
		return (await castArmsTask) ?? [];
	}

	public async Task<IEnumerable<CmxBodyEntry>> GetCastLegsAsync()
	{
		castLegsTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetCastLegs());
		return (await castLegsTask) ?? [];
	}

	public async Task<IEnumerable<CmxBodyEntry>> GetCostumesAsync()
	{
		costumesTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetCostumes());
		return (await costumesTask) ?? [];
	}

	public async Task<IEnumerable<CmxEarEntry>> GetEarsAsync()
	{
		earsTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetEars());
		return (await earsTask) ?? [];
	}

	public async Task<IEnumerable<CmxEyeEntry>> GetEyesAsync()
	{
		eyesTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetEyes());
		return (await eyesTask) ?? [];
	}

	public async Task<IEnumerable<CmxEyebrowEntry>> GetEyebrowsAsync()
	{
		eyebrowsTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetEyebrows());
		return (await eyebrowsTask) ?? [];
	}

	public async Task<IEnumerable<CmxEyebrowEntry>> GetEyelashesAsync()
	{
		eyelashesTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetEyelashes());
		return (await eyelashesTask) ?? [];
	}

	public async Task<IEnumerable<CmxFaceEntry>> GetFacesAsync()
	{
		facesTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetFaces());
		return (await facesTask) ?? [];
	}

	public async Task<IEnumerable<CmxFaceTextureEntry>> GetFaceTexturesAsync()
	{
		faceTexturesTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetFaceTextures());
		return (await faceTexturesTask) ?? [];
	}

	public async Task<IEnumerable<CmxFacepaintEntry>> GetFacepaintAsync()
	{
		facePaintTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetFacepaint());
		return (await facePaintTask) ?? [];
	}

	public async Task<IEnumerable<CmxHairEntry>> GetHairAsync()
	{
		hairTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetHair());
		return (await hairTask) ?? [];
	}

	public async Task<IEnumerable<CmxHornEntry>> GetHornsAsync()
	{
		hornsTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetHorns());
		return (await hornsTask) ?? [];
	}

	public async Task<IEnumerable<CmxBodypaintEntry>> GetInnerwearAsync()
	{
		innerwearTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetInnerwear());
		return (await innerwearTask) ?? [];
	}

	public async Task<IEnumerable<CmxBodyEntry>> GetOuterwearAsync()
	{
		outerwearTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetOuterwear());
		return (await outerwearTask) ?? [];
	}

	public async Task<IEnumerable<CmxSkinEntry>> GetSkinsAsync()
	{
		skinsTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetSkins());
		return (await skinsTask) ?? [];
	}

	public async Task<IEnumerable<CmxStickerEntry>> GetStickersAsync()
	{
		stickersTask ??= Task.Run(async () => (await GetFactoryAsync())?.GetStickers());
		return (await stickersTask) ?? [];
	}

	public async Task<IEnumerable<CmxTeethEntry>> GetTeethAsync()
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
		Func<Task<IEnumerable<T>>> getObjects
	)
		where T : ICmxEntry
	{
		return (await getObjects()).Cast<ICmxEntry>();
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
				var watch = Stopwatch.StartNew();
				Debug.WriteLine("CMX load started");

				var cmx = ReferenceGenerator.ExtractCMX(Pso2BinPath);
				ReferenceGenerator.ReadCMXText(
					Pso2BinPath,
					out var partsText,
					out var acceText,
					out _,
					out _
				);

				var faceVariationDict = FaceVariationDict.Load(Pso2BinPath);

				watch.Stop();
				Debug.WriteLine($"CMX loaded in {watch.Elapsed.TotalSeconds} s");

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
