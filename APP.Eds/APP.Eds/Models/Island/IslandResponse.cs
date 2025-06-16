using Newtonsoft.Json;

namespace APP.Eds.Models.Island;

public class IslandResponse
{
    [JsonProperty("idisland")]
    public int Idisland { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }
}