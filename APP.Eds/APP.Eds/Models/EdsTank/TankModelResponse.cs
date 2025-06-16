using Newtonsoft.Json;

namespace APP.Eds.Models.EdsTank;

public class TankModelResponse
{
    [JsonProperty("idTank")]
    public int IdTank { get; set; }

    [JsonProperty("number")]
    public string Number { get; set; }
}
