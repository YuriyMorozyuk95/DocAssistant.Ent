using Shared.TableEntities;

namespace ClientApp.Pages;

public sealed partial class PermissionTable
{
    private List<PermissionEntity> _permissions;

    [Inject]
    public PermissionApiClient PermissionApiClient { get; set; }

    protected override async Task OnInitializedAsync()  
    {  
        _permissions = (List<PermissionEntity>)await PermissionApiClient.GetAllPermissions();  
    }  
  
    private Task Create()  
    {
        var lastId = _permissions?.Last()?.Id ?? 1.ToString();
        lastId = lastId != null ? (int.Parse(lastId) + 1).ToString() : 1.ToString();

        var newPermission = new PermissionEntity()
        {   
            Id = lastId,
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
