using System.Text.Json.Serialization;

namespace APP.Eds.Models.Shopping;

public class EdsTankApiResponse
{
    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("data")]
    public List<EdsTankData> Data { get; set; }
}

public class EdsTankData
{
    [JsonPropertyName("idEdsTank")]
    public int IdEdsTank { get; set; }

    [JsonPropertyName("idEds")]
    public int IdEds { get; set; }

    [JsonPropertyName("idTank")]
    public int IdTank { get; set; }
}
