using System.Text.Json.Serialization;

namespace APP.Eds.Models.CompartimentCapacity;

public class CapacityModelResponse
{
    [JsonPropertyName("idcapacity")]
    public int IdCapacity { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; }
}
