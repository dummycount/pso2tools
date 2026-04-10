using System.Security.Cryptography;
using System.Text;
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
			);
		}
	}

	public string Win32Path(string pso2BinPath)
	{
		return Path.Join(pso2BinPath, "data/win32", Hash);
	}

	public string Win32RebootPath(string pso2BinPath)
	{
		return Path.Join(pso2BinPath, "data/win32reboot", Hash.Insert(2, "/"));
	}

	public string Win32NaPath(string pso2BinPath)
	{
		return Path.Join(pso2BinPath, "data/win32_na", Hash);
	}

	public string Win32RebootNaPath(string pso2BinPath)
	{
		return Path.Join(pso2BinPath, "data/win32reboot_na", Hash.Insert(2, "/"));
	}

	public string? FindFile(string pso2BinPath)
	{
		if (!this)
		{
			return null;
		}

		var path = Win32Path(pso2BinPath);
		if (File.Exists(path))
		{
			return path;
		}

		path = Win32NaPath(pso2BinPath);
		if (File.Exists(path))
		{
			return path;
		}

		path = Win32RebootPath(pso2BinPath);
		if (File.Exists(path))
		{
			return path;
		}

		path = Win32RebootNaPath(pso2BinPath);
		if (File.Exists(path))
		{
			return path;
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
}
