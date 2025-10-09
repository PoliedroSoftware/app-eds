namespace APP.Eds.Models.Hose
{
    public class HoseModel
    {
        public int IdHose {  get; set; }
        public int IdDispensers { get; set; }
        public int Number { get; set; }
        public double AccumulatedAmount { get; set; }
        public double AccumulatedGallons { get; set; }
        public int IdProductType { get; set; } 
        public int IdCompartiment { get; set; }
        public double Price { get; set; }
    }
}
