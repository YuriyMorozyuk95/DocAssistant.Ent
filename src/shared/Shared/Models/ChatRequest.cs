// Copyright (c) Microsoft. All rights reserved.

namespace Shared.Models;

public class ChatRequest : ApproachRequest
{
    public ChatTurn[] History { get; set; }
    public Approach Approach { get; set; }
    public SearchParameters? Overrides { get; set; }

    public ChatRequest(ChatTurn[] history, Approach approach, SearchParameters? overrides = null)
        : base(approach)
    {
        History = history;
        Approach = approach;
        Overrides = overrides;
    }

    public string? LastUserQuestion
    {
        get
        {
            return History?.LastOrDefault()?.User;
        }
    }
}

