using Newtonsoft.Json;

namespace APP.Eds.Models.Hose;

public class HoseResponse
{
    [JsonProperty("idHose")]
    public int IdHose { get; set; }

    [JsonProperty("idDispensers")]
    public int IdDispensers { get; set; }

    [JsonProperty("number")]
    public int Number { get; set; }

    [JsonProperty("accumulatedAmount")]
    public decimal AccumulatedAmount { get; set; }

    [JsonProperty("accumulatedGallons")]
    public decimal AccumulatedGallons { get; set; }

    [JsonProperty("idProductType")]
    public int IdProductType { get; set; }
}
