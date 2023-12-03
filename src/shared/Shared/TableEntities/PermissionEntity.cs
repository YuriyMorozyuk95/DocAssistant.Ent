using Newtonsoft.Json;

namespace Shared.TableEntities;

public class PermissionEntity
{
    [JsonProperty("id")] public string Id { get; set; }

    public string PartitionKey { get; set; }

    public string Name { get; set; }

    override public string ToString() => Name;
}
