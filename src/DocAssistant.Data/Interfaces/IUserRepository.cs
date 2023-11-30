// Copyright (c) Microsoft. All rights reserved.

using Shared.TableEntities;

namespace DocAssistant.Data.Interfaces;
public interface IUserRepository
{
    Task<UserEntity> GetUserByIdAsync(string userId, string userName);

    Task<UserEntity> AddUserAsync(UserEntity user);
}
