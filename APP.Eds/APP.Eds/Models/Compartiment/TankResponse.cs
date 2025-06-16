using System.Text.Json.Serialization;

namespace APP.Eds.Models.Compartiment;


public class TankResponse
{
    [JsonPropertyName("idTank")]
    public int IdTank { get; set; }

    [JsonPropertyName("number")]
    public string Number { get; set; }

    [JsonPropertyName("compartment")]
    public int Compartment { get; set; }

    [JsonPropertyName("ability")]
    public int Ability { get; set; }

    [JsonPropertyName("stock")]
    public int Stock { get; set; }
}
