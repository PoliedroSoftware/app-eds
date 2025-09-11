using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APP.Eds.Models.StrongBox
{
    public class StrongBoxModel
    {
        public long? IdCorte { get; set; }

        public string Type { get; set; }

        public double Ammount { get; set; }

        public string Note { get; set; }
    }
}
