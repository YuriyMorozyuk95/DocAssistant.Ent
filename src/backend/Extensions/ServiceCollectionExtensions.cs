// Copyright (c) Microsoft. All rights reserved.

using System.Net;

using Azure;
using Azure.Core.Pipeline;
using Azure.Search.Documents.Indexes;

namespace MinimalApi.Extensions;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddAzureServices(this IServiceCollection services)
    {
        services.AddSingleton<BlobServiceClient>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var azureStorageAccountConnectionString = config["AzureStorageAccountConnectionString"];
            ArgumentNullException.ThrowIfNullOrEmpty(azureStorageAccountConnectionString);

            var blobServiceClient = new BlobServiceClient(azureStorageAccountConnectionString);

            return blobServiceClient;
        });

        services.AddSingleton<BlobContainerClient>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var azureStorageContainer = config["AzureStorageContainer"];
            return sp.GetRequiredService<BlobServiceClient>().GetBlobContainerClient(azureStorageContainer);
        });

        services.AddSingleton<SearchClient>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var (azureSearchServiceEndpoint, azureSearchIndex, key) =
                (config["AzureSearchServiceEndpoint"], config["AzureSearchIndex"], config["AzureSearchServiceEndpointKey"]);

            ArgumentNullException.ThrowIfNullOrEmpty(azureSearchServiceEndpoint);

            var credential = new AzureKeyCredential(key!);

            var searchClient = new SearchClient(
                new Uri(azureSearchServiceEndpoint), azureSearchIndex, credential, new SearchClientOptions
                {
                    Transport = new HttpClientTransport(new HttpClient(new HttpClientHandler()
                    {
                        Proxy = new WebProxy()
                        {
                            BypassProxyOnLocal = false,
                            UseDefaultCredentials = true,
                        }
                    }))
                });


            return searchClient;
        });

        services.AddSingleton<SearchIndexClient>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var (azureSearchServiceEndpoint, key) =
                (config["AzureSearchServiceEndpoint"], config["AzureSearchServiceEndpointKey"]);

            var credential = new AzureKeyCredential(key!);

            var searchIndexClient = new SearchIndexClient(
                new Uri(azureSearchServiceEndpoint!),
                credential, new SearchClientOptions
                {
                    Transport = new HttpClientTransport(new HttpClient(new HttpClientHandler()
                    {
                        Proxy = new WebProxy()
                        {
                            BypassProxyOnLocal = false,
                            UseDefaultCredentials = true,
                        }
                    }))
                });

            return searchIndexClient;
        });

        services.AddSingleton<DocumentAnalysisClient>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var azureOpenAiServiceEndpoint = config["AzureDocumentIntelligenceEndpoint"] ?? throw new ArgumentNullException();
            var key = config["AzureDocumentIntelligenceEndpointKey"] ?? throw new ArgumentNullException();

            var credential = new AzureKeyCredential(key!);

            var documentAnalysisClient = new DocumentAnalysisClient(
                new Uri(azureOpenAiServiceEndpoint), credential, new DocumentAnalysisClientOptions{
                    Transport = new HttpClientTransport(new HttpClient(new HttpClientHandler()
                    {
                        Proxy = new WebProxy()
                        {
                            BypassProxyOnLocal = false,
                            UseDefaultCredentials = true,
                        }
                    }))
                });
            return documentAnalysisClient;
        });

        services.AddSingleton<OpenAIClient>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var (azureOpenAiServiceEndpoint, key) = (config["AzureOpenAiServiceEndpoint"], config["AzureOpenAiServiceEndpointKey"]);

            ArgumentNullException.ThrowIfNullOrEmpty(azureOpenAiServiceEndpoint);

            var credential = new AzureKeyCredential(key!);

            var openAiClient = new OpenAIClient(
                new Uri(azureOpenAiServiceEndpoint), credential);

            return openAiClient;
        });

        services.AddSingleton<AzureBlobStorageService>();
        services.AddSingleton<ReadRetrieveReadChatService>();
        services.AddSingleton<IUploaderDocumentService, UploaderDocumentService>();
        services.AddSingleton<IAzureSearchEmbedService, AzureSearchAzureSearchEmbedService>();

        return services;
    }

    internal static IServiceCollection AddCrossOriginResourceSharing(this IServiceCollection services)
    {
        services.AddCors(
            options =>
                options.AddDefaultPolicy(
                    policy =>
                        policy.AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod()));

        return services;
    }
}
