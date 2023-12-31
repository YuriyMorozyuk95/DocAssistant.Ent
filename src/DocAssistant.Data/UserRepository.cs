﻿using DocAssistant.Data.Interfaces;
using Microsoft.Azure.Cosmos;
using Shared.TableEntities;

namespace DocAssistant.Data;


public class UserRepository : IUserRepository
{
    private readonly Container _container;

    public UserRepository(Container container)
    {
        _container = container;
    }

    public async Task<UserEntity> AddUserAsync(UserEntity user)
    {

        user.Id = Guid.NewGuid().ToString();
        user.PartitionKey = user.Email;
        var response = await _container.CreateItemAsync(user, new PartitionKey(user.Email));

        return response.Resource;
    }

    public async Task<UserEntity> GetUserByIdAsync(string userId, string email)
    {
        var response = await _container.ReadItemAsync<UserEntity>(userId, new PartitionKey(email));
        return response.Resource;
    }

    public async Task<IEnumerable<UserEntity>> GetAllUsersAsync()
    {
        var query = new QueryDefinition("SELECT * FROM c");
        var resultSetIterator = _container.GetItemQueryIterator<UserEntity>(query);
        List<UserEntity> results = new List<UserEntity>();
        while (resultSetIterator.HasMoreResults)
        {
            var response = await resultSetIterator.ReadNextAsync();
            results.AddRange(response.ToList());
        }
        return results;
    }

    public async Task UpdateUserAsync(UserEntity user)
    {
        await _container.UpsertItemAsync(user, new PartitionKey(user.Email));
    }

    public async Task DeleteUserAsync(string id, string email)
    {
        await _container.DeleteItemAsync<UserEntity>(id, new PartitionKey(email));
    }
}
