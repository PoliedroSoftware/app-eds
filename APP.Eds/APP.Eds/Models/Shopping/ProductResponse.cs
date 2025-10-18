using System.Text.Json.Serialization;

namespace APP.Eds.Models.Shopping;


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

    [JsonPropertyName("sellPrice")]
    public double SellPrice { get; set; }

    [JsonPropertyName("purchasePrice")]
    public double PurchasePrice { get; set; }

    [JsonPropertyName("stock")]
    public double Stock { get; set; }
    // Propiedad para mostrar el nombre del tipo de combustible
    public string ProductTypeName { get; set; }
}
