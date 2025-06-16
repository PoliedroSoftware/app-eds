using System.Text.Json.Serialization;

namespace APP.Eds.Models.Court
{
    public class BusinessCourtModel
    {
        [JsonPropertyName("id_business")]
        public int IdBusiness { get; set; }


        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
