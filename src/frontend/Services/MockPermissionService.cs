// Copyright (c) Microsoft. All rights reserved.

using Shared.TableEntities;

namespace ClientApp.Services;

public static class MockPermissionService
{
    private static List<PermissionEntity> _permissions;

    static MockPermissionService()
    {
        _permissions = new List<PermissionEntity>
        {
            new PermissionEntity { Id = "1", Name = "Permission 1" },
            new PermissionEntity { Id = "2", Name = "Permission 2" },
            new PermissionEntity { Id = "3", Name = "Permission 3" },
            new PermissionEntity { Id = "4", Name = "Permission 4" },
            new PermissionEntity { Id = "5", Name = "Permission 5" },
        };
    }

    public static List<PermissionEntity> GetPermissions()
    {
        return _permissions;
    }

    public static Task Delete(PermissionEntity permission)
    {
        _permissions.RemoveAll(p => p.Id == permission.Id);
        return Task.CompletedTask;
    }

    public static Task SaveChanges(IEnumerable<PermissionEntity> permission)
    {
        _permissions = permission.ToList();
        return Task.CompletedTask;
    }
}
