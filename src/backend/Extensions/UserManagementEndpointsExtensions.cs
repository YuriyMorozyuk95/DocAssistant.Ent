// Copyright (c) Microsoft. All rights reserved.

namespace MinimalApi.Extensions;

internal static class UserManagementEndpointsExtensions
{
    internal static void MapUserManagementApi(this RouteGroupBuilder api)
    {
        // User management endpoints  
        api.MapGet("users", OnGetAllUsersAsync);    
        api.MapGet("users/{employeeId}", OnGetUserDetailsAsync);    
        api.MapPut("users/{employeeId}", OnUpdateUserAsync);    
        api.MapDelete("users/{employeeId}", OnDeleteUserAsync);  
    }

    private static Task OnUpdateUserAsync(HttpContext context)
    {
        throw new NotImplementedException();
    }

    private static Task OnGetUserDetailsAsync(HttpContext context)
    {
        throw new NotImplementedException();
    }

    private static Task OnGetAllUsersAsync(HttpContext context)
    {
        throw new NotImplementedException();
    }

    private static Task OnDeleteUserAsync(HttpContext context)
    {
        throw new NotImplementedException();
    }
}
