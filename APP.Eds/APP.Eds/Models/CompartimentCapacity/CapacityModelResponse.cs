using Newtonsoft.Json;

namespace APP.Eds.Models.Capacity;

public class CapacityModelResponse
{
    [JsonProperty("idcapacity")]
    public int IdCapacity { get; set; }

    [JsonProperty("code")]
    public String Code { get; set; }
}
