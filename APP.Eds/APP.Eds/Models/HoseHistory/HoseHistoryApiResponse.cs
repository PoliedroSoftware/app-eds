using Newtonsoft.Json;
using System.Collections.Generic;

namespace APP.Eds.Models.HoseHistory;

public class HoseHistoryApiResponse
{
    [JsonProperty("statusCode")]
    public int StatusCode { get; set; }

    [JsonProperty("success")]
    public bool Success { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }

    [JsonProperty("data")]
    public List<HoseHistoryResponse> Data { get; set; }
}
