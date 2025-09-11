using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace APP.Eds.Models.StrongBox
{
    public class StrongBoxGetLastBalanceModel
    {
        public double Saldo { get; set; }

        public long? Id { get; set; }

        public DateTime DateTime { get; set; }
    }
}
