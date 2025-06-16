using Newtonsoft.Json;

namespace APP.Eds.Models.Dispensers;

public class IslanderResponse
{
    [JsonProperty("idIslander")]
    public int IdIslander { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("idEds")]
    public int IdEds { get; set; }
}
