using Newtonsoft.Json;

namespace APP.Eds.Models.CompartimentCapacity;

public class CapacityModelResponse
{
    [JsonProperty("idcapacity")]
    public int IdCapacity { get; set; }

    [JsonProperty("code")]
    public string Code { get; set; }
}
