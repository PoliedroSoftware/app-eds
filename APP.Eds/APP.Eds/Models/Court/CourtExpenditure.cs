namespace APP.Eds.Models.Court
{
    public class CourtExpenditure
    {
        public int IdExpenditure { get; set; }
        public string ExpenditureName { get; set; }
        public double Amount { get; set; }
        public string? Description { get; set; }
    }
}
