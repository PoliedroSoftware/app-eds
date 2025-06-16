using Newtonsoft.Json;

namespace APP.Eds.Models.Product;

public class ProductResponse
{
    [JsonProperty("statusCode")]
    public int StatusCode { get; set; }

    [JsonProperty("success")]
    public bool Success { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }

    [JsonProperty("data")]
    public List<ProductModelResponse> Data { get; set; }
}
