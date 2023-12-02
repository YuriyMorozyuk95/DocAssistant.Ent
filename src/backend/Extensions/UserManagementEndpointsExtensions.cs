// Copyright (c) Microsoft. All rights reserved.

using DocAssistant.Data.Interfaces;
using Shared.TableEntities;

namespace MinimalApi.Extensions;

internal static class UserManagementEndpointsExtensions
{
    internal static void MapUserManagementApi(this RouteGroupBuilder api)
    {
        // User management endpoints  
        api.MapGet("users", OnGetAllUsersAsync);    
        api.MapGet("users/{userId}", OnGetUserDetailsAsync);    
        api.MapPut("users/{userId}", OnUpdateUserAsync);
        api.MapPost("users", OnAddUserAsync);
        api.MapDelete("users/{userId}", OnDeleteUserAsync);  
    }

    private static async Task OnUpdateUserAsync(HttpContext context, IUserRepository repository)
    {
        var user = await context.Request.ReadFromJsonAsync<UserEntity>();
        await repository.UpdateUserAsync(user);
        context.Response.StatusCode = StatusCodes.Status204NoContent;
    }

    private static async Task OnAddUserAsync(HttpContext context, IUserRepository repository)
    {
        var user = await context.Request.ReadFromJsonAsync<UserEntity>();
        await repository.AddUserAsync(user);
        context.Response.StatusCode = StatusCodes.Status204NoContent;
    }

    private static async Task OnGetUserDetailsAsync(HttpContext context, IUserRepository repository)
    {
        var userId = context.Request.RouteValues["userId"]?.ToString();
        var permissions = await repository.GetUserByIdAsync(userId);
        await context.Response.WriteAsJsonAsync(permissions);
    }

    private static async Task OnGetAllUsersAsync(HttpContext context, IUserRepository repository)
    {
        var permissions = await repository.GetAllUsersAsync();
        await context.Response.WriteAsJsonAsync(permissions);
    }

    private static async Task OnDeleteUserAsync(HttpContext context, IUserRepository repository)
    {
        var userId = context.Request.RouteValues["userId"]?.ToString();
        await repository.DeleteUserAsync(userId);

        context.Response.StatusCode = StatusCodes.Status204NoContent;
    }
}
