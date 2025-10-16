namespace APP.Eds.Models.PointOfSale;

public class SaleModel
{
    public int SaleId { get; set; }
    public DateTime Date { get; set; }
    public List<SaleItemModel> Items { get; set; } = new();
    public double SubTotal => Items.Sum(item => item.TotalPrice);
    public double Tax { get; set; }
    public double Discount { get; set; }
    public double Total => SubTotal + Tax - Discount;
    public PaymentMethod PaymentMethod { get; set; }
    public SaleStatus Status { get; set; }
}

public enum PaymentMethod
{
    Cash,
    Card,
    Mixed
}

public enum SaleStatus
{
    Pending,
    Completed,
    Cancelled
}