using System.Text.Json.Serialization;
using APP.Eds.Models.Eds;
using APP.Eds.Models.Product;

namespace APP.Eds.Models.Court;

public class DispenserCourtModel
{
    [JsonPropertyName("number")]
    public int Number { get; set; }
    [JsonPropertyName("eds")]
    public EdsCourtModel Eds { get; set; }


    [JsonPropertyName("dispensersEntity")]
    public DispenserCourtModel DispensersEntity { get; set; }

    [JsonPropertyName("productTypeEntity")]
    public ProductTypeModelResponse ProductTypeEntity { get; set; }

    [JsonPropertyName("edsEntity")]
    public EdsResponse EdsEntity { get; set; }


    public string ProductName => ProductTypeEntity?.Description ?? "Unknown";
    public int DispensersNumber => DispensersEntity?.Number ?? 0;

    
}
