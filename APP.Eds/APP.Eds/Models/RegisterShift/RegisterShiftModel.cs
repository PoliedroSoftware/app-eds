using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APP.Eds.Models.RegisterShift;

public class RegisterShiftModel
{
    public long? IdEds { get; set; }
    public DateOnly DateStartTime { get; set; }
    public TimeOnly StartTime { get; set; }
    public DateOnly DateEndTime { get; set; }
    public TimeOnly EndTime { get; set; }
}
