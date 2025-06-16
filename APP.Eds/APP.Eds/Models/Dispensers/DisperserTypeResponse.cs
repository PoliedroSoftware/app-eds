using System.Text.Json.Serialization;

namespace APP.Eds.Models.Dispensers;

public class DisperserTypeResponse
{
    [JsonPropertyName("idType")]
    public int IdType { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }
}
