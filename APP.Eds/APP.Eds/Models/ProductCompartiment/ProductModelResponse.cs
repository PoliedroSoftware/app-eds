using Newtonsoft.Json;

namespace APP.Eds.Models.ProductCompartiment;

public class ProductModelResponse
{
    [JsonProperty("idProduct")]
    public int IdProduct { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }
}
