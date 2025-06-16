using System.Text.Json.Serialization;

namespace APP.Eds.Models.ShoppingProduct;


public class ShoppingProductResponse
{
    [JsonPropertyName("idShoppingProduct")]
    public int IdShoppingProduct { get; set; }

    [JsonPropertyName("idShopping")]
    public int IdShopping { get; set; }

    [JsonPropertyName("idProduct")]
    public int IdProduct { get; set; }

    [JsonPropertyName("quantity")]
    public double Quantity { get; set; }

    [JsonPropertyName("price")]
    public double Price { get; set; }

    [JsonPropertyName("idCompartment")]
    public int IdCompartment { get; set; }
}
