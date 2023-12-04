// Copyright (c) Microsoft. All rights reserved.

using Shared.TableEntities;

namespace DocAssistant.Data.Interfaces;
public interface IPermissionRepository
{
    Task<PermissionEntity> GetPermissionByIdAsync(string id);

    Task<IEnumerable<PermissionEntity>> GetAllPermissionsAsync();

    Task DeletePermissionAsync(string id);

    Task SavePermissionAsync(IEnumerable<PermissionEntity> permissions);
}
