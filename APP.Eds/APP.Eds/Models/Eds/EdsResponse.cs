using Newtonsoft.Json;

namespace APP.Eds.Models.Eds;

public class EdsResponse
{
    [JsonProperty("idEds")]
    public int IdEds { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("nit")]
    public string Nit { get; set; }

    [JsonProperty("address")]
    public string Address { get; set; }

    [JsonProperty("sicom")]
    public string Sicom { get; set; }

    [JsonProperty("idBusiness")]
    public int IdBusiness { get; set; }
}
