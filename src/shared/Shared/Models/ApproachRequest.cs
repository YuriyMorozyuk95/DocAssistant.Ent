// Copyright (c) Microsoft. All rights reserved.

namespace Shared.Models;

public class ApproachRequest
{
    public Approach Approach { get; set; }

    public ApproachRequest(Approach approach)
    {
        Approach = approach;
    }
}

