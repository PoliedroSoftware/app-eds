using Newtonsoft.Json;

namespace APP.Eds.Models.HoseHistory;

public class HoseHistoryResponse
{
    [JsonProperty("idHoseHistory")]
    public int idHoseHistory { get; set; }

    [JsonProperty("idHose")]
    public int IdHose { get; set; }

    [JsonProperty("idDispensers")]
    public int IdDispensers { get; set; }

    [JsonProperty("Date")]
    public DateTime Date { get; set; }

    [JsonProperty("accumulatedAmount")]
    public decimal AccumulatedAmount { get; set; }

    [JsonProperty("accumulatedGallons")]
    public decimal AccumulatedGallons { get; set; }
}
