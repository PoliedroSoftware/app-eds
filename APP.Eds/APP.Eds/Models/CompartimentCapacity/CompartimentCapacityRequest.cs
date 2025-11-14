using System.Text.Json.Serialization;

namespace APP.Eds.Models.CompartimentCapacity;

public class CompartimentCapacityRequest
{
    // El backend exige esta propiedad raíz: "Request"
    [JsonPropertyName("Request")]
    public CompartimentCapacityModel Request { get; set; } = default!;
}

