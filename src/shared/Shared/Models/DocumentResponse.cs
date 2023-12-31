﻿// Copyright (c) Microsoft. All rights reserved.

namespace Shared.Models;

public class DocumentResponse(string name, string contentType, long size, DateTimeOffset? lastModified, Uri url, DocumentProcessingStatus status, EmbeddingType embeddingType, string permissions)
{
    public string Name { get; set; } = name;
    public string ContentType { get; set; } = contentType;
    public long Size { get; set; } = size;
    public DateTimeOffset? LastModified { get; set; } = lastModified;
    public Uri Url { get; set; } = url;
    public DocumentProcessingStatus Status { get; set; } = status;
    public EmbeddingType EmbeddingType { get; set; } = embeddingType;
    public string Permissions { get; set; } = permissions;
}
