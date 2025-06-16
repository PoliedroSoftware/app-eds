using System.Text.Json.Serialization;

namespace APP.Eds.Models.ShoppingProduct;

public class ProductApiResponse
{
    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("data")]
    public List<ProductResponse> Data { get; set; }
}
