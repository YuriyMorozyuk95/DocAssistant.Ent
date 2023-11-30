// Copyright (c) Microsoft. All rights reserved.

namespace MinimalApi.Extensions;

internal static class PermissionManagementEndpointsExtensions  
{  
    internal static void MapPermissionManagementApi(this RouteGroupBuilder api)  
    {  
        // Permission management endpoints    
        api.MapGet("permissions", OnGetAllPermissionsAsync);      
        api.MapDelete("permissions/{permissionId}", OnDeletePermissionAsync);    
        api.MapPut("permissions", OnSaveChangesAsync);      
    }  
  
    private static Task OnGetAllPermissionsAsync(HttpContext context)  
    {  
        throw new NotImplementedException();  
    }  
  
    private static Task OnDeletePermissionAsync(HttpContext context)  
    {  
        throw new NotImplementedException();  
    }  
  
    private static Task OnSaveChangesAsync(HttpContext context)  
    {  
        throw new NotImplementedException();  
    }  
}
