using System;
using System.Threading.Tasks;
using HelixToolkit.SharpDX;

namespace Pso2Tools.Defrost;

public class EffectsManagerService : IAsyncDisposable
{
	private Task<IEffectsManager>? value;

	public Task<IEffectsManager> GetEffectsManagerAsync()
	{
		value ??= Task.Run(() => new DefaultEffectsManager() as IEffectsManager);

		return value;
	}

	public async ValueTask DisposeAsync()
	{
		if (value is not null)
		{
			(await value).Dispose();
			value = null;
		}
	}
}
