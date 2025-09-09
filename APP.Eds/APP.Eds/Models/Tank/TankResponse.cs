using Newtonsoft.Json;

namespace APP.Eds.Models.Tank;

public class TankResponse
{
    [JsonProperty("idTank")]
    public int IdTank { get; set; }

    [JsonProperty("compartment")]
    public int Compartment { get; set; }

    [JsonProperty("number")]
    public string Number { get; set; }

    [JsonProperty("ability")]
    public double Ability { get; set; }

    [JsonProperty("stock")]
    public double Stock { get; set; }
}
