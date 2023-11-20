// Copyright (c) Microsoft. All rights reserved.


using EmbedFunctions.Services;

using Microsoft.Extensions.Configuration;

internal static partial class Program
{
    private static BlobContainerClient? s_corpusContainerClient;
    private static BlobContainerClient? s_containerClient;
    private static DocumentAnalysisClient? s_documentClient;
    private static SearchIndexClient? s_searchIndexClient;
    private static SearchClient? s_searchClient;
    private static OpenAIClient? s_openAiClient;

    private static readonly SemaphoreSlim s_corpusContainerLock = new(1);
    private static readonly SemaphoreSlim s_containerLock = new(1);
    private static readonly SemaphoreSlim s_documentLock = new(1);
    private static readonly SemaphoreSlim s_searchIndexLock = new(1);
    private static readonly SemaphoreSlim s_searchLock = new(1);
    private static readonly SemaphoreSlim s_openAiLock = new(1);
    private static readonly SemaphoreSlim s_embeddingLock = new(1);

    public static IConfiguration Configuration { get; set; }

    //TODO change to Shared I Configuration class
    public static IConfiguration GetConfiguration()
    {
        var builder = new ConfigurationBuilder()  
            .SetBasePath(Directory.GetCurrentDirectory())  
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);  
  
        IConfiguration configuration = builder.Build();

        return configuration;
    }

    private static Task<AzureSearchEmbedService> GetAzureSearchEmbedService(ConsoleAppOptions options) =>
        GetLazyClientAsync<AzureSearchEmbedService>(options, s_embeddingLock, async o =>
        {
            Configuration = GetConfiguration();

            var searchIndexClient = await GetSearchIndexClient(o);
            var searchClient = await GetSearchClient(o);
            var documentClient = await GetFormRecognizerClient(o);
            var blobContainerClient = await GetCorpusBlobContainerClient(o);
            var openAiClient = await GetAzureOpenAiClient(o);
            var embeddingModelName = Configuration["AZURE_OPENAI_EMBEDDING_DEPLOYMENT"];
            var searchIndexName = Configuration["AzureSearchIndex"];

            return new AzureSearchEmbedService(openAiClient, embeddingModelName, searchClient, searchIndexName, searchIndexClient, documentClient, blobContainerClient, null);
        });


    private static Task<BlobContainerClient> GetCorpusBlobContainerClient(ConsoleAppOptions options) =>
        GetLazyClientAsync<BlobContainerClient>(options, s_corpusContainerLock, static async o =>
        {
            if (s_corpusContainerClient is null)
            {
                var connectionString = Configuration["AzureStorageAccountConnectionString"];
                ArgumentNullException.ThrowIfNullOrEmpty(connectionString);

                var blobService = new BlobServiceClient(connectionString);

                var azureStorageContainer = Configuration["AzureStorageContainer"];

                s_corpusContainerClient = blobService.GetBlobContainerClient(azureStorageContainer);

                await s_corpusContainerClient.CreateIfNotExistsAsync();
            }

            return s_corpusContainerClient;
        });

    private static Task<BlobContainerClient> GetBlobContainerClient(ConsoleAppOptions options) =>
        GetLazyClientAsync<BlobContainerClient>(options, s_containerLock, static async o =>
        {
            if (s_containerClient is null)
            {
                var endpoint = o.StorageServiceBlobEndpoint;
                ArgumentNullException.ThrowIfNullOrEmpty(endpoint);

                var blobService = new BlobServiceClient(
                    new Uri(endpoint),
                    DefaultCredential);

                var blobContainerName = o.Container;
                ArgumentNullException.ThrowIfNullOrEmpty(blobContainerName);

                s_containerClient = blobService.GetBlobContainerClient(blobContainerName);

                await s_containerClient.CreateIfNotExistsAsync();
            }

            return s_containerClient;
        });

    private static Task<DocumentAnalysisClient> GetFormRecognizerClient(ConsoleAppOptions options) =>
        GetLazyClientAsync<DocumentAnalysisClient>(options, s_documentLock, static async o =>
        {
            if (s_documentClient is null)
            {
                var azureOpenAiServiceEndpoint = Configuration["AzureDocumentIntelligenceEndpoint"] ?? throw new ArgumentNullException();
                var key = Configuration["AzureDocumentIntelligenceEndpointKey"] ?? throw new ArgumentNullException();

                var credential = new AzureKeyCredential(key!);

                s_documentClient = new DocumentAnalysisClient(
                    new Uri(azureOpenAiServiceEndpoint),
                    credential,
                    new DocumentAnalysisClientOptions
                    {
                        Diagnostics =
                        {
                            IsLoggingContentEnabled = true
                        }
                    });
            }

            await Task.CompletedTask;

            return s_documentClient;
        });

    private static Task<SearchIndexClient> GetSearchIndexClient(ConsoleAppOptions options) =>
        GetLazyClientAsync<SearchIndexClient>(options, s_searchIndexLock, static async o =>
        {
            if (s_searchIndexClient is null)
            {
                var (azureSearchServiceEndpoint, azureSearchIndex, key) =
                    (Configuration["AzureSearchServiceEndpoint"], Configuration["AzureSearchIndex"], Configuration["AzureSearchServiceEndpointKey"]);

                var endpoint = o.SearchServiceEndpoint;


                ArgumentNullException.ThrowIfNullOrEmpty(endpoint);

                var credential = new AzureKeyCredential(key!);

                s_searchIndexClient = new SearchIndexClient(
                    new Uri(azureSearchServiceEndpoint),
                    credential);
            }

            await Task.CompletedTask;

            return s_searchIndexClient;
        });

    private static Task<SearchClient> GetSearchClient(ConsoleAppOptions options) =>
        GetLazyClientAsync<SearchClient>(options, s_searchLock, async o =>
        {
            if (s_searchClient is null)
            {
                var (azureSearchServiceEndpoint, azureSearchIndex, key) =
                    (Configuration["AzureSearchServiceEndpoint"], Configuration["AzureSearchIndex"], Configuration["AzureSearchServiceEndpointKey"]);

                ArgumentNullException.ThrowIfNullOrEmpty(azureSearchServiceEndpoint);

                var credential = new AzureKeyCredential(key!);

                s_searchClient = new SearchClient(
                    new Uri(azureSearchServiceEndpoint), azureSearchIndex, credential);
            }

            await Task.CompletedTask;

            return s_searchClient;
        });

    private static Task<OpenAIClient> GetAzureOpenAiClient(ConsoleAppOptions options) =>
       GetLazyClientAsync<OpenAIClient>(options, s_openAiLock, async o =>
       {
           if (s_openAiClient is null)
           {
               var (azureOpenAiServiceEndpoint, key) = (Configuration["AzureOpenAiServiceEndpoint"], Configuration["AzureOpenAiServiceEndpointKey"]);
               ArgumentNullException.ThrowIfNullOrEmpty(azureOpenAiServiceEndpoint);

               var credential = new AzureKeyCredential(key!);

               s_openAiClient = new OpenAIClient(
                   new Uri(azureOpenAiServiceEndpoint),
                   credential);
           }
           await Task.CompletedTask;
           return s_openAiClient;
       });

    private static async Task<TClient> GetLazyClientAsync<TClient>(
        ConsoleAppOptions options,
        SemaphoreSlim locker,
        Func<ConsoleAppOptions, Task<TClient>> factory)
    {
        await locker.WaitAsync();

        try
        {
            return await factory(options);
        }
        finally
        {
            locker.Release();
        }
    }
}
