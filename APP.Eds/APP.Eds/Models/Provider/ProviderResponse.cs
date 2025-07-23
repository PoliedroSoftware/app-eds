using System.Text.Json.Serialization;

namespace APP.Eds.Models.Provider;
public class ProviderResponse
{
    [JsonPropertyName("idProvider")]
    public int IdProvider { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}