using Newtonsoft.Json;

namespace APP.Eds.Models.Product;

public class ProductTypeModelResponse
{
    [JsonProperty("idProductType")]
    public int IdProductType { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }
}
