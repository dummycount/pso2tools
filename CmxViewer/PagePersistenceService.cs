using System.Collections.Generic;

namespace Pso2Tools.CmxViewer;

public class PagePersistenceData
{
	public string? FilterText { get; set; }
	public double? ScrollOffset { get; set; }
}

public class PagePersistenceService
{
	private readonly Dictionary<string, PagePersistenceData> values = [];

	public PagePersistenceData? Get(string key)
	{
		return values.TryGetValue(key, out var value) ? value : null;
	}

	public PagePersistenceData? Get(CmxObjectType objectType) => Get(objectType.ToString());

	public void Set(string key, PagePersistenceData value)
	{
		values[key] = value;
	}

	public void Set(CmxObjectType objectType, PagePersistenceData data) =>
		Set(objectType.ToString(), data);
}
