using DocAssistant.Data.Interfaces;
using Microsoft.Azure.Cosmos;
using Shared.TableEntities;

namespace DocAssistant.Data;


public class PermissionRepository : IPermissionRepository
{
    protected readonly Container Container;
    public PermissionRepository(Container container)
    {
        Container = container;
    }

    public async Task<PermissionEntity> GetPermissionByIdAsync(string id, string? name)
    {
        ItemResponse<PermissionEntity> response = await Container.ReadItemAsync<PermissionEntity>(id, new PartitionKey(name));
        return response.Resource;

    }

    public async Task<PermissionEntity> AddPermissionAsync(PermissionEntity permission)
    {

        permission.Id = Guid.NewGuid().ToString();
        var response = await Container.CreateItemAsync(permission, new PartitionKey(permission.Name));

        return response.Resource;
    }
}
