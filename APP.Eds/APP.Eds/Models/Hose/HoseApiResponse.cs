using Newtonsoft.Json;

namespace APP.Eds.Models.Hose;

public class HoseApiResponse
{
    [JsonProperty("statusCode")]
    public int StatusCode { get; set; }

    [JsonProperty("success")]
    public bool Success { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }

    [JsonProperty("data")]
    public List<HoseResponse> Data { get; set; }
}
