// Copyright (c) Microsoft. All rights reserved.

using EmbedFunctions.Services;

using Microsoft.Extensions.Configuration;

s_rootCommand.SetHandler(
    async (context) =>
    {
        var consoleAppOptions =  Configuration.Get<ConsoleAppOptions>();
        if (consoleAppOptions.RemoveAll)
        {
            await RemoveBlobsAsync(consoleAppOptions);
            await RemoveFromIndexAsync(consoleAppOptions);
        }
        else
        {
           
            var searchIndexName = Configuration["AzureSearchIndex"];
            var embedService = await GetAzureSearchEmbedService(consoleAppOptions);

            await embedService.EnsureSearchIndexAsync(consoleAppOptions.SearchIndexName);

            Matcher matcher = new();
            matcher.AddInclude(consoleAppOptions.Files);

            var results = matcher.Execute(
                new DirectoryInfoWrapper(
                    new DirectoryInfo(Directory.GetCurrentDirectory())));

            var files = results.HasMatches
                ? results.Files.Select(f => f.Path).ToArray()
                : Array.Empty<string>();

            context.Console.WriteLine($"Processing {files.Length} files...");

            var tasks = Enumerable.Range(0, files.Length)
                .Select(i =>
                {
                    var fileName = files[i];
                    return ProcessSingleFileAsync(consoleAppOptions, fileName, embedService);
                });

            await Task.WhenAll(tasks);

            // ReSharper disable once InconsistentNaming
            static async Task ProcessSingleFileAsync(ConsoleAppOptions options, string fileName, IEmbedService embedService)
            {
                if (options.Verbose)
                {
                    Console.WriteLine($"Processing '{fileName}'");
                }

                if (options.Remove)
                {
                    await RemoveBlobsAsync(options, fileName);
                    await RemoveFromIndexAsync(options, fileName);
                    return;
                }

                if (options.SkipBlobs)
                {
                    return;
                }

                await UploadBlobsAndCreateIndexAsync(options, fileName, embedService);
            }
        }
    });

return await s_rootCommand.InvokeAsync(args);

// ReSharper disable once InconsistentNaming
static async ValueTask RemoveBlobsAsync(
    ConsoleAppOptions options, string? fileName = null)
{
    if (options.Verbose)
    {
        Console.WriteLine($"Removing blobs for '{fileName ?? "all"}'");
    }

    var prefix = string.IsNullOrWhiteSpace(fileName)
        ? Path.GetFileName(fileName)
        : null;

    var getContainerClientTask = GetBlobContainerClient(options);
    var getCorpusClientTask = GetCorpusBlobContainerClient(options);
    var clientTasks = new[] { getContainerClientTask, getCorpusClientTask };

    await Task.WhenAll(clientTasks);

    foreach (var clientTask in clientTasks)
    {
        var client = await clientTask;
        await DeleteAllBlobsFromContainerAsync(client, prefix);
    }

    // ReSharper disable once MoveLocalFunctionAfterJumpStatement
    // ReSharper disable once InconsistentNaming
    static async Task DeleteAllBlobsFromContainerAsync(BlobContainerClient client, string? prefix)
    {
        await foreach (var blob in client.GetBlobsAsync())
        {
            if (string.IsNullOrWhiteSpace(prefix) ||
                blob.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                await client.DeleteBlobAsync(blob.Name);
            }
        }
    };
}

// ReSharper disable once InconsistentNaming
static async ValueTask RemoveFromIndexAsync(
    ConsoleAppOptions options, string? fileName = null)
{
    if (options.Verbose)
    {
        Console.WriteLine($"""
            Removing sections from '{fileName ?? "all"}' from search index '{options.SearchIndexName}.'
            """);
    }

    var searchClient = await GetSearchClient(options);

    while (true)
    {
        var filter = (fileName is null) ? null : $"sourcefile eq '{Path.GetFileName(fileName)}'";

        var response = await searchClient.SearchAsync<SearchDocument>("",
            new SearchOptions
            {
                Filter = filter,
                Size = 1_000,
                IncludeTotalCount = true
            });

        var documentsToDelete = new List<SearchDocument>();
        await foreach (var result in response.Value.GetResultsAsync())
        {
            documentsToDelete.Add(new SearchDocument
            {
                ["id"] = result.Document["id"]
            });
        }

        if (documentsToDelete.Count == 0)
        {
            break;
        }
        Response<IndexDocumentsResult> deleteResponse =
            await searchClient.DeleteDocumentsAsync(documentsToDelete);

        if (options.Verbose)
        {
            Console.WriteLine($"""
                    Removed {deleteResponse.Value.Results.Count} sections from index
                """);
        }

        // It can take a few seconds for search results to reflect changes, so wait a bit
        await Task.Delay(TimeSpan.FromMilliseconds(2_000));
    }
}

// ReSharper disable once InconsistentNaming
static async ValueTask UploadBlobsAndCreateIndexAsync(
    ConsoleAppOptions options, string fileName, IEmbedService embeddingService)
{
    var container = await GetBlobContainerClient(options);

    // If it's a PDF, split it into single pages.
    if (Path.GetExtension(fileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
    {
        using var documents = PdfReader.Open(fileName, PdfDocumentOpenMode.Import);
        for (int i = 0; i < documents.PageCount; i++)
        {
            var documentName = BlobNameFromFilePage(fileName, i);
            var blobClient = container.GetBlobClient(documentName);
            if (await blobClient.ExistsAsync())
            {
                continue;
            }

            var tempFileName = Path.GetTempFileName();

            try
            {
                using var document = new PdfDocument();
                document.AddPage(documents.Pages[i]);
                document.Save(tempFileName);

                await using var stream = File.OpenRead(tempFileName);
                await blobClient.UploadAsync(stream, new BlobHttpHeaders
                {
                    ContentType = "application/pdf"
                });

                // revert stream position
                stream.Position = 0;

                await embeddingService.EmbedBlobAsync(stream, documentName);
            }
            finally
            {
                File.Delete(tempFileName);
            }
        }
    }
    else
    {
        var blobName = BlobNameFromFilePage(fileName);
        await UploadBlobAsync(fileName, blobName, container);
        await embeddingService.EmbedBlobAsync(File.OpenRead(fileName), blobName);
    }
}

// ReSharper disable once InconsistentNaming
static async Task UploadBlobAsync(string fileName, string blobName, BlobContainerClient container)
{
    var blobClient = container.GetBlobClient(blobName);
    if (await blobClient.ExistsAsync())
    {
        return;
    }

    var blobHttpHeaders = new BlobHttpHeaders
    {
        ContentType = GetContentType(fileName)
    };

    await using var fileStream = File.OpenRead(fileName);
    await blobClient.UploadAsync(fileStream, blobHttpHeaders);
}

static string GetContentType(string fileName)
{
    var extension = Path.GetExtension(fileName);
    return extension switch
    {
        ".pdf" => "application/pdf",
        ".txt" => "text/plain",

        _ => "application/octet-stream"
    };
}

static string BlobNameFromFilePage(string filename, int page = 0) => Path.GetExtension(filename).ToLower() is ".pdf"
        ? $"{Path.GetFileNameWithoutExtension(filename)}-{page}.pdf"
        : Path.GetFileName(filename);

internal static partial class Program
{
    [GeneratedRegex("[^0-9a-zA-Z_-]")]
    private static partial Regex MatchInSetRegex();

    internal static DefaultAzureCredential DefaultCredential { get; } = new();
}
