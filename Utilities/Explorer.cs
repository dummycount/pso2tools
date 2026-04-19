using System.Diagnostics;

namespace Pso2Tools;

public static class Explorer
{
	public static void OpenFolder(string path)
	{
		var process = new ProcessStartInfo
		{
			FileName = "explorer",
			Arguments = $"/e, \"{Path.GetFullPath(path)}\"",
		};

		Process.Start(process);
	}

	public static void OpenAndSelect(string path)
	{
		var process = new ProcessStartInfo
		{
			FileName = "explorer",
			Arguments = $"/e, /select, \"{Path.GetFullPath(path)}\"",
		};

		Process.Start(process);
	}
}
