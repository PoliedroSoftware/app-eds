using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APP.Eds.Models.RegisterShift;

public class RegisterShiftUserModel
{
    public long? IdEds { get; set; }
    public long? IdBusiness { get; set; }
    public long? IdIslander { get; set; } 
    public DateOnly DateStartTime { get; set; }
    public TimeOnly StartTime { get; set; }
    public DateOnly DateEndTime { get; set; }
    public TimeOnly EndTime { get; set; }

}
