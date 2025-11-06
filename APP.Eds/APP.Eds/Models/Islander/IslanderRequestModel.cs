using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APP.Eds.Models.Islander;

public class IslanderRequestModel
{
    [JsonProperty("idIslander")]
    public int IdIslander { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("idEds")]
    public int IdEds { get; set; }

    [JsonProperty("password")]
    public string Password { get; set; }

    [JsonProperty("email")]
    public string Email { get; set; }
}
