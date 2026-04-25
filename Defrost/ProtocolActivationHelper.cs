using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Web;

namespace Pso2Tools.Defrost;

internal class ActivationArgs
{
	public string? FilePath { get; set; }
	public string? ExtractSuffix { get; set; }
}

internal class ProtocolActivationHelper
{
	public const string Scheme = "pso2defrost";

	public static bool IsRegistered() => ProtocolHandler.IsRegistered(Scheme);

	public static void Register()
	{
		var process = Process.GetCurrentProcess();
		var path =
			(process.MainModule?.FileName) ?? throw new Exception("Could not get process path");

		ProtocolHandler.Register(Scheme, "Open in Defrost", path);
	}

	public static void Unregister()
	{
		ProtocolHandler.Unregister(Scheme);
	}

	public static ActivationArgs ParseCommandLine()
	{
		var args = Environment.GetCommandLineArgs()[1..];

		Argument<string> pathOrUriArgument = new("path")
		{
			Description = "File to open",
			Arity = ArgumentArity.ZeroOrOne,
		};

		Option<string> suffixOption = new("--suffix")
		{
			Description = "Descriptive suffix for extracting files",
		};

		RootCommand rootCommand = new("PSO2 ICE archive tool") { pathOrUriArgument, suffixOption };

		var parsed = rootCommand.Parse(args);

		if (parsed.Errors.Count != 0)
		{
			foreach (var error in parsed.Errors)
			{
				Console.Error.WriteLine(error.Message);
			}
			return new();
		}

		ActivationArgs result = new() { ExtractSuffix = parsed.GetValue(suffixOption) };

		if (parsed.GetValue(pathOrUriArgument) is string path)
		{
			if (path.StartsWith("pso2defrost://"))
			{
				var uri = new Uri(path);

				if (uri.Host == "file")
				{
					result.FilePath = uri.AbsolutePath.RemovePrefix("/");
				}

				var query = HttpUtility.ParseQueryString(uri.Query);

				var suffix = query["suffix"];
				if (suffix is not null)
				{
					result.ExtractSuffix = suffix;
				}
			}
			else
			{
				result.FilePath = path;
			}
		}

		return result;
	}
}
