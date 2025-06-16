using System.Text.Json.Serialization;

namespace APP.Eds.Models.Shopping;

public class ProviderResponseModel
{
    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("data")]
    public List<ProviderModel> Data { get; set; }
}
