// Copyright (c) Microsoft. All rights reserved.

using Shared.TableEntities;

namespace DocAssistant.Data.Interfaces;
public interface IPermissionRepository
{
    Task<PermissionEntity> GetPermissionByIdAsync(string id, string name);

    Task<PermissionEntity> AddPermissionAsync(PermissionEntity permission);
}
