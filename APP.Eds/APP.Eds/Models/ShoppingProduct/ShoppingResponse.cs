using System.Text.Json.Serialization;

namespace APP.Eds.Models.ShoppingProduct;


public class ShoppingResponse
{
    [JsonPropertyName("idShopping")]
    public int IdShopping { get; set; }

    [JsonPropertyName("invoice")]
    public string Invoice { get; set; }

    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("idProvider")]
    public int IdProvider { get; set; }

    [JsonPropertyName("idCategory")]
    public int IdCategory { get; set; }

    [JsonPropertyName("amount")]
    public double Amount { get; set; }

    [JsonPropertyName("provider")]
    public ProviderDetail Provider { get; set; }

    [JsonPropertyName("category")]
    public CategoryDetail Category { get; set; }

    [JsonPropertyName("shoppingProducts")]
    public List<ShoppingProductDetail> ShoppingProducts { get; set; } = new();
}

public class ProviderDetail
{
    [JsonPropertyName("idProvider")]
    public int IdProvider { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class CategoryDetail
{
    [JsonPropertyName("idCategory")]
    public int IdCategory { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }
}

public class ShoppingProductDetail
{
    [JsonPropertyName("idShoppingProduct")]
    public int IdShoppingProduct { get; set; }

    [JsonPropertyName("idShopping")]
    public int IdShopping { get; set; }

    [JsonPropertyName("idProduct")]
    public int IdProduct { get; set; }

    [JsonPropertyName("quantity")]
    public double Quantity { get; set; }

    [JsonPropertyName("purchasePrice")]
    public double PurchasePrice { get; set; }

    [JsonPropertyName("sellPrice")]
    public double SellPrice { get; set; }

    [JsonPropertyName("idCompartment")]
    public int IdCompartment { get; set; }

    [JsonPropertyName("product")]
    public ProductDetail Product { get; set; }

    // Propiedades calculadas
    public double TotalPrice => Quantity * PurchasePrice;
    public string ProductName => Product?.Name ?? "Producto";
}

public class ProductDetail
{
    [JsonPropertyName("idProduct")]
    public int IdProduct { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("idProductType")]
    public int IdProductType { get; set; }

    [JsonPropertyName("sellPrice")]
    public double SellPrice { get; set; }

    [JsonPropertyName("purchasePrice")]
    public double PurchasePrice { get; set; }

    [JsonPropertyName("stock")]
    public double Stock { get; set; }
}
