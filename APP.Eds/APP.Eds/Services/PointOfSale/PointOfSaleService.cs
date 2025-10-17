using APP.Eds.Models.PointOfSale;
using APP.Eds.Models.Product;
using APP.Eds.Services.Config;
using APP.Eds.Helpers;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Net.Http.Headers;

namespace APP.Eds.Services.PointOfSale;

public interface IPointOfSaleService
{
    Task<List<ProductModel>> GetAvailableProductsAsync();
    Task<bool> ProcessSaleAsync(SaleModel sale);
    Task<List<SaleModel>> GetSalesHistoryAsync();
}

public class PointOfSaleService : IPointOfSaleService
{
    private readonly string? _authToken;
    private readonly List<SaleModel> _salesHistory = new();

    public PointOfSaleService()
    {
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
    }

    public async Task<List<ProductModel>> GetAvailableProductsAsync()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            // Fallback con productos mínimos si no hay token
            return GetSampleProducts();
        }

        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            
            string url = $"{Configuration.BaseUrl}/api/v1/product?includeProductType=true&PageNumber=1&PageSize=100";
            var response = await httpClient.GetStringAsync(url);
            
            var productResponse = JsonSerializer.Deserialize<ProductResponse>(response, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });

            if (productResponse?.Data != null && productResponse.Data.Any())
            {
                // Convertir productos de la API al modelo del punto de venta
                return productResponse.Data
                    .Where(p => p.Stock > 0) // Solo productos con stock
                    .Select(p => new ProductModel
                    {
                        Name = p.Name,
                        IdProductType = p.IdProductType,
                        SellPrice = p.SellPrice,
                        PurchasePrice = p.PurchasePrice,
                        Stock = p.Stock
                    })
                    .ToList();
            }
            else
            {
                return GetSampleProducts();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error obteniendo productos reales: {ex.Message}");
            return GetSampleProducts();
        }
    }

    private List<ProductModel> GetSampleProducts()
    {
        // Solo productos básicos como respaldo
        return
        [
            new ProductModel { Name = "Gasolina Corriente", SellPrice = 3250.00, Stock = 5000 },
            new ProductModel { Name = "Gasolina Extra", SellPrice = 3420.00, Stock = 3500 },
            new ProductModel { Name = "ACPM", SellPrice = 3150.00, Stock = 8000 }
        ];
    }

    public async Task<bool> ProcessSaleAsync(SaleModel sale)
    {
        try
        {
            await Task.Delay(500); // Simular procesamiento
            
            // Obtener productos actuales para verificar stock
            var products = await GetAvailableProductsAsync();
            
            // Verificar stock disponible
            foreach (var item in sale.Items)
            {
                var product = products.FirstOrDefault(p => p.Name == item.ProductName);
                if (product == null || product.Stock < item.Quantity)
                {
                    return false;
                }
            }
            
            // En una implementación real, aquí se actualizaría el stock en la base de datos
            // Por ahora, solo marcamos la venta como completada
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

// Modelos para la respuesta de la API
public class ProductResponse
{
    public List<ProductModelResponse> Data { get; set; } = new();
}

public class ProductModelResponse
{
    public int IdProduct { get; set; }
    public string Name { get; set; } = string.Empty;
    public int IdProductType { get; set; }
    public double SellPrice { get; set; }
    public double PurchasePrice { get; set; }
    public int Stock { get; set; }
}