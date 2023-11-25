// Copyright (c) Microsoft. All rights reserved.

using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;

namespace MinimalApi.Services;

public interface IUploaderDocumentService
{
    IAsyncEnumerable<DocumentResponse> GetDocuments(CancellationToken cancellationToken);
    Task UploadToAzureIndex(CancellationToken cancellationToken);
}
public class UploaderDocumentService : IUploaderDocumentService
{
    private readonly SearchIndexClient _searchClient;
    private readonly ILogger<UploaderDocumentService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IAzureSearchEmbedService _azureSearchEmbedService;
    private readonly IStorageService _storageService;

    public UploaderDocumentService(
        SearchIndexClient searchClient,
        ILogger<UploaderDocumentService> logger,
        IConfiguration configuration,
        IAzureSearchEmbedService azureSearchEmbedService,
        IStorageService storageService)
    {
        _searchClient = searchClient;
        _logger = logger;
        _configuration = configuration;
        _azureSearchEmbedService = azureSearchEmbedService;
        _storageService = storageService;
    }

    public async IAsyncEnumerable<DocumentResponse> GetDocuments(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var container = await _storageService.GetInputBlobContainerClient();
        await foreach (var blob in container.GetBlobsAsync(cancellationToken: cancellationToken))
        {
            if (blob is not null and { Deleted: false })
            {
                var props = blob.Properties;
                var baseUri = container.Uri;
                var builder = new UriBuilder(baseUri);
                builder.Path += $"/{blob.Name}";

                BlobClient blobClient = container.GetBlobClient(blob.Name);  
                Response<BlobProperties> response = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);  
                IDictionary<string, string> metadata = response.Value.Metadata;

                var documentProcessingStatus = GetMetadataEnumOrDefault(
                    metadata,
                    nameof(DocumentProcessingStatus),
                    DocumentProcessingStatus.NotProcessed);

                var embeddingType = GetMetadataEnumOrDefault(
                    metadata,
                    nameof(EmbeddingType),
                    EmbeddingType.AzureSearch);

                yield return new DocumentResponse(
                    blob.Name,
                    props.ContentType,
                    props.ContentLength ?? 0,
                    props.LastModified,
                    builder.Uri,
                    documentProcessingStatus,
                    embeddingType);
            }
        }
    }

    public async Task UploadToAzureIndex(CancellationToken cancellationToken)
    {
        var searchIndexName = _configuration["AzureSearchIndex"];
        var embeddingModel = _configuration["AzureOpenAiEmbeddingDeployment"];

        var consoleAppOptions = new UploaderOptions();
        _configuration.GetSection("UploaderOptions").Bind(consoleAppOptions);

        _logger?.LogInformation("Deleting '{searchIndexName}' search index", searchIndexName);  

        //Re-create search index
        await _azureSearchEmbedService.RemoveSearchIndex(searchIndexName!);
        await _azureSearchEmbedService.CreateSearchIndex(searchIndexName!);

        var inputContainer = await _storageService.GetInputBlobContainerClient();

        await foreach(var document in GetDocuments(cancellationToken))
        {
            var fileName = document.Name;
            var blobClient = inputContainer.GetBlobClient(document.Name);

            var stream = await GetBlobStreamAsync(blobClient);
            if (!stream.CanRead || !stream.CanSeek)  
            {  
                throw new NotSupportedException("The stream must be readable and seekable.");  
            }
            using var documents = PdfReader.Open(stream, PdfDocumentOpenMode.Import);

            for (int i = 0; i < documents.PageCount; i++)
            {
                var chunkName = BlobNameFromFilePage(fileName, i);
                var tempFileName = Path.GetTempFileName();

                try
                {
                    using var pdfDocument = new PdfDocument();
                    pdfDocument.AddPage(documents.Pages[i]);
                    pdfDocument.Save(tempFileName);

                    await using var tempStream = File.OpenRead(tempFileName);
                    await _azureSearchEmbedService.EmbedBlob(tempStream, chunkName, searchIndexName!, embeddingModel!);

                    //Add metadata
                }
                finally
                {
                    File.Delete(tempFileName);
                }
            }

            //TODO add DocumentProcessingStatus.Failed in case of fail
            BlobProperties properties = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);  
            var metadata = properties.Metadata;

            metadata[nameof(DocumentProcessingStatus)] = DocumentProcessingStatus.Succeeded.ToString();
            metadata[nameof(EmbeddingType)] = EmbeddingType.AzureSearch.ToString();

            await blobClient.SetMetadataAsync(metadata, cancellationToken: cancellationToken); 
        }
    }

    private async Task<Stream> GetBlobStreamAsync(BlobClient blobClient)
    {
        if (await blobClient.ExistsAsync())
        {
            BlobDownloadInfo download = await blobClient.DownloadAsync();

            var memoryStream = new MemoryStream();  
            await download.Content.CopyToAsync(memoryStream);  
            memoryStream.Position = 0;  
            return memoryStream; 
        }
        else
        {
            throw new FileNotFoundException("The blob does not exist.");
        }
    }

    private TEnum GetMetadataEnumOrDefault<TEnum>(
        IDictionary<string, string> metadata,
        string key,
        TEnum @default) where TEnum : struct
    {
        return metadata.TryGetValue(key, out var value)
               && Enum.TryParse<TEnum>(value, out var status)
            ? status
            : @default;
    }

    private static string BlobNameFromFilePage(string filename, int page = 0) =>
        Path.GetExtension(filename).ToLower() is ".pdf"
            ? $"{Path.GetFileNameWithoutExtension(filename)}-{page}.pdf"
            : Path.GetFileName(filename);
}

public class UploaderOptions
{
    public string? Category { get; set; }
    public bool SkipBlobs { get; set; }
    public string? StorageServiceBlobEndpoint { get; set; }
    public string? Container { get; set; }
    public string? TenantId { get; set; }
    public string? SearchServiceEndpoint { get; set; }
    public string? AzureOpenAiServiceEndpoint { get; set; }
    public string? SearchIndexName { get; set; }
    public string? EmbeddingModelName { get; set; }
    public bool Remove { get; set; }
    public bool RemoveAll { get; set; }
    public string? FormRecognizerServiceEndpoint { get; set; }
    public bool Verbose { get; set; }
}
