// Copyright (c) Microsoft. All rights reserved.

using Microsoft.AspNetCore.Components.Authorization;
using Shared.TableEntities;

namespace ClientApp.Services;

public class AuthenticatedUserService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly IUserApiClient _userApiClient;

    public AuthenticatedUserService(AuthenticationStateProvider authenticationStateProvider, IUserApiClient userApiClient)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _userApiClient = userApiClient;
    }

    public async Task<UserEntity> GetAuthenticatedUserAsync()
    {
        var email = await GetAuthenticatedUserNameAsync();
        var users = await _userApiClient.GetAllUsers();

        return users.FirstOrDefault(x => x.Email == email);
    }
    private async Task<string> GetAuthenticatedUserNameAsync()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        string email = string.Empty;
        if (user.Identity.IsAuthenticated)
        {
            var nicknameClaim = user.Claims.FirstOrDefault(c => c.Type == "nickname");
            if (nicknameClaim != null)
            {
                var nickname = nicknameClaim.Value;
                email = $"{nickname}@gmail.com";
            }
        }

        return email;
    }
}
