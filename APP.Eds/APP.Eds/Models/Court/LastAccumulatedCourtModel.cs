using System.Text.Json.Serialization;

namespace APP.Eds.Models.Court;

public class LastAccumulatedCourtModel
{
    [JsonPropertyName("lastAccumulatedAmount")]
    public double LastAccumulatedAmount { get; set; }

    [JsonPropertyName("lastAccumulatedGallons")]
    public double LastAccumulatedGallons { get; set; }
}
