using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace APP.Eds.Models.Inventory
{
    /// <summary>
    /// Wrapper class for the API response structure
    /// </summary>
    public class ApiResponseWrapper<T>
    {
        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("data")]
        public T? Data { get; set; }
    }
}
