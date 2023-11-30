// Copyright (c) Microsoft. All rights reserved.

using Shared.TableEntities;

namespace ClientApp.Services;

public interface IUserApiClient
{
    Task<IEnumerable<UserEntity>> GetAllUsers(bool refreshRequired = false);
    Task<UserEntity> GetUserDetails(int employeeId);
    Task UpdateUser(UserEntity employee);
    Task DeleteUser(string employeeId);
}

public class UserApiClient : IUserApiClient  
{  
    private readonly HttpClient _httpClient;  
  
    public UserApiClient(HttpClient httpClient)  
    {  
        _httpClient = httpClient;  
    }  
  
    public async Task<IEnumerable<UserEntity>> GetAllUsers(bool refreshRequired = false)  
    {  
        if (refreshRequired)  
        {  
            var response = await _httpClient.GetAsync("api/users/refresh");  
            response.EnsureSuccessStatusCode();  
        }  
  
        var usersResponse = await _httpClient.GetAsync("api/users");  
        usersResponse.EnsureSuccessStatusCode();  
        return await usersResponse.Content.ReadFromJsonAsync<IEnumerable<UserEntity>>();  
    }  
  
    public async Task<UserEntity> GetUserDetails(int employeeId)  
    {  
        var response = await _httpClient.GetAsync($"api/users/{employeeId}");  
        response.EnsureSuccessStatusCode();  
        return await response.Content.ReadFromJsonAsync<UserEntity>();  
    }  
  
    public async Task UpdateUser(UserEntity employee)  
    {  
        var response = await _httpClient.PutAsJsonAsync($"api/users/{employee.Id}", employee);  
        response.EnsureSuccessStatusCode();  
    }  
  
    public async Task DeleteUser(string employeeId)  
    {  
        var response = await _httpClient.DeleteAsync($"api/users/{employeeId}");  
        response.EnsureSuccessStatusCode();  
    }  
}  

