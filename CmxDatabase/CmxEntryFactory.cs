using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData;
using UnluacNET;

namespace Pso2Tools;

public class CmxEntryFactory(
	CharacterMakingIndex cmx,
	PSO2Text partsText,
	PSO2Text accessoryText,
	Dictionary<string, int> faceVariationDict
)
{
	public PSO2Text PartsText { get; } = partsText;
	public PSO2Text AccessoryText { get; } = accessoryText;

	public IEnumerable<CmxAccessoryEntry> GetAccessories()
	{
		var names = CmxNameDictionary.GetItemNames(AccessoryText, "decoy");
		return GetEntries<CmxAccessoryEntry, ACCEObject>(
			CmxObjectType.Accessory,
			cmx.accessoryDict,
			cmx.accessoryIdLink,
			names
		);
	}

	public IEnumerable<CmxBodyEntry> GetBasewear()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "basewear");
		return GetEntries<CmxBodyEntry, BODYObject>(
			CmxObjectType.Basewear,
			cmx.baseWearDict,
			cmx.baseWearIdLink,
			names
		);
	}

	public IEnumerable<CmxBodypaintEntry> GetBodypaint()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "bodypaint1");
		return GetEntries<CmxBodypaintEntry, BBLYObject>(
			CmxObjectType.Bodypaint,
			cmx.bodyPaintDict,
			names
		);
	}

	public IEnumerable<CmxBodyEntry> GetCastBodies()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "body");
		return GetEntries<CmxBodyEntry, BODYObject>(
			CmxObjectType.CastBody,
			cmx.costumeDict.Where((kv) => kv.Key >= CmxObjectIds.ClassicCastStart),
			cmx.costumeIdLink,
			names
		);
	}

	public IEnumerable<CmxBodyEntry> GetCastArms()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "arm");
		return GetEntries<CmxBodyEntry, BODYObject>(
			CmxObjectType.CastArms,
			cmx.carmDict,
			cmx.castArmIdLink,
			names
		);
	}

	public IEnumerable<CmxBodyEntry> GetCastLegs()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "Leg");
		return GetEntries<CmxBodyEntry, BODYObject>(
			CmxObjectType.CastLegs,
			cmx.clegDict,
			cmx.clegIdLink,
			names
		);
	}

	public IEnumerable<CmxBodyEntry> GetCostumes()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "costume");
		return GetEntries<CmxBodyEntry, BODYObject>(
			CmxObjectType.Costume,
			cmx.costumeDict.Where((kv) => kv.Key < CmxObjectIds.ClassicCastStart),
			cmx.costumeIdLink,
			names
		);
	}

	public IEnumerable<CmxEarEntry> GetEars()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "ears");
		return GetEntries<CmxEarEntry, NGS_EarObject>(CmxObjectType.Ear, cmx.ngsEarDict, names);
	}

	public IEnumerable<CmxEyeEntry> GetEyes()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "eye");
		return GetEntries<CmxEyeEntry, EYEObject>(CmxObjectType.Eye, cmx.eyeDict, names);
	}

	public IEnumerable<CmxEyebrowEntry> GetEyebrows()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "eyebrows");
		return GetEntries<CmxEyebrowEntry, EYEBObject>(
			CmxObjectType.Eyebrow,
			cmx.eyebrowDict,
			names
		);
	}

	public IEnumerable<CmxEyebrowEntry> GetEyelashes()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "eyelashes");
		return GetEntries<CmxEyebrowEntry, EYEBObject>(
			CmxObjectType.Eyelash,
			cmx.eyelashDict,
			names
		);
	}

	public IEnumerable<CmxFaceEntry> GetFaces()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "face");
		names.Update(CmxNameDictionary.GetItemNames(PartsText, "facevariation", faceVariationDict));

		return GetEntries<CmxFaceEntry, FACEObject>(CmxObjectType.Face, cmx.faceDict, names);
	}

	public IEnumerable<CmxFaceTextureEntry> GetFaceTextures()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "facepaint1");
		return GetEntries<CmxFaceTextureEntry, FaceTextureObject>(
			CmxObjectType.FaceTexture,
			cmx.faceTextureDict,
			names
		);
	}

	public IEnumerable<CmxFacepaintEntry> GetFacepaint()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "facepaint2");
		return GetEntries<CmxFacepaintEntry, FCPObject>(
			CmxObjectType.Facepaint,
			cmx.fcpDict,
			names
		);
	}

	public IEnumerable<CmxHairEntry> GetHair()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "hair");
		return GetEntries<CmxHairEntry, HAIRObject>(CmxObjectType.Hair, cmx.hairDict, names);
	}

	public IEnumerable<CmxHornEntry> GetHorns()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "horn");
		return GetEntries<CmxHornEntry, NGS_HornObject>(CmxObjectType.Horn, cmx.ngsHornDict, names);
	}

	public IEnumerable<CmxBodypaintEntry> GetInnerwear()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "innerwear");
		return GetEntries<CmxBodypaintEntry, BBLYObject>(
			CmxObjectType.Innerwear,
			cmx.innerWearDict,
			cmx.innerWearIdLink,
			names
		);
	}

	public IEnumerable<CmxBodyEntry> GetOuterwear()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "costume");
		return GetEntries<CmxBodyEntry, BODYObject>(
			CmxObjectType.Outerwear,
			cmx.outerDict,
			cmx.outerWearIdLink,
			names
		);
	}

	public IEnumerable<CmxSkinEntry> GetSkins()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "skin");
		return GetEntries<CmxSkinEntry, NGS_SKINObject>(CmxObjectType.Skin, cmx.ngsSkinDict, names);
	}

	public IEnumerable<CmxStickerEntry> GetStickers()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "bodypaint2");
		return GetEntries<CmxStickerEntry, StickerObject>(
			CmxObjectType.Sticker,
			cmx.stickerDict,
			names
		);
	}

	public IEnumerable<CmxTeethEntry> GetTeeth()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "dental");
		return GetEntries<CmxTeethEntry, NGS_TeethObject>(
			CmxObjectType.Teeth,
			cmx.ngsTeethDict,
			names
		);
	}

	private static IEnumerable<T> GetEntries<T, O>(
		CmxObjectType objectType,
		IEnumerable<KeyValuePair<int, O>> objects,
		Dictionary<int, BCLNObject> linkDict,
		CmxNameDictionary names
	)
		where T : BaseCmxEntry<O>, new()
		where O : BaseCMXObject, new()
	{
		return objects.Select(
			(kv) =>
				new T
				{
					ObjectType = objectType,
					Id = kv.Key,
					FileId = GetFileId(kv.Key, linkDict),
					Names = names[kv.Key],
					Data = kv.Value,
				}
		);
	}

	private static IEnumerable<T> GetEntries<T, O>(
		CmxObjectType objectType,
		IEnumerable<KeyValuePair<int, O>> objects,
		CmxNameDictionary names
	)
		where T : BaseCmxEntry<O>, new()
		where O : BaseCMXObject, new()
	{
		return objects.Select(
			(kv) =>
				new T
				{
					ObjectType = objectType,
					Id = kv.Key,
					FileId = kv.Key,
					Names = names[kv.Key],
					Data = kv.Value,
				}
		);
	}

	private static int GetFileId(int itemId, Dictionary<int, BCLNObject> linkIdDict)
	{
		if (linkIdDict.TryGetValue(itemId, out var link))
		{
			return link.bcln.fileId;
		}
		return itemId;
	}
}
