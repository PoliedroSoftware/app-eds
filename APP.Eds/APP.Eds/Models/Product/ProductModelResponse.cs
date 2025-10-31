using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace APP.Eds.Models.Product;

public class ProductModelResponse
{
    [JsonPropertyName("idProduct")]
    public int IdProduct { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("idProductType")]
    public int IdProductType { get; set; }

    [JsonPropertyName("sellPrice")]
    public decimal? SellPrice { get; set; }

    [JsonPropertyName("purchasePrice")]
    public decimal? PurchasePrice { get; set; }

    [JsonPropertyName("stock")]
    public decimal? Stock { get; set; }
}