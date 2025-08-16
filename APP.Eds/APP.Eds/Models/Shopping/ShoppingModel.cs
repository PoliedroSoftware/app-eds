namespace APP.Eds.Models.Shopping;

public class ShoppingModel
{
    public string Invoice { get; set; }
    public DateTime Date { get; set; }
    public double? Amount { get; set; }
    public int IdProvider { get; set; }
    public int IdCategory { get; set; }
    public List<ShoppingProductNestedModel> ShoppingProducts { get; set; } = new();
    public List<SellPriceProductModel> SellPriceProducts { get; set; } = new();
}

public class ShoppingProductNestedModel
{
    public int IdShopping { get; set; }
    public int IdProduct { get; set; }
    public double? Quantity { get; set; }
    public double? Price { get; set; }
    public string Name { get; set; }
    public double? TotalPrice { get; set; }
    public int IdCompartment { get; set; } 
}

public class SellPriceProductModel
{
    public int IdProduct { get; set; }
    public double? Price { get; set; }
}