using Newtonsoft.Json;

namespace APP.Eds.Models.Capacity;

public class CapacityResponse
{
    [JsonProperty("statusCode")]
    public int StatusCode { get; set; }

    [JsonProperty("success")]
    public bool Success { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }

    [JsonProperty("data")]
    public List<CapacityModelResponse> Data { get; set; }
}
