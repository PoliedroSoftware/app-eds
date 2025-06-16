using System.Text.Json.Serialization;

namespace APP.Eds.Models.Court
{
    public class EdsCourtResponseModel
    {
        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("data")]
        public List<EdsCourtModel> Data { get; set; }
    }
}
