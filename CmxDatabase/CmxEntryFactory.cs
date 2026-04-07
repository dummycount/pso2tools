using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData;

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

	public IEnumerable<CmxEntry<ACCEObject>> GetAccessories()
	{
		var names = CmxNameDictionary.GetItemNames(AccessoryText, "decoy");
		return GetEntries(cmx.accessoryDict, cmx.accessoryIdLink, names);
	}

	public IEnumerable<CmxEntry<BODYObject>> GetBasewear()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "basewear");
		return GetEntries(cmx.baseWearDict, cmx.baseWearIdLink, names);
	}

	public IEnumerable<CmxEntry<BBLYObject>> GetBodypaint()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "bodypaint1");
		return GetEntries(cmx.bodyPaintDict, names);
	}

	public IEnumerable<CmxEntry<BODYObject>> GetCastBodies()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "body");
		return GetEntries(
			cmx.costumeDict.Where((kv) => kv.Key >= CmxObjectIds.ClassicCastStart),
			cmx.costumeIdLink,
			names
		);
	}

	public IEnumerable<CmxEntry<BODYObject>> GetCastArms()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "arm");
		return GetEntries(cmx.carmDict, cmx.castArmIdLink, names);
	}

	public IEnumerable<CmxEntry<BODYObject>> GetCastLegs()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "Leg");
		return GetEntries(cmx.clegDict, cmx.clegIdLink, names);
	}

	public IEnumerable<CmxEntry<BODYObject>> GetCostumes()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "costume");
		return GetEntries(
			cmx.costumeDict.Where((kv) => kv.Key < CmxObjectIds.ClassicCastStart),
			cmx.costumeIdLink,
			names
		);
	}

	public IEnumerable<CmxEntry<NGS_EarObject>> GetEars()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "ears");
		return GetEntries(cmx.ngsEarDict, names);
	}

	public IEnumerable<CmxEntry<EYEObject>> GetEyes()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "eye");
		return GetEntries(cmx.eyeDict, names);
	}

	public IEnumerable<CmxEntry<EYEBObject>> GetEyebrows()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "eyebrows");
		return GetEntries(cmx.eyebrowDict, names);
	}

	public IEnumerable<CmxEntry<EYEBObject>> GetEyelashes()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "eyelashes");
		return GetEntries(cmx.eyelashDict, names);
	}

	public IEnumerable<CmxEntry<FACEObject>> GetFaces()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "face");
		names.Update(CmxNameDictionary.GetItemNames(PartsText, "facevariation", faceVariationDict));

		return GetEntries(cmx.faceDict, names);
	}

	public IEnumerable<CmxEntry<FaceTextureObject>> GetFaceTextures()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "facepaint1");
		return GetEntries(cmx.faceTextureDict, names);
	}

	public IEnumerable<CmxEntry<FCPObject>> GetFacepaint()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "facepaint2");
		return GetEntries(cmx.fcpDict, names);
	}

	public IEnumerable<CmxEntry<HAIRObject>> GetHair()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "hair");
		return GetEntries(cmx.hairDict, names);
	}

	public IEnumerable<CmxEntry<NGS_HornObject>> GetHorns()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "horn");
		return GetEntries(cmx.ngsHornDict, names);
	}

	public IEnumerable<CmxEntry<BBLYObject>> GetInnerwear()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "innerwear");
		return GetEntries(cmx.innerWearDict, cmx.innerWearIdLink, names);
	}

	public IEnumerable<CmxEntry<BODYObject>> GetOuterwear()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "costume");
		return GetEntries(cmx.outerDict, cmx.outerWearIdLink, names);
	}

	public IEnumerable<CmxEntry<NGS_SKINObject>> GetSkins()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "skin");
		return GetEntries(cmx.ngsSkinDict, names);
	}

	public IEnumerable<CmxEntry<StickerObject>> GetStickers()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "bodypaint2");
		return GetEntries(cmx.stickerDict, names);
	}

	public IEnumerable<CmxEntry<NGS_TeethObject>> GetTeeth()
	{
		var names = CmxNameDictionary.GetItemNames(PartsText, "dental");
		return GetEntries(cmx.ngsTeethDict, names);
	}

	private static IEnumerable<CmxEntry<T>> GetEntries<T>(
		IEnumerable<KeyValuePair<int, T>> objects,
		CmxNameDictionary names
	)
		where T : BaseCMXObject
	{
		return objects.Select(
			(kv) =>
			{
				var id = kv.Key;

				return new CmxEntry<T>
				{
					Id = id,
					FileId = id,
					Names = names[id],
					Data = kv.Value,
				};
			}
		);
	}

	private static IEnumerable<CmxEntry<T>> GetEntries<T>(
		IEnumerable<KeyValuePair<int, T>> objects,
		Dictionary<int, BCLNObject> linkDict,
		CmxNameDictionary names
	)
		where T : BaseCMXObject
	{
		return objects.Select(
			(kv) =>
			{
				var id = kv.Key;

				return new CmxEntry<T>
				{
					Id = id,
					FileId = GetFileId(id, linkDict),
					Names = names[id],
					Data = kv.Value,
				};
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
