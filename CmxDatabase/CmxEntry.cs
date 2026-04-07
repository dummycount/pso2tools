using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using AquaModelLibrary.Data.LegacyObj;
using AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData;
using AquaModelLibrary.Data.PSO2.Constants;

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

	public static string GetFilePathStart(int id) =>
		IsNgs(id) ? CharacterMakingDynamic.rebootStart : CharacterMakingDynamic.classicStart;
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
	public int Id { get; set; }
	public int FileId { get; set; }
	public CmxNames Names { get; set; }
	public BaseCMXObject Data { get; set; }

	public string Name { get; }
}

public class CmxEntry<T> : ICmxEntry
	where T : BaseCMXObject
{
	public required int Id { get; set; }
	public required int FileId { get; set; }
	public required CmxNames Names { get; set; }
	public required T Data { get; set; }

	public string Name => Names.En ?? Names.Jp ?? $"Unnamed {Id}";

	BaseCMXObject ICmxEntry.Data
	{
		get => Data;
		set => throw new NotImplementedException();
	}
}
