namespace APP.Eds.Models.Shopping;

public class ShoppingModel
{
    public string Invoice { get; set; }
    public DateTime Date { get; set; }
    public double Amount { get; set; }
    public int IdProvider { get; set; }
    public int IdCategory { get; set; }

}