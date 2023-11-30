using DocAssistant.Data.Interfaces;
using Microsoft.Azure.Cosmos;
using Shared.TableEntities;

namespace DocAssistant.Data;


public class UserRepository : IUserRepository
{
    protected readonly Container Container;
    public UserRepository(Container container)
    {
        Container = container;
    }

    public async Task<UserEntity> GetUserByIdAsync(string userId, string userName)
    {
        ItemResponse<UserEntity> response = await Container.ReadItemAsync<UserEntity>(userId, new PartitionKey(userName));
        return response.Resource;

    }

    public async Task<UserEntity> AddUserAsync(UserEntity user)
    {

        user.Id = Guid.NewGuid().ToString();
        var response = await Container.CreateItemAsync(user, new PartitionKey(user.Username));

        return response.Resource;
    }
}
