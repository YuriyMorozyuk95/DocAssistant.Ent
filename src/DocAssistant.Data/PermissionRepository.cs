using DocAssistant.Data.Interfaces;
using Microsoft.Azure.Cosmos;
using Shared.TableEntities;

namespace DocAssistant.Data;


public class PermissionRepository : IPermissionRepository
{
    private readonly Container _container;

    public PermissionRepository(Container container)
    {
        _container = container;
    }

    public async Task<PermissionEntity> GetPermissionByIdAsync(string id)
    {
        var response = await _container.ReadItemAsync<PermissionEntity>(id, new PartitionKey(id));
        return response.Resource;
    }

    public async Task<IEnumerable<PermissionEntity>> GetAllPermissionsAsync()
    {
        var query = new QueryDefinition("SELECT * FROM c");
        var resultSetIterator = _container.GetItemQueryIterator<PermissionEntity>(query);
        List<PermissionEntity> results = new List<PermissionEntity>();
        while (resultSetIterator.HasMoreResults)
        {
            var response = await resultSetIterator.ReadNextAsync();
            results.AddRange(response.ToList());
        }

        return results;
    }

    public async Task DeletePermissionAsync(string id)
    {
        await _container.DeleteItemAsync<PermissionEntity>(id, new PartitionKey(id));
    }

    public async Task SavePermissionAsync(IEnumerable<PermissionEntity> permissions)
    {
        var existingPermissions = await GetAllPermissionsAsync();

        foreach (var permission in permissions)
        {
            var existingPermission = existingPermissions.FirstOrDefault(p => p.Id == permission.Id);
            if (existingPermission == null)  
            {
                permission.PartitionKey = permission.Id;
                await _container.CreateItemAsync(permission, new PartitionKey(permission.Id));
            }
            else 
            {
                if (!existingPermission.Equals(permission))  
                {
                    await _container.ReplaceItemAsync(permission, permission.Id, new PartitionKey(permission.Id));
                }
            }
        }

        foreach (var existingPermission in existingPermissions)
        {
            if (!permissions.Any(p => p.Id == existingPermission.Id))
            {
                await _container.DeleteItemAsync<PermissionEntity>(existingPermission.Id,
                    new PartitionKey(existingPermission.Id));
            }
        }
    }
}
