using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using UnluacNET;

namespace Pso2Tools.Defrost.ViewModels.Preview;

public partial class PreviewModelLua(NotificationService notificationService) : ObservableObject
{
	[ObservableProperty]
	public partial string Text { get; set; }

	[ObservableProperty]
	public partial bool IsLoading { get; set; } = false;

	public async Task LoadFileAsync(IceDataFile file)
	{
		try
		{
			IsLoading = true;

			Text = await GetScriptTextAsync(file.Data.ToArray());
		}
		catch (Exception ex)
		{
			notificationService.ShowNotification(
				new()
				{
					Title = "Failed to parse Lua script",
					Message = ex.Message,
					Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error,
					Duration = TimeSpan.FromSeconds(10),
				}
			);
		}
		finally
		{
			IsLoading = false;
		}
	}

	private static readonly byte[] LuaHeader = [0x1B, 0x4C, 0x75, 0x61];

	private static async Task<string> GetScriptTextAsync(byte[] data)
	{
		return await Task.Run(() => GetScriptText(data));
	}

	private static string GetScriptText(byte[] data)
	{
		if (data.StartsWith(LuaHeader))
		{
			return Decompile(data);
		}

		return Encoding.UTF8.GetString(data);
	}

	private static string Decompile(byte[] data)
	{
		using var stream = new MemoryStream(data);
		var header = new BHeader(stream);

		var main = header.Function.Parse(stream, header);
		var decompiler = new Decompiler(main);

		decompiler.Decompile();

		using var writer = new StringWriter();
		decompiler.Print(new Output(writer));

		writer.Flush();

		return writer.ToString();
	}
}
