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
            new PermissionEntity { Id = "1", Name = "Development" },
            new PermissionEntity { Id = "2", Name = "Management" },
            new PermissionEntity { Id = "3", Name = "Business Analyst" },
            new PermissionEntity { Id = "4", Name = "Chief Executive Officer" },
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

    public static PermissionEntity GetPermissionById(string id)
    {
        return _permissions.FirstOrDefault(p => p.Id == id);
    }
}
