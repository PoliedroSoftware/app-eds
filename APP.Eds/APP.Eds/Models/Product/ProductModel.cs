namespace APP.Eds.Models.Product;

public class ProductModel
{
    public int IdProduct { get; set; }
    public string Name { get; set; }
    public int IdProductType { get; set; }
    public double SellPrice { get; set; }
    public double PurchasePrice { get; set; }
    public int Stock { get; set; }
}
