using Newtonsoft.Json;

namespace APP.Eds.Models.Eds;

public class EdsApiResponse
{
    [JsonProperty("statusCode")]
    public int StatusCode { get; set; }

    [JsonProperty("success")]
    public bool Success { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }

    [JsonProperty("data")]
    public List<EdsResponse> Data { get; set; }
}
