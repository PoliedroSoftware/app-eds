using System.Text.Json.Serialization;

namespace APP.Eds.Models.ShoppingProduct;


public class ProductResponse
{
    [JsonPropertyName("idProduct")]
    public int IdProduct { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("idProductType")]
    public int IdProductType { get; set; }

    [JsonPropertyName("price")]
    public double Price { get; set; }
}
