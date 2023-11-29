// Copyright (c) Microsoft. All rights reserved.

using DocAssistant.Data.TableEntities;

namespace DocAssistant.Data.Interfaces;
public interface IPermissionRepository
{
    Task<PermissionEntity> GetPermissionByIdAsync(string id, string name);

    Task<PermissionEntity> AddPermissionAsync(PermissionEntity permission);
}
