namespace APP.Eds.Models.Compartiment
{
    public class CompartimentModel
    {
        public int Number { get; set; }
        public double Nominal { get; set; }
        public double Operative { get; set; }
        public double Height { get; set; }
        public int IdTank { get; set; }
        public int IdProduct { get; set; }
        public string? NumberTank { get; set; }
        public string? NameProduct { get; set; }
    }
}
