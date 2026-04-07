using System;
using System.Collections.Generic;
using System.Text;
using Zamboni;

namespace Pso2Tools;

public class IceDataFile(byte[] data)
{
	public string Name => IceFile.getFileName(data);
	public ReadOnlySpan<byte> Data => GetData(data);

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
		WrappedFile.groupOneFiles.Select(data => new IceDataFile(data));

	public IEnumerable<IceDataFile> GroupTwo =>
		WrappedFile.groupTwoFiles.Select(data => new IceDataFile(data));

	public IEnumerable<IceDataFile> Files => GroupOne.Concat(GroupTwo);

	public IEnumerable<IceDataFile> FindByName(string name)
	{
		return Files.Where(f => f.Name == name);
	}

	public static IceWrapper Load(string path)
	{
		using var stream = new FileStream(path, FileMode.Open);

		return new IceWrapper(IceFile.LoadIceFile(stream));
	}
}
