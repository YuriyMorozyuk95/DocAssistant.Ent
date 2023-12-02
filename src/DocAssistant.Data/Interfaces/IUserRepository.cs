// Copyright (c) Microsoft. All rights reserved.

using Shared.TableEntities;

namespace DocAssistant.Data.Interfaces;
public interface IUserRepository
{
    Task<UserEntity> GetUserByIdAsync(string userId);

    Task<UserEntity> AddUserAsync(UserEntity user);

    Task<IEnumerable<UserEntity>> GetAllUsersAsync();

    Task UpdateUserAsync(UserEntity user);

    Task DeleteUserAsync(string id);

}
