using Newtonsoft.Json;

namespace APP.Eds.Models.ProductCompartiment;

public class CompartimentModelResponse
{
    [JsonProperty("idCompartiment")]
    public int IdCompartiment { get; set; }

    [JsonProperty("number")]
    public int Number { get; set; }
}
