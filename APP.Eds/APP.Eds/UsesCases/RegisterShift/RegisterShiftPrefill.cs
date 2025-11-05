namespace APP.Eds.UsesCases.RegisterShift;

public class RegisterShiftPrefill
{
    public int? IdBusiness { get; set; }
    public int? IdEds { get; set; }
    public int? IdIslander { get; set; }
    public DateTime DateStart { get; set; }
    public TimeSpan StartTime { get; set; }
    public DateTime DateEnd { get; set; }
    public TimeSpan EndTime { get; set; }
}
