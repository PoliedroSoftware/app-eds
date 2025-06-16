using APP.Eds.Models.Compartiment;
using Newtonsoft.Json;

namespace APP.Eds.Models.ProductCompartiment;

public class compartimentResponse
{
    [JsonProperty("statusCode")]
    public int StatusCode { get; set; }

    [JsonProperty("success")]
    public bool Success { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }

    [JsonProperty("data")]
    public List<CompartimentModelResponse> Data { get; set; }
}
