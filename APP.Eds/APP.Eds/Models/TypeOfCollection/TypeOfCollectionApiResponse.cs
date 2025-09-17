using System.Text.Json.Serialization;

namespace APP.Eds.Models.TypeOfCollection
{
    public class TypeOfCollectionApiResponse
    {
        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("data")]
        public List<TypeOfCollectionItem> Data { get; set; } = new();
    }

    public class TypeOfCollectionItem
    {
        [JsonPropertyName("idTypeOfCollection")]
        public int IdTypeOfCollection { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }
}