using System.Text.Json.Serialization;

namespace APP.Eds.Models.Court
{
    public class ProductCourtModel
    {
        [JsonPropertyName("id_product")]
        public int IdProduct { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("id_produc_type")]
        public int IdProductType { get; set; }

        [JsonPropertyName("purchasePrice")]
        public double PurchasePrice { get; set; }

        [JsonPropertyName("sellPrice")]
        public double SellPrice { get; set; }

        [JsonPropertyName("stock")]
        public int Stock { get; set; }


    }
}
