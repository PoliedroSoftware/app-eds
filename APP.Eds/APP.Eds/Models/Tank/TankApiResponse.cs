using Newtonsoft.Json;

namespace APP.Eds.Models.Tank;

public class TankApiResponse
{
    [JsonProperty("statusCode")]
    public int StatusCode { get; set; }

    [JsonProperty("success")]
    public bool Success { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }

    [JsonProperty("data")]
    public List<TankResponse> Data { get; set; }
}
