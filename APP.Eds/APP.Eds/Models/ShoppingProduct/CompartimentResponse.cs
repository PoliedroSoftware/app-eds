using System.Text.Json.Serialization;

namespace APP.Eds.Models.ShoppingProduct;


public class CompartimentResponse
{
    [JsonPropertyName("idCompartment")]
    public int IdCompartment { get; set; }

    [JsonPropertyName("number")]
    public int Number { get; set; }

    [JsonPropertyName("nominal")]
    public int Nominal { get; set; }

    [JsonPropertyName("operative")]
    public int Operative { get; set; }

    [JsonPropertyName("stock")]
    public int Stock { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("idTank")]
    public int IdTank { get; set; }
}
