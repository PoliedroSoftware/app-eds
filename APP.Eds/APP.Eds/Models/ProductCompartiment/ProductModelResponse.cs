using Newtonsoft.Json;

namespace APP.Eds.Models.Product;

public class ProductModelResponse
{
    [JsonProperty("idProduct")]
    public int IdProduct { get; set; }

    [JsonProperty("name")]
    public String Name { get; set; }
}
