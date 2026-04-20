using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace Pso2Tools.Defrost;

public enum CollisionOption
{
	[Display(Name = "Ask before overwriting")]
	Ask,

	[Display(Name = "Overwrite")]
	Overwrite,

	[Display(Name = "Skip")]
	Skip,

	[Display(Name = "Rename new file")]
	Rename,
}

public enum CollisionResponse
{
	Skip,
	SkipAll,
	Overwrite,
	OverwriteAll,
}

public class ExtractItem
{
	public required string FileName { get; set; }
	public required byte[] Data { get; set; }
}

public class ExtractProgress
{
	public required string CurrentFileName { get; set; }
	public required long ExtractedBytes { get; set; }
	public required long TotalBytes { get; set; }
	public required int ExtractedCount { get; set; }
	public required int TotalCount { get; set; }

	public double ProgressPercent => TotalBytes == 0 ? 0 : ExtractedBytes * 100.0 / TotalBytes;
}

public class CollisionData
{
	public required string FileName { get; set; }
	public required StorageFolder DestFolder { get; set; }
}

public static class FileOperations
{
	public static async Task<StorageFile> CreateStreamedFileAsync(string name, byte[] data)
	{
		return await StorageFile.CreateStreamedFileAsync(
			name,
			(request) => OnStreamedDataRequested(request, data),
			null
		);
	}

	private static async void OnStreamedDataRequested(StreamedFileDataRequest request, byte[] data)
	{
		try
		{
			using var outputStream = request.AsStreamForWrite();
			await outputStream.WriteAsync(data);
			await outputStream.FlushAsync();
		}
		catch (Exception)
		{
			request.FailAndClose(StreamedFileFailureMode.Failed);
		}
	}

	public static async Task<StorageFolder> GetOrCreateStorageFolderAsync(string path)
	{
		try
		{
			return await StorageFolder.GetFolderFromPathAsync(path);
		}
		catch (FileNotFoundException)
		{
			var parentPath = Path.GetDirectoryName(path);
			if (parentPath is null)
			{
				throw;
			}

			var parent = await GetOrCreateStorageFolderAsync(parentPath);

			return await parent.CreateFolderAsync(Path.GetFileName(path));
		}
	}

	public static string GetExtractPath(string name, int group, bool useGroupFolders)
	{
		return useGroupFolders ? Path.Join($"group{group}", name) : name;
	}

	public static async Task ExtractAsync(
		StorageFolder destFolder,
		IEnumerable<ExtractItem> items,
		IProgress<ExtractProgress> progress,
		CollisionOption collisionOption,
		Func<CollisionData, CancellationToken, Task<CollisionResponse>> showCollisionPrompt,
		CancellationToken cancellationToken
	)
	{
		CollisionResponse? response = null;

		var totalBytes = items.Sum(item => item.Data.Length);
		var extractedBytes = 0;
		var totalFiles = items.Count();
		var extractedFiles = 0;

		void reportProgress(string name)
		{
			progress.Report(
				new ExtractProgress
				{
					CurrentFileName = name,
					ExtractedBytes = extractedBytes,
					TotalBytes = totalBytes,
					ExtractedCount = extractedFiles,
					TotalCount = totalFiles,
				}
			);
		}

		foreach (var item in items)
		{
			cancellationToken.ThrowIfCancellationRequested();

			reportProgress(item.FileName);

			var (file, itemResponse) = await GetExtractDestinationFile(
				destFolder,
				item,
				collisionOption,
				response,
				showCollisionPrompt,
				cancellationToken
			);

			response = itemResponse;

			if (file is not null)
			{
				using var stream = await file.OpenStreamForWriteAsync();

				await stream.WriteAsync(item.Data, cancellationToken);
				await stream.FlushAsync(cancellationToken);

				extractedBytes += item.Data.Length;
			}

			extractedFiles++;
		}

		reportProgress("Complete");
	}

	private static async Task<(
		StorageFile?,
		CollisionResponse? response
	)> GetExtractDestinationFile(
		StorageFolder destFolder,
		ExtractItem item,
		CollisionOption collisionOption,
		CollisionResponse? response,
		Func<CollisionData, CancellationToken, Task<CollisionResponse>> showCollisionPrompt,
		CancellationToken cancellationToken
	)
	{
		var creationCollisionOption = collisionOption switch
		{
			CollisionOption.Ask => response switch
			{
				CollisionResponse.OverwriteAll => CreationCollisionOption.ReplaceExisting,
				_ => CreationCollisionOption.FailIfExists,
			},
			CollisionOption.Overwrite => CreationCollisionOption.ReplaceExisting,
			CollisionOption.Rename => CreationCollisionOption.GenerateUniqueName,
			_ => CreationCollisionOption.FailIfExists,
		};

		try
		{
			var dest = await destFolder.CreateFileAsync(item.FileName, creationCollisionOption);

			return (dest, response);
		}
		catch (Exception)
		{
			if (collisionOption == CollisionOption.Ask && response != CollisionResponse.SkipAll)
			{
				response = await showCollisionPrompt(
					new CollisionData { DestFolder = destFolder, FileName = item.FileName },
					cancellationToken
				);

				switch (response)
				{
					case CollisionResponse.Overwrite:
					case CollisionResponse.OverwriteAll:
						var dest = await destFolder.CreateFileAsync(
							item.FileName,
							CreationCollisionOption.ReplaceExisting
						);
						return (dest, response);
				}
			}

			return (null, response);
		}
	}
}
