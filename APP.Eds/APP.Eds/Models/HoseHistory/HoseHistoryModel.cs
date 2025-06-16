namespace APP.Eds.Models.HoseHistory
{
    public class HoseHistoryModel
    {
        public int IdHose { get; set; }
        public int IdDispensers { get; set; }
        public DateTime Date { get; set; }
        public double AccumulatedAmount { get; set; }
        public double AccumulatedGallons { get; set; }
    }
}
