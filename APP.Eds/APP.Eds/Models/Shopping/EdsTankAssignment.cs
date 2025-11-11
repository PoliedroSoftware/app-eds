using System.Text.Json.Serialization;

namespace APP.Eds.Models.Shopping;

public class EdsTankAssignment
{
    [JsonPropertyName("idEds")]
    public int IdEds { get; set; }

    [JsonPropertyName("idTank")]
    public int IdTank { get; set; }

    [JsonPropertyName("edsName")]
    public string EdsName { get; set; }

    [JsonPropertyName("tankNumber")]
    public int TankNumber { get; set; }
}
