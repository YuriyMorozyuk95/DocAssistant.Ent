// Copyright (c) Microsoft. All rights reserved.

using Shared.TableEntities;

namespace ClientApp.Services;

public interface IPermissionApiClient  
{  
    Task<IEnumerable<PermissionEntity>> GetAllPermissions();  
    Task DeletePermission(string permissionId);  
    Task SaveChanges(IEnumerable<PermissionEntity> permissions);  
}  
  
public class PermissionApiClient : IPermissionApiClient    
{    
    private readonly HttpClient _httpClient;    
    
    public PermissionApiClient(HttpClient httpClient)    
    {    
        _httpClient = httpClient;    
    }    
    
    public async Task<IEnumerable<PermissionEntity>> GetAllPermissions()    
    {
        var permissionsResponse = await _httpClient.GetAsync("api/permissions");    
        permissionsResponse.EnsureSuccessStatusCode();    
        return await permissionsResponse.Content.ReadFromJsonAsync<IEnumerable<PermissionEntity>>();    
    }    
    
    public async Task DeletePermission(string permissionId)    
    {    
        var response = await _httpClient.DeleteAsync($"api/permissions/{permissionId}");    
        response.EnsureSuccessStatusCode();    
    }    
  
    public async Task SaveChanges(IEnumerable<PermissionEntity> permissions)    
    {    
        var response = await _httpClient.PutAsJsonAsync("api/permissions", permissions);    
        response.EnsureSuccessStatusCode();    
    }    
}    
