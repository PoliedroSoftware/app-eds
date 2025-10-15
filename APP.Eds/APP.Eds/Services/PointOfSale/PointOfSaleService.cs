using APP.Eds.Models.PointOfSale;
using APP.Eds.Models.Product;
using System.Collections.ObjectModel;

namespace APP.Eds.Services.PointOfSale;

public interface IPointOfSaleService
{
    Task<List<ProductModel>> GetAvailableProductsAsync();
    Task<bool> ProcessSaleAsync(SaleModel sale);
    Task<List<SaleModel>> GetSalesHistoryAsync();
}

public class PointOfSaleService : IPointOfSaleService
{
    // Simulación de productos para el ejemplo
    private readonly List<ProductModel> _products = new()
    {
        new ProductModel { Name = "Coca Cola 500ml", SellPrice = 2.50, Stock = 50 },
        new ProductModel { Name = "Agua 1L", SellPrice = 1.00, Stock = 100 },
        new ProductModel { Name = "Chips", SellPrice = 1.50, Stock = 30 },
        new ProductModel { Name = "Chocolate", SellPrice = 3.00, Stock = 25 },
        new ProductModel { Name = "Sandwich", SellPrice = 5.00, Stock = 15 },
        new ProductModel { Name = "Café", SellPrice = 2.00, Stock = 40 },
        new ProductModel { Name = "Jugo Naranja", SellPrice = 2.25, Stock = 35 },
        new ProductModel { Name = "Galletas", SellPrice = 1.75, Stock = 45 }
    };

    private readonly List<SaleModel> _salesHistory = new();

    public async Task<List<ProductModel>> GetAvailableProductsAsync()
    {
        await Task.Delay(100); // Simular llamada a API
        return _products.Where(p => p.Stock > 0).ToList();
    }

    public async Task<bool> ProcessSaleAsync(SaleModel sale)
    {
        try
        {
            await Task.Delay(500); // Simular procesamiento
            
            // Verificar stock disponible
            foreach (var item in sale.Items)
            {
                var product = _products.FirstOrDefault(p => p.Name == item.ProductName);
                if (product == null || product.Stock < item.Quantity)
                {
                    return false;
                }
            }
            
            // Actualizar stock
            foreach (var item in sale.Items)
            {
                var product = _products.FirstOrDefault(p => p.Name == item.ProductName);
                if (product != null)
                {
                    product.Stock -= item.Quantity;
                }
            }
            
            sale.Status = SaleStatus.Completed;
            sale.Date = DateTime.Now;
            _salesHistory.Add(sale);
            
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<SaleModel>> GetSalesHistoryAsync()
    {
        await Task.Delay(100);
        return _salesHistory.OrderByDescending(s => s.Date).ToList();
    }
}