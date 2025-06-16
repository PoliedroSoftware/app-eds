using System.Text.Json.Serialization;

namespace APP.Eds.Models.Dispensers;

public class EdsResponseModel
{
    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("data")]
    public List<EdsModel> Data { get; set; }
}
