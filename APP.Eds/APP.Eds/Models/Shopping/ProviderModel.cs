using System.Text.Json.Serialization;

namespace APP.Eds.Models.Shopping;

public class ProviderModel
{
    [JsonPropertyName("idProvider")]
    public int IdProvider { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}
