using System.Text.Json.Serialization;

namespace APP.Eds.Models.Inventory;

public class InventoryModel
{
    [JsonPropertyName("businesses")]
    public List<Business> Businesses { get; set; }
}
