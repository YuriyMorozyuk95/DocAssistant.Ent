// Copyright (c) Microsoft. All rights reserved.

namespace ClientApp.Models;

public record RequestSettingsOverrides
{
    public Approach Approach { get; set; }
    public SearchParameters Overrides { get; set; } = new();
}
