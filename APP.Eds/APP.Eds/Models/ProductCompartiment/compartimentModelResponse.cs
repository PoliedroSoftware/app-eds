using Newtonsoft.Json;

namespace APP.Eds.Models.Compartiment;

public class CompartimentModelResponse
{
    [JsonProperty("idCompartiment")]
    public int IdCompartiment { get; set; }

    [JsonProperty("number")]
    public int Number { get; set; }

    [JsonProperty("nominal")]
    public double Nominal { get; set; }

    [JsonProperty("operative")]
    public double Operative { get; set; }

    [JsonProperty("stock")]
    public double Stock { get; set; }

    [JsonProperty("height")]
    public double Height { get; set; }

    [JsonProperty("idTank")]
    public int IdTank { get; set; }

    public string DisplayCompartiment =>
        $"Compartiment: {Number}\nNominal: {Nominal}\nOperative: {Operative}\nStock: {Stock}\nHeight: {Height}\n-------------------------------------------------";
}
