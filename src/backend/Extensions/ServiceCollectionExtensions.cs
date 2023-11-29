// Copyright (c) Microsoft. All rights reserved.

using System.Net;

using Azure;
using Azure.Core.Pipeline;
using Azure.Search.Documents.Indexes;
using DocAssistant.Data;
using DocAssistant.Data.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Cosmos;
using Microsoft.IdentityModel.Tokens;

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

        services.AddSingleton<IStorageService, StorageService>();

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

    internal static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                var authority = $"https://{configuration["Auth0:Domain"]}";
                options.Authority = authority;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidAudience = configuration["Auth0:Audience"],
                    ValidIssuer = authority
                };
            });

        // Cosmos Db configuration
#pragma warning disable CA2000
        var cosmosClient = GetCosmosClient(configuration);
#pragma warning restore CA2000
        var cosmosDbName = configuration["CosmosDB:Name"];
        var userContainer =
            cosmosClient.GetContainer(cosmosDbName, "User");
        var permissionContainer =
            cosmosClient.GetContainer(cosmosDbName, "Permissions");

        services.AddSingleton<IUserRepository, UserRepository>(_ =>
            new UserRepository(userContainer));
        services.AddSingleton<IPermissionRepository, PermissionRepository>(_ =>
            new PermissionRepository(permissionContainer));

        return services;
    }

    private static CosmosClient GetCosmosClient(IConfiguration configuration)
    {
        var cosmosDbConnectionUri = configuration["CosmosDB:EndpointUrl"];
        var cosmosDbKey = configuration["CosmosDB:Key"];

        var cosmosClientOptions = new CosmosClientOptions
        {
            AllowBulkExecution = true,
            MaxRetryAttemptsOnRateLimitedRequests = 20,
            MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(60)
        };

        var cosmosClient = new CosmosClient(cosmosDbConnectionUri, cosmosDbKey, cosmosClientOptions);
        return cosmosClient;
    }
}
