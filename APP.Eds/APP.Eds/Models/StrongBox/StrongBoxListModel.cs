using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APP.Eds.Models.StrongBox
{
    public class StrongBoxListModel
    {
        public long Id { get; set; }

        public long? IdCorte { get; set; }

        public int? IdEds { get; set; }

        public string Type { get; set; }

        public double Ammount { get; set; }

        public double Saldo { get; set; }

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? CreatedBy { get; set; }
    }
}
