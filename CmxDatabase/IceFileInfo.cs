using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using AquaModelLibrary.Data.PSO2.Constants;

namespace Pso2Tools;

public class IceFileInfo(string name)
{
	public static readonly IceFileInfo None = new("");

	public static implicit operator bool(IceFileInfo x) => x.Name != "";

	public static string MD5Digest(string data)
	{
		var source = Encoding.UTF8.GetBytes(data);
		var hash = MD5.HashData(source);
		return Convert.ToHexString(hash);
	}

	public string Name { get; } = name;
	public string Hash { get; } = MD5Digest(name);

	public IceFileInfo(string start, string tag, int adjustedId)
		: this($"{start}{tag}_{adjustedId:05d}.ice") { }

	public IceFileInfo Ex
	{
		get
		{
			if (!Name.StartsWith(CharacterMakingDynamic.rebootStart))
			{
				return IceFileInfo.None;
			}

			return new IceFileInfo(
				Name.Replace(
						CharacterMakingDynamic.rebootStart,
						CharacterMakingDynamic.rebootExStart
					)
					.Replace(".ice", "_ex.ice")
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
}
