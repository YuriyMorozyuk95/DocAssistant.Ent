// Copyright (c) Microsoft. All rights reserved.

using Microsoft.AspNetCore.Http;

using Shared.TableEntities;

namespace Shared;

public class PermissionsAndFilesRequest  
{  
    public PermissionEntity[] Permissions { get; set; }  
    public IFormFileCollection Files { get; set; }  
}
