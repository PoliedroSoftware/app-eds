namespace APP.Eds.Models.ShoppingProduct
{
    public class ShoppingProductModel
    {
        public int IdShopping { get; set; }
        public int IdProduct { get; set; }
        public double Quantity { get; set; }
        public double Price { get; set; }
        public string Name { get; set; }
        public double TotalPrice { get; set; }
        public int IdCompartment { get; set; }
    }
}
