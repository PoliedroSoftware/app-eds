using System.Text.Json.Serialization;

namespace APP.Eds.Models.Court
{
    public class TypeOfCollectionCourtModel
    {
        [JsonPropertyName("id_type_of_collection")]
        public int IdTypeOfCollection { get; set; }


        [JsonPropertyName("description")]
        public string Description { get; set; }
    }
}
