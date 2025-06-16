using System.Text.Json.Serialization;

namespace APP.Eds.Models.Eds;

public class BusinessModel
{
    [JsonPropertyName("idBusiness")]
    public int IdBusiness { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}
