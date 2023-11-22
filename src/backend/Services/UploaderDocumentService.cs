// Copyright (c) Microsoft. All rights reserved.

using Azure.Search.Documents.Indexes;

namespace MinimalApi.Services;

public interface IUploaderDocumentService
{
    IAsyncEnumerable<DocumentResponse> GetDocuments(CancellationToken cancellationToken);
    Task UploadToAzureIndex(CancellationToken cancellationToken);
}
public class UploaderDocumentService : IUploaderDocumentService
{
    private readonly BlobContainerClient _blobContainerClient;
    private readonly SearchIndexClient _searchClient;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<UploaderDocumentService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IAzureSearchEmbedService _azureSearchEmbedService;

    public UploaderDocumentService(
        BlobContainerClient blobContainerClient,
        SearchIndexClient searchClient,
        BlobServiceClient blobServiceClient,
        ILogger<UploaderDocumentService> logger,
        IConfiguration configuration,
        IAzureSearchEmbedService azureSearchEmbedService)
    {
        _blobContainerClient = blobContainerClient;
        _searchClient = searchClient;
        _blobServiceClient = blobServiceClient;
        _logger = logger;
        _configuration = configuration;
        _azureSearchEmbedService = azureSearchEmbedService;
    }

    public async IAsyncEnumerable<DocumentResponse> GetDocuments(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var blob in _blobContainerClient.GetBlobsAsync(cancellationToken: cancellationToken))
        {
            if (blob is not null and { Deleted: false })
            {
                var props = blob.Properties;
                var baseUri = _blobContainerClient.Uri;
                var builder = new UriBuilder(baseUri);
                builder.Path += $"/{blob.Name}";

                var metadata = blob.Metadata;
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
        var embeddingModel = _configuration["AZURE_OPENAI_EMBEDDING_DEPLOYMENT"];

        var consoleAppOptions = new UploaderOptions();
        _configuration.GetSection("UploaderOptions").Bind(consoleAppOptions);

        _logger?.LogInformation("Deleting '{searchIndexName}' search index", searchIndexName);  

        //Re-create search index
        await _azureSearchEmbedService.RemoveSearchIndex(searchIndexName!);
        await _azureSearchEmbedService.CreateSearchIndex(searchIndexName!);

        await foreach(var document in GetDocuments(cancellationToken))
        {
            var stream = await GetBlobStreamAsync(document);

            await _azureSearchEmbedService.EmbedBlob(stream, document.Name, searchIndexName!, embeddingModel!);

            //TODO update document metdata
        }
    }

    private async Task<Stream> GetBlobStreamAsync(DocumentResponse document)
    {
        var blobClient = _blobContainerClient.GetBlobClient(document.Name);

        if (await blobClient.ExistsAsync())
        {
            BlobDownloadInfo download = await blobClient.DownloadAsync();
            return download.Content;
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
