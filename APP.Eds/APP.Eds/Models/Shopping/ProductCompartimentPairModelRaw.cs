namespace APP.Eds.Models.Shopping;

public class ProductCompartimentPairModelRaw
{
    public int idCompartiment { get; set; }
    public int number { get; set; }
    public double nominal { get; set; }
    public double operative { get; set; }
    public int idProduct { get; set; }
    public double height { get; set; }
    public int idTank { get; set; }
    
    // Optional fields that might not be in the response
    public double stock { get; set; }
    public string productName { get; set; } = string.Empty;
    public string idEds { get; set; } = string.Empty;
    public string idBusiness { get; set; } = string.Empty;
    public string date { get; set; } = string.Empty;
}