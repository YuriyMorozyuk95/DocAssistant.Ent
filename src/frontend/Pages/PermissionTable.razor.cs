using Shared.TableEntities;

namespace ClientApp.Pages;

public sealed partial class PermissionTable
{
    private List<PermissionEntity> _permissions;

    [Inject]
    public IPermissionApiClient PermissionApiClient { get; set; }

    protected override async Task OnInitializedAsync()  
    {
        _permissions = (await PermissionApiClient.GetAllPermissions()).ToList();  
    }  
  
    private Task Create()  
    {

        var newPermission = new PermissionEntity()
        {   
            Id = Guid.NewGuid().ToString(),
            Name = "New Permission",
        };
        _permissions.Add(newPermission);
        return Task.CompletedTask;
    }

    private Task Delete(PermissionEntity permission)  
    {
        _permissions.Remove(permission);
        return Task.CompletedTask;
    }

    private async Task SaveChangesAsync()
    {
        await PermissionApiClient.SaveChanges(_permissions);
    }

    private Task Update(PermissionEntity context)
    {
        _permissions[_permissions.FindIndex(ind => ind.Id == context.Id)] = context;
        return Task.CompletedTask;
    }
}
