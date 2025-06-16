using Newtonsoft.Json;

namespace APP.Eds.Models.Category;

public class CategoryResponse
{
    [JsonProperty("idCategory")]
    public int IdCategory { get; set; }

    [JsonProperty("idDescription")]
    public string Description { get; set; }
}