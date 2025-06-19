using APP.Eds.Models.Eds;
using APP.Eds.Models.Product;
using System.Text.Json.Serialization;


namespace APP.Eds.Models.Court;

public class HoseCourtModel 
{
    [JsonPropertyName("idHose")]
    public int IdHose { get; set; }

    [JsonPropertyName("idDispensers")]
    public int IdDispensers { get; set; }

    [JsonPropertyName("number")]
    public int Number { get; set; }

    [JsonPropertyName("accumulatedAmount")]
    public double AccumulatedAmount { get; set; }

    [JsonPropertyName("accumulatedGallons")]
    public double AccumulatedGallons { get; set; }

    [JsonPropertyName("idProductType")]
    public int IdProductType { get; set; }

    [JsonPropertyName("price")]
    public double Price { get; set; }

    [JsonPropertyName("dispensersEntity")]
    public DispenserCourtModel DispensersEntity { get; set; }

    [JsonPropertyName("productTypeEntity")]
    public ProductTypeModelResponse ProductTypeEntity { get; set; }

    [JsonPropertyName("edsEntity")]
    public EdsResponse EdsEntity { get; set; }

   

    public string ProductName => ProductTypeEntity?.Description ?? "Unknown";
    public int DispensersNumber => DispensersEntity?.Number ?? 0;
    public string EdsName => EdsEntity?.Name ?? "Unknown";
    public int EdsId => DispensersEntity?.Eds?.IdEds ?? 0;

    public string DisplayText =>
     $"Dispenser: {DispensersNumber}\nHose: {Number}\nProduct: {ProductName}\n-------------------------------------------------";


}