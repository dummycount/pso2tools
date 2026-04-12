using System.ComponentModel.DataAnnotations;
using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData;
using AquaModelLibrary.Data.PSO2.Constants;
using UnluacNET;

namespace Pso2Tools;

public enum CmxObjectType
{
	[Display(Name = "Accessories")]
	Accessory,

	[Display(Name = "Basewear")]
	Basewear,

	[Display(Name = "Bodypaint")]
	Bodypaint,

	[Display(Name = "CAST Arms")]
	CastArms,

	[Display(Name = "CAST Bodies")]
	CastBody,

	[Display(Name = "CAST Legs")]
	CastLegs,

	[Display(Name = "Costumes")]
	Costume,

	[Display(Name = "Ears")]
	Ear,

	[Display(Name = "Eyes")]
	Eye,

	[Display(Name = "Eyebrows")]
	Eyebrow,

	[Display(Name = "Eyelashes")]
	Eyelash,

	[Display(Name = "Faces")]
	Face,

	[Display(Name = "Face Textures")]
	FaceTexture,

	[Display(Name = "Facepaint")]
	Facepaint,

	[Display(Name = "Hair")]
	Hair,

	[Display(Name = "Horns")]
	Horn,

	[Display(Name = "Innerwear")]
	Innerwear,

	[Display(Name = "Outerwear")]
	Outerwear,

	[Display(Name = "Skin Textures")]
	Skin,

	[Display(Name = "Stickers")]
	Sticker,

	[Display(Name = "Teeth")]
	Teeth,
}

public static class CmxObjectIds
{
	public const int ClassicStart = 0;
	public const int ClassicMaleCostumeStart = 0;
	public const int ClassicFemaleCostumeStart = 10000;
	public const int ClassicMaleStart = 20000;
	public const int ClassicFemaleStart = 30000;
	public const int ClassicCastStart = 40000;
	public const int ClassicCasealStart = 50000;
	public const int ClassicUnknownStart = 60000;
	public const int NgsStart = 100000;
	public const int NgsT1Start = 100000;
	public const int NgsT2Start = 200000;
	public const int NgsCastStart = 300000;
	public const int NgsCasealStart = 400000;
	public const int NgsGenderlessStart = 500000;
	public const int NgsUnknownStart = 600000;

	public static bool IsNgs(int id) => id >= NgsStart;

	public static bool IsT1(int id) =>
		(id >= ClassicMaleCostumeStart && id < ClassicFemaleCostumeStart)
		|| (id >= ClassicMaleStart && id < ClassicFemaleStart)
		|| (id >= ClassicCastStart && id < ClassicCasealStart)
		|| (id >= NgsT1Start && id < NgsT2Start)
		|| (id >= NgsCastStart && id < NgsCasealStart);

	public static bool IsT2(int id) =>
		(id >= ClassicFemaleCostumeStart && id < ClassicMaleStart)
		|| (id >= ClassicFemaleStart && id < ClassicCastStart)
		|| (id >= ClassicCasealStart && id < ClassicUnknownStart)
		|| (id >= NgsT2Start && id < NgsCastStart)
		|| (id >= NgsCasealStart && id < NgsGenderlessStart);

	public static bool IsNonGendered(int id) => !IsT1(id) && !IsT2(id);

	public static bool IsCast(int id) =>
		(id >= ClassicCastStart && id < ClassicUnknownStart)
		|| (id >= NgsCastStart && id < NgsGenderlessStart);
}

public class CmxColorMapping
{
	public CharColorMapping R { get; set; }
	public CharColorMapping G { get; set; }
	public CharColorMapping B { get; set; }
	public CharColorMapping A { get; set; }
}

public interface ICmxEntry
{
	public CmxObjectType ObjectType { get; }
	public int Id { get; }
	public int FileId { get; }
	public CmxNames Names { get; }
	public BaseCMXObject Data { get; }
	public IEnumerable<IceFileInfo> IceFiles { get; }

	public string Name => Names.En ?? Names.Jp ?? $"Unnamed {Id}";
}

