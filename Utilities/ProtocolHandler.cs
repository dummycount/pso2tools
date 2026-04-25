using System.Runtime.Versioning;
using Microsoft.Win32;

namespace Pso2Tools;

public static class ProtocolHandler
{
	[SupportedOSPlatform("windows")]
	public static void Register(
		string scheme,
		string description,
		string executable,
		string args = "\"%1\""
	)
	{
		using var key = Registry.CurrentUser.CreateSubKey(GetProtocolKey(scheme));

		key.SetValue("", description);
		key.SetValue("URL Protocol", "");

		using var commandKey = key.CreateSubKey(@"shell\open\command");
		commandKey.SetValue("", GetCommand(executable, args));
	}

	[SupportedOSPlatform("windows")]
	public static void Unregister(string scheme)
	{
		Registry.CurrentUser.DeleteSubKeyTree(GetProtocolKey(scheme));
	}

	[SupportedOSPlatform("windows")]
	public static bool IsRegistered(string scheme)
	{
		using var key = Registry.CurrentUser.OpenSubKey(GetProtocolKey(scheme));

		return key is not null && key.GetValue("URL Protocol") is not null;
	}

	private static string GetProtocolKey(string scheme) => $@"SOFTWARE\Classes\{scheme}";

	private static string GetCommand(string executable, string args) => $"\"{executable}\" {args}";
}
