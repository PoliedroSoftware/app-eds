using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace APP.Eds.Models.RegisterShift;

public class RegisterShiftDataModel
{
    [JsonPropertyName("idEds")]
    public int IdEds { get; set; }

    [JsonPropertyName("idBusiness")]
    public int IdBusiness { get; set; }

    [JsonPropertyName("idIslander")]
    public int IdIslander { get; set; }

}
