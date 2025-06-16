using System.Text.Json.Serialization;

namespace APP.Eds.Models.Shopping;

public class CategoryModel
{
    [JsonPropertyName("idCategory")]
    public int IdCategory { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }
}
