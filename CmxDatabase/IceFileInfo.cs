using System.Security.Cryptography;
using System.Text;
using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Constants;

namespace Pso2Tools;

public class IceFileInfo
{
	public static readonly IceFileInfo None = new("");

	public static implicit operator bool(IceFileInfo x) => x.Name != "";

	public static string MD5Digest(string data)
	{
		var source = Encoding.UTF8.GetBytes(data);
		var hash = MD5.HashData(source);
		return Convert.ToHexString(hash).ToLower();
	}

	public string Start { get; }
	public string Rest { get; }

	public string Name { get; }
	public string Hash { get; }

	public string? Description { get; set; }
	public string DataDir { get; set; } = CharacterMakingIndex.dataDir;

	public IceFileInfo(string start, string rest)
	{
		Start = start;
		Rest = rest;

		Name = start + rest;
		Hash = MD5Digest(Name);
	}

	public IceFileInfo(string name)
		: this("", name) { }

	public IceFileInfo(string start, string tag, int id)
		: this(start, $"{tag}_{id:D5}.ice") { }

	public IceFileInfo(string tag, int id)
		: this(GetFilePathStart(id), tag, id) { }

	public IceFileInfo(string start, CmxObjectType objectType, int id)
		: this(start, GetFileTag(objectType), id) { }

	public IceFileInfo(CmxObjectType objectType, int id)
		: this(GetFilePathStart(id), objectType, id) { }

	public IceFileInfo Ex
	{
		get
		{
			if (!Name.StartsWith(CharacterMakingDynamic.rebootStart))
			{
				return None;
			}

			return new IceFileInfo(
				CharacterMakingDynamic.rebootExStart,
				Rest.Replace(".ice", "_ex.ice")
			)
			{
				Description = Description is null
					? null
					: $"HQ {Description.ToSentenceCaseLower()}",
			};
		}
	}

	public string GetPath(string pso2BinPath)
	{
		var hash = Hash;
		if (
			DataDir == CharacterMakingIndex.dataReboot
			|| DataDir == CharacterMakingIndex.dataRebootNA
		)
		{
			hash = hash.Insert(2, "\\");
		}

		return Path.Join(pso2BinPath, DataDir, hash);
	}

	public string? FindFile(string pso2BinPath)
	{
		if (this)
		{
			var path = GetPath(pso2BinPath);
			if (File.Exists(path))
			{
				return path;
			}
		}

		return null;
	}

	public string? FindFileRelative(string pso2BinPath)
	{
		if (FindFile(pso2BinPath) is string path)
		{
			return Path.GetRelativePath(pso2BinPath, path);
		}
		return null;
	}

	public static string GetFilePathStart(int id) =>
		CmxObjectIds.IsNgs(id)
			? CharacterMakingDynamic.rebootStart
			: CharacterMakingDynamic.classicStart;

	public static string GetFileTag(CmxObjectType objectType) =>
		objectType switch
		{
			CmxObjectType.Accessory => "ac",
			CmxObjectType.Basewear => "bw",
			CmxObjectType.Bodypaint => "b1",
			CmxObjectType.CastArms => "am",
			CmxObjectType.CastBody => "bd",
			CmxObjectType.CastLegs => "lg",
			CmxObjectType.Costume => "bd",
			CmxObjectType.Ear => "ea",
			CmxObjectType.Eye => "ey",
			CmxObjectType.Eyebrow => "eb",
			CmxObjectType.Eyelash => "el",
			CmxObjectType.Face => "fc",
			CmxObjectType.FaceTexture => "f1",
			CmxObjectType.Facepaint => "f2",
			CmxObjectType.Hair => "hr",
			CmxObjectType.Horn => "hn",
			CmxObjectType.Innerwear => "iw",
			CmxObjectType.Outerwear => "ow",
			CmxObjectType.Skin => "sk",
			CmxObjectType.Sticker => "b2",
			CmxObjectType.Teeth => "de",
			_ => throw new ArgumentException("Invalid object type"),
		};

	public static string GetIconTag(CmxObjectType objectType) =>
		objectType switch
		{
			CmxObjectType.Accessory => CharacterMakingDynamic.accessoryIcon,
			CmxObjectType.Basewear => CharacterMakingDynamic.basewearIcon,
			CmxObjectType.Bodypaint => CharacterMakingDynamic.bodyPaintIcon,
			CmxObjectType.CastArms => CharacterMakingDynamic.castArmIcon,
			CmxObjectType.CastBody => CharacterMakingDynamic.castPartIcon,
			CmxObjectType.CastLegs => CharacterMakingDynamic.castLegIcon,
			CmxObjectType.Costume => CharacterMakingDynamic.costumeIcon,
			CmxObjectType.Ear => CharacterMakingDynamic.earIcon,
			CmxObjectType.Eye => CharacterMakingDynamic.eyeIcon,
			CmxObjectType.Eyebrow => CharacterMakingDynamic.eyeBrowsIcon,
			CmxObjectType.Eyelash => CharacterMakingDynamic.eyelashesIcon,
			CmxObjectType.Face => CharacterMakingDynamic.faceIcon,
			CmxObjectType.FaceTexture => "facepaint01_", // ?
			CmxObjectType.Facepaint => CharacterMakingDynamic.facePaintIcon,
			CmxObjectType.Hair => CharacterMakingDynamic.hairIcon,
			CmxObjectType.Horn => CharacterMakingDynamic.hornIcon,
			CmxObjectType.Innerwear => CharacterMakingDynamic.innerwearIcon,
			CmxObjectType.Outerwear => CharacterMakingDynamic.outerwearIcon,
			CmxObjectType.Skin => CharacterMakingDynamic.skinIcon,
			CmxObjectType.Sticker => CharacterMakingDynamic.stickerIcon,
			CmxObjectType.Teeth => CharacterMakingDynamic.teethIcon,
			_ => throw new ArgumentException("Invalid object type"),
		};

	public static string GetIconGender(int id)
	{
		if (CmxObjectIds.IsT1(id))
		{
			return CharacterMakingDynamic.iconMale;
		}
		if (CmxObjectIds.IsT2(id))
		{
			return CharacterMakingDynamic.iconFemale;
		}
		return "";
	}

	public static string GetIconName(CmxObjectType objectType, int id)
	{
		switch (objectType)
		{
			case CmxObjectType.CastBody:
			case CmxObjectType.Costume:
				return $"{GetIconTag(objectType)}{id}.ice";

			case CmxObjectType.CastArms:
			case CmxObjectType.CastLegs:
				return $"{CharacterMakingDynamic.castPartIcon}{GetIconTag(objectType)}{id}.ice";

			case CmxObjectType.Basewear:
			case CmxObjectType.Innerwear:
			case CmxObjectType.Outerwear:
			case CmxObjectType.Skin:
				return $"{GetIconTag(objectType)}{GetIconGender(id)}{id}.ice";

			default:
				return $"{GetIconTag(objectType)}{id}.ice";
		}
	}
}
