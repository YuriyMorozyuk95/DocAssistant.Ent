using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace DocAssistant.Data.TableEntities;
public class UserEntity
{
    [JsonProperty("id")] public string Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PasswordHash { get; set; }
    public int AccountId { get; set; }
    public bool IsAdmin { get; set; } = false;
    public IEnumerable<Permission> Permissions { get; set; }
}
