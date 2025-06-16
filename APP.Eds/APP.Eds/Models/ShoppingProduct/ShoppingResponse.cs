using System.Text.Json.Serialization;

namespace APP.Eds.Models.ShoppingProduct;


public class ShoppingResponse
{
    [JsonPropertyName("idShopping")]
    public int IdShopping { get; set; }

    [JsonPropertyName("invoice")]
    public string Invoice { get; set; }

    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("idProvider")]
    public int IdProvider { get; set; }

    [JsonPropertyName("idCategory")]
    public int IdCategory { get; set; }

    [JsonPropertyName("amount")]
    public double Amount { get; set; }
}
