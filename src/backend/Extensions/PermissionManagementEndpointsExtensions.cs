// Copyright (c) Microsoft. All rights reserved.

using DocAssistant.Data.Interfaces;
using Shared.TableEntities;

namespace MinimalApi.Extensions;

internal static class PermissionManagementEndpointsExtensions
{
    internal static void MapPermissionManagementApi(this RouteGroupBuilder api)  
    {  
        // Permission management endpoints    
        api.MapGet("permissions", OnGetAllPermissionsAsync);
        api.MapGet("permissions/{permissionId}", OnGetPermissionByIdAsync);
        api.MapDelete("permissions/{permissionId}", OnDeletePermissionAsync);    
        api.MapPut("permissions", OnSaveChangesAsync);      
    }  
  
    private static async Task OnGetAllPermissionsAsync(HttpContext context, IPermissionRepository repository)  
    {
        var permissions = await repository.GetAllPermissionsAsync();
        await context.Response.WriteAsJsonAsync(permissions);
    }

    private static async Task OnGetPermissionByIdAsync(HttpContext context, IPermissionRepository repository)
    {
        var permissionId = context.Request.RouteValues["permissionId"]?.ToString();
        var permissions = await repository.GetPermissionByIdAsync(permissionId);
        await context.Response.WriteAsJsonAsync(permissions);
    }

    private static async Task OnDeletePermissionAsync(HttpContext context, IPermissionRepository repository)  
    {
        var permissionId = context.Request.RouteValues["permissionId"]?.ToString();
        await repository.DeletePermissionAsync(permissionId);

        context.Response.StatusCode = StatusCodes.Status204NoContent;
    }  
  
    private static async Task OnSaveChangesAsync(HttpContext context, IPermissionRepository repository)  
    {
        var permissions = await context.Request.ReadFromJsonAsync<IEnumerable<PermissionEntity>>();
        await repository.SavePermissionAsync(permissions);
        context.Response.StatusCode = StatusCodes.Status204NoContent;
    }  
}
