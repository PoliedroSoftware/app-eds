using APP.Eds.Models.PointOfSale;
using APP.Eds.Models.Product;
using APP.Eds.Models.Client;
using APP.Eds.Models.ShoppingProduct;
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
    Task<List<ClientLegalModel>> GetClientsAsync();
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
 System.Diagnostics.Debug.WriteLine("No hay token de autenticación para obtener productos");
    return new List<ProductModel>();
     }

 try
        {
   using var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
 
          string url = $"{Configuration.BaseUrl}/api/v1/product";
     var response = await httpClient.GetStringAsync(url);
    
 var productResponse = JsonSerializer.Deserialize<ProductApiResponse>(response, new JsonSerializerOptions 
    { 
         PropertyNameCaseInsensitive = true 
      });

 // Convertir de ProductResponse (del API) a ProductModel
   if (productResponse?.Data != null)
    {
   return productResponse.Data.Select(p => new ProductModel
     {
  IdProduct = p.IdProduct,
     Name = p.Name,
  IdProductType = p.IdProductType,
     SellPrice = p.SellPrice,
     PurchasePrice = p.PurchasePrice,
      Stock = (int)p.Stock
    }).ToList();
    }

     return new List<ProductModel>();
  }
      catch (Exception ex)
     {
    System.Diagnostics.Debug.WriteLine($"Error obteniendo productos: {ex.Message}");
    return new List<ProductModel>();
        }
    }

    public async Task<bool> ProcessSaleAsync(SaleModel sale)
    {
      try
   {
  // Aquí implementarías la lógica para procesar la venta
    _salesHistory.Add(sale);
 return true;
        }
        catch (Exception ex)
{
         System.Diagnostics.Debug.WriteLine($"Error procesando venta: {ex.Message}");
       return false;
    }
    }

    public async Task<List<SaleModel>> GetSalesHistoryAsync()
    {
        return await Task.FromResult(_salesHistory);
    }

    public async Task<List<ClientLegalModel>> GetClientsAsync()
    {
      if (string.IsNullOrEmpty(_authToken))
        {
      System.Diagnostics.Debug.WriteLine("No hay token de autenticación para obtener clientes");
return new List<ClientLegalModel>();
     }

     try
  {
          using var httpClient = new HttpClient();
   httpClient.DefaultRequestHeaders.Clear();
     httpClient.DefaultRequestHeaders.Add("X-Environment", "clients");
 httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
 
  string url = $"{Configuration.BaseUrl}/api/v1/client/legal";
   System.Diagnostics.Debug.WriteLine($"Llamando a URL de clientes: {url}");
    
    var response = await httpClient.GetStringAsync(url);
        System.Diagnostics.Debug.WriteLine($"Respuesta completa de clientes: {response}");
    
  var clientResponse = JsonSerializer.Deserialize<ClientLegalResponse>(response, new JsonSerializerOptions 
    { 
   PropertyNameCaseInsensitive = true 
      });

  if (clientResponse?.Data != null && clientResponse.Data.Any())
       {
     System.Diagnostics.Debug.WriteLine($"? Se cargaron {clientResponse.Data.Count} clientes correctamente");
  
            // Log detallado de los primeros 3 clientes
 for (int i = 0; i < Math.Min(3, clientResponse.Data.Count); i++)
  {
   var client = clientResponse.Data[i];
        System.Diagnostics.Debug.WriteLine($"Cliente {i + 1}:");
 System.Diagnostics.Debug.WriteLine($"  - ID: {client.Id}");
   System.Diagnostics.Debug.WriteLine($"  - Nombre: '{client.Name}'");
          System.Diagnostics.Debug.WriteLine($"  - DocTypeId: {client.DocumentTypeId} ({client.DocumentType})");
 System.Diagnostics.Debug.WriteLine($"  - DocNumber: '{client.DocumentNumber}'");
     System.Diagnostics.Debug.WriteLine($"  - VerificationDigit: {client.VerificationDigit}");
 System.Diagnostics.Debug.WriteLine($"  - DisplayText: '{client.DisplayText}'");
   }
           
         return clientResponse.Data;
    }
          else
    {
  System.Diagnostics.Debug.WriteLine("?? No se encontraron clientes en la respuesta");
      return new List<ClientLegalModel>();
}
     }
     catch (Exception ex)
   {
     System.Diagnostics.Debug.WriteLine($"? Error obteniendo clientes: {ex.Message}");
       System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            return new List<ClientLegalModel>();
   }
    }
}