using System.Text.Json.Serialization;

namespace APP.Eds.Models.Compartiment;


public class CompartimentResponse
{
    [JsonPropertyName("idCompartiment")]
    public int IdCompartment { get; set; }

    [JsonPropertyName("number")]
    public int Number { get; set; }

    [JsonPropertyName("nominal")]
    public double Nominal { get; set; }

    [JsonPropertyName("operative")]
    public double Operative { get; set; }

    [JsonPropertyName("height")]
    public double Height { get; set; }

    [JsonPropertyName("idTank")]
    public int IdTank { get; set; }

    [JsonPropertyName("idProduct")]
    public int IdProduct { get; set; }

    public string DisplayCompartiment =>
        $"Compartimento: {Number}\nCapacidad nominal: {Nominal}\nCapacidad operativa: {Operative}\nAltura: {Height}\nID del producto: {IdProduct}\n-------------------------------------------------";
}
