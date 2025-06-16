using System.Text.Json.Serialization;

namespace APP.Eds.Models.Court;

public class HoseCourtResponseModel
{
    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("data")]
    public List<HoseCourtModel> Data { get; set; }
}
