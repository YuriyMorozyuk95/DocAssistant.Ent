// Copyright (c) Microsoft. All rights reserved.

using Shared.TableEntities;

namespace ClientApp.Services;

public interface IUserApiClient
{
    Task<IEnumerable<UserEntity>> GetAllUsers();
    Task<UserEntity> GetUserDetails(int userId);
    Task UpdateUser(UserEntity user);
    Task DeleteUser(string userId);
}

public class UserApiClient : IUserApiClient  
{  
    private readonly HttpClient _httpClient;  
  
    public UserApiClient(HttpClient httpClient)  
    {  
        _httpClient = httpClient;  
    }  
  
    public async Task<IEnumerable<UserEntity>> GetAllUsers()  
    {
        var usersResponse = await _httpClient.GetAsync("api/users");  
        usersResponse.EnsureSuccessStatusCode();  
        return await usersResponse.Content.ReadFromJsonAsync<IEnumerable<UserEntity>>();  
    }  
  
    public async Task<UserEntity> GetUserDetails(int userId)  
    {  
        var response = await _httpClient.GetAsync($"api/users/{userId}");  
        response.EnsureSuccessStatusCode();  
        return await response.Content.ReadFromJsonAsync<UserEntity>();  
    }  
  
    public async Task UpdateUser(UserEntity user)  
    {  
        var response = await _httpClient.PutAsJsonAsync($"api/users/{user.Id}", user);  
        response.EnsureSuccessStatusCode();  
    }  
  
    public async Task DeleteUser(string userId)  
    {  
        var response = await _httpClient.DeleteAsync($"api/users/{userId}");  
        response.EnsureSuccessStatusCode();  
    }  
}  

