using Zamboni;

namespace Pso2Tools;

public class IceDataFile(byte[] data, int group)
{
	private readonly Lazy<string> name = new(() => IceFile.getFileName(data));

	public string Name => name.Value;
	public ReadOnlySpan<byte> Data => GetData(data);
	public int Group => group;

	private static ReadOnlySpan<byte> GetData(ReadOnlySpan<byte> data)
	{
		var headerSize = BitConverter.ToInt32(data[0x0C..0x10]);

		return data[headerSize..];
	}
}

public class IceWrapper(IceFile file)
{
	public IceFile WrappedFile { get; } = file;

	public IEnumerable<IceDataFile> GroupOne =>
		WrappedFile.groupOneFiles.Select(data => new IceDataFile(data, 1));

	public IEnumerable<IceDataFile> GroupTwo =>
		WrappedFile.groupTwoFiles.Select(data => new IceDataFile(data, 2));

	public IEnumerable<IceDataFile> Files => [.. GroupOne, .. GroupTwo];

	public IEnumerable<IceDataFile> FindByName(string name)
	{
		return Files.Where(f => f.Name == name);
	}

	public static IceWrapper Load(string path)
	{
		using var stream = new FileStream(path, FileMode.Open);

		return Load(stream);
	}

	public static IceWrapper Load(Stream stream)
	{
		return new IceWrapper(IceFile.LoadIceFile(stream));
	}

	public static Task<IceWrapper> LoadAsync(string path)
	{
		return Task.Run(() => Load(path));
	}

	public static Task<IceWrapper> LoadAsync(Stream stream)
	{
		return Task.Run(() => Load(stream));
	}
}
