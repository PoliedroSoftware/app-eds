using System.Text.Json.Serialization;

namespace APP.Eds.Models.Court
{
    public class EdsCourtModel
    {
        [JsonPropertyName("idEds")]
        public int IdEds { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("nit")]
        public string Nit { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("sicom")]
        public string Sicom { get; set; }

        [JsonPropertyName("idBusiness")]
        public int IdBusiness { get; set; }
    }
}
