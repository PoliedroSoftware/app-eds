using System.Text.Json.Serialization;

namespace APP.Eds.Models.ProductCompartiment;

public class CompartimentModelResponse
{
    [JsonPropertyName("idCompartiment")]
    public int IdCompartiment { get; set; }

    [JsonPropertyName("number")]
    public int Number { get; set; }
}
