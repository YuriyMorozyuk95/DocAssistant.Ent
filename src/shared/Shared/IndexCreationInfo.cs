// Copyright (c) Microsoft. All rights reserved.

namespace Shared;

public class IndexCreationInfo
{
    public IndexCreationInfo()
    {
        LastIndexStatus = IndexStatus.NotStarted;
        ChunksProcessed = 0;
        DocumentPageProcessed = 0;
    }
    public IndexStatus LastIndexStatus { get; set; }
    public string LastIndexErrorMessage { get; set; }
    public int ChunksProcessed { get; set; } = default;
    public int DocumentPageProcessed { get; set; } = default;
    public int DocumentPageProcessedBuffer => DocumentPageProcessed + 2;
    public int TotalPageCount { get; set; } = default;
}

public enum IndexStatus
{
    Processing = 0,
    Succeeded = 1,
    Failed = 2,
    NotStarted = 3,
}

