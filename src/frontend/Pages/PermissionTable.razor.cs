using System.Security;

using Shared.TableEntities;

namespace ClientApp.Pages;

public sealed partial class PermissionTable
{
    private List<PermissionEntity> _permissions;  
  
    protected override void OnInitialized()  
    {  
        _permissions = MockPermissionService.GetPermissions();  
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

    private Task SaveChanges()
    {
        MockPermissionService.SaveChanges(_permissions);
        return Task.CompletedTask;
    }

    private Task Update(PermissionEntity context)
    {
        _permissions[_permissions.FindIndex(ind => ind.Id == context.Id)] = context;
        return Task.CompletedTask;
    }
}
