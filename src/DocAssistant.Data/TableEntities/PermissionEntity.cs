using Newtonsoft.Json;

namespace DocAssistant.Data.TableEntities;
public class PermissionEntity 
{
    [JsonProperty("id")] public string Id { get; set; }

    public string Name { get; set; }

    public Rights Right { get; set; }
}
