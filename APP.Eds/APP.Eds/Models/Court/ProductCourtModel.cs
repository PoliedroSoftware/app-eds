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

        [JsonPropertyName("price")]
        public double Price { get; set; }
    }
}
