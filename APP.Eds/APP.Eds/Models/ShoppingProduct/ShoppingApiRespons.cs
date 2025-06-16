using System.Text.Json.Serialization;

namespace APP.Eds.Models.ShoppingProduct;

public class ShoppingApiResponse
{
    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("data")]
    public List<ShoppingResponse> Data { get; set; }
}
