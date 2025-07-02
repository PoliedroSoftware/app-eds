using System.Text.Json.Serialization;

namespace APP.Eds.Models.ShoppingProduct;


public class CompartimentResponse
{
    [JsonPropertyName("idCompartment")]
    public int IdCompartment { get; set; }

    [JsonPropertyName("number")]
    public int Number { get; set; }

    [JsonPropertyName("nominal")]
    public double Nominal { get; set; }

    [JsonPropertyName("operative")]
    public double Operative { get; set; }

    [JsonPropertyName("stock")]
    public double Stock { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("idTank")]
    public int IdTank { get; set; }

    public string DisplayCompartiment =>
        $"Compartiment: {Number}\nNominal: {Nominal}\nOperative: {Operative}\nStock: {Stock}\n_________________________________________";
}
