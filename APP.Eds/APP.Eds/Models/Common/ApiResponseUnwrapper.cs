using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace APP.Eds.Models.Common
{
    public static class ApiResponseUnwrapper
    {
        public static List<T> UnwrapDataList<T>(string json)
        {
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            
            try
            {
                var list = JsonSerializer.Deserialize<List<ApiEnvelope<T>>>(json, opts);
                if (list is { Count: > 0 } && list[0]?.Data is { } d1) return d1;
            }
            catch { }

            
            try
            {
                var env = JsonSerializer.Deserialize<ApiEnvelope<T>>(json, opts);
                if (env?.Data is { } d2) return d2;
            }
            catch { }

            
            try
            {
                var plain = JsonSerializer.Deserialize<List<T>>(json, opts);
                if (plain is { } d3) return d3;
            }
            catch { }

            return new List<T>();
        }
    }
}
