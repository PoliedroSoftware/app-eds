namespace APP.Eds.Models.Shopping;

public class ProductCompartimentPairModel
{
    public int IdProduct { get; set; }
    public string ProductName { get; set; }
    public int IdCompartment { get; set; }
    public int Number { get; set; }
    public double Operative { get; set; }
    public double Stock { get; set; }

    public string Display => $"{ProductName} - Compartimento {Number}\nCapacidad: {Operative:N0} | Stock: {Stock:N0}";
}