using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace APP.Eds.Models.RegisterShift;

public class EdsRegisterShiftModel
{
    [JsonPropertyName("idEds")]
    public int IdEds { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}
