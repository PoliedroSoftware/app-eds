using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace APP.Eds.Models.Dispenser;
public class DispenserModelResponse
{
    [JsonPropertyName("id")]
    public int IdDispensers { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; }

    [JsonPropertyName("number")]
    public int Number { get; set; }

    [JsonPropertyName("dispenserTypeId")]
    public int IdDispenserType { get; set; }

    [JsonPropertyName("edsId")]
    public int IdEds { get; set; }

    [JsonPropertyName("idIsland")]
    public int IdIsland { get; set; }

    [JsonPropertyName("hoseNumber")]
    public int NumberHose { get; set; }

    public string DisplayName => $" Number:{Number} - Code:{Code} ";
}
