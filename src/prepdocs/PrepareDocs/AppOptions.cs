// Copyright (c) Microsoft. All rights reserved.

namespace PrepareDocs;

internal record class AppOptions(
    string Files,
    string? Category,
    bool SkipBlobs,
    string? StorageServiceBlobEndpoint,
    string? Container,
    string? TenantId,
    string? SearchServiceEndpoint,
    string? AzureOpenAiServiceEndpoint,
    string? SearchIndexName,
    string? EmbeddingModelName,
    bool Remove,
    bool RemoveAll,
    string? FormRecognizerServiceEndpoint,
    bool Verbose,
    IConsole Console) : AppConsole(Console);

internal record class AppConsole(IConsole Console);

public class ConsoleAppOptions
{
    public string Files { get; set; }
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