public class BaseCmxEntry<T> : ICmxEntry
	where T : BaseCMXObject, new()
{
	public CmxObjectType ObjectType { get; set; }
	public int Id { get; set; }
	public int FileId { get; set; }
	public CmxNames Names { get; set; } = new();
	public T Data { get; set; } = new();

	public virtual IEnumerable<IceFileInfo> IceFiles
	{
		get
		{
			var normal = new IceFileInfo(ObjectType, FileId) { Description = "Main" };
			var hq = normal.Ex;
			hq.Description = "High quality";

			var icon = new IceFileInfo(
				CharacterMakingDynamic.icon,
				IceFileInfo.GetIconName(ObjectType, FileId)
			)
			{
				Description = "Icon",
			};

			return [normal, hq, icon];
		}
	}

	BaseCMXObject ICmxEntry.Data => Data;
}

public class CmxAccessoryEntry : BaseCmxEntry<ACCEObject> { }

public class CmxBodypaintEntry : BaseCmxEntry<BBLYObject> { }

public class CmxBodyEntry : BaseCmxEntry<BODYObject>
{
	public override IEnumerable<IceFileInfo> IceFiles
	{
		get
		{
			var main = new IceFileInfo(ObjectType, FileId) { Description = "Model" };

			yield return main;
			yield return main.Ex;

			var icon = new IceFileInfo(
				CharacterMakingDynamic.icon,
				IceFileInfo.GetIconName(ObjectType, FileId)
			)
			{
				Description = "Icon",
			};

			yield return icon;

			var replacement = new IceFileInfo(main.Name.Replace(".ice", "_rp.ice"))
			{
				Description = "Replacement",
			};

			yield return replacement;
			yield return replacement.Ex;

			var materialAnim = new IceFileInfo("bm", FileId) { Description = "Material anim" };

			yield return materialAnim;
			yield return materialAnim.Ex;

			if (!CmxObjectIds.IsNgs(FileId))
			{
				var hand = new IceFileInfo("hn", FileId) { Description = "Hand texture" };
				yield return hand;
			}

			if (Data.body2.linkedInnerId >= 0)
			{
				var linkedInner = new IceFileInfo(
					main.Start,
					CmxObjectType.Innerwear,
					Data.body2.linkedInnerId
				)
				{
					Description = "Linked inner",
				};

				yield return linkedInner;
				yield return linkedInner.Ex;
			}

			// Some objects have an ID like 30000 here which doesn't seem to match any file.
			if (Data.body2.linkedOuterId >= CmxObjectIds.NgsStart)
			{
				var linkedOuter = new IceFileInfo(
					main.Start,
					CmxObjectType.Outerwear,
					Data.body2.linkedOuterId
				)
				{
					Description = "Linked outer",
				};

				yield return linkedOuter;
				yield return linkedOuter.Ex;
			}

			if (Data.body2.headId >= 0)
			{
				var isCast = CmxObjectIds.IsCast(Data.body2.headId);
				var headType = isCast ? CmxObjectType.Face : CmxObjectType.Hair;

				var head = new IceFileInfo(headType, Data.body2.headId)
				{
					Description = isCast ? "Face model" : "Hair model",
				};

				yield return head;
				yield return head.Ex;
			}

			if (Data.body2.costumeSoundId >= 0)
			{
				yield return new IceFileInfo("bs", Data.body2.costumeSoundId)
				{
					Description = "Sounds",
				};

				if (!CmxObjectIds.IsNgs(Data.body2.costumeSoundId))
				{
					yield return new IceFileInfo("ls", Data.body2.costumeSoundId)
					{
						Description = "CAST sounds",
					};
				}
			}
		}
	}
}

public class CmxEarEntry : BaseCmxEntry<NGS_EarObject> { }

public class CmxEyeEntry : BaseCmxEntry<EYEObject> { }

public class CmxEyebrowEntry : BaseCmxEntry<EYEBObject> { }

public class CmxFaceEntry : BaseCmxEntry<FACEObject> { }

public class CmxFacepaintEntry : BaseCmxEntry<FCPObject> { }

public class CmxFaceTextureEntry : BaseCmxEntry<FaceTextureObject> { }

public class CmxHairEntry : BaseCmxEntry<HAIRObject> { }

public class CmxHornEntry : BaseCmxEntry<NGS_HornObject> { }

public class CmxSkinEntry : BaseCmxEntry<NGS_SKINObject> { }

public class CmxStickerEntry : BaseCmxEntry<StickerObject> { }

public class CmxTeethEntry : BaseCmxEntry<NGS_TeethObject> { }
