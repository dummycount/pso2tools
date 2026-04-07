using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Pso2Tools;

public partial class GameFinder
{
	public static string? FindPso2BinPath()
	{
		if (Directory.Exists(WindowsStorePath))
		{
			return WindowsStorePath;
		}

		foreach (var library in GetSteamLibraries())
		{
			var path = Path.GetFullPath(
				Path.Join(library, "SteamApps/common/PHANTASYSTARONLINE2_NA_STEAM/pso2_bin")
			);

			if (Directory.Exists(path))
			{
				return path;
			}
		}

		return null;
	}

	private static string ProgramFiles =>
		Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
	private static string WindowsStorePath =>
		Path.Join(ProgramFiles, "ModifiableWindowsApps/pso2_bin");

	private static IEnumerable<string> GetSteamLibraries()
	{
		var path = PathRegex();
		var steamLibrariesFile = Path.Join(ProgramFiles, "Steam/SteamApps/libraryfolders.vdf");

		try
		{
			var file = File.ReadAllText(steamLibrariesFile);
			return file.Split('\n')
				.Select(line => path.Match(line))
				.Where(m => m.Success)
				.Select(m => m.Groups[1].Value.Replace(@"\\", @"\"));
		}
		catch (FileNotFoundException)
		{
			return [];
		}
	}

	[GeneratedRegex(@"\s*""path""\s*""([^""]+)""\s*")]
	private static partial Regex PathRegex();
}
