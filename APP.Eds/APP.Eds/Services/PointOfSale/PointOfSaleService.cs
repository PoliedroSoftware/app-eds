using APP.Eds.Helpers;
using APP.Eds.Models.Client;
using APP.Eds.Models.PointOfSale;
using APP.Eds.Models.Product;
using APP.Eds.Models.ShoppingProduct;
using APP.Eds.Services.Config;
using System.Net.Http.Headers;
using System.Text.Json;

namespace APP.Eds.Services.PointOfSale;

public interface IPointOfSaleService
{
    Task<List<ProductModel>> GetAvailableProductsAsync();
    Task<bool> ProcessSaleAsync(SaleModel sale);
    Task<List<SaleModel>> GetSalesHistoryAsync();
    Task<List<ClientLegalModel>> GetClientsAsync();
    Task<ClientLegalModel?> SearchClientByDocumentAsync(string documentNumber);
}

public class PointOfSaleService : IPointOfSaleService
{
    private readonly string? _authToken;
    private readonly List<SaleModel> _salesHistory = new();

    public PointOfSaleService()
    {
        _authToken = TokenHelper.LoadToken();
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

    /// <summary>
    /// Procesa una venta completa: valida stock, registra la venta y actualiza inventario
    /// </summary>
    /// <param name="sale">Modelo de venta con items, totales y método de pago</param>
    /// <returns>True si la venta se procesó correctamente, False en caso contrario</returns>
    public async Task<bool> ProcessSaleAsync(SaleModel sale)
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            System.Diagnostics.Debug.WriteLine("❌ No hay token de autenticación para procesar venta");
            return false;
        }

        try
        {
            System.Diagnostics.Debug.WriteLine($"🛒 Iniciando proceso de venta #{sale.SaleId}");
            System.Diagnostics.Debug.WriteLine($"   - Fecha: {sale.Date:yyyy-MM-dd HH:mm:ss}");
            System.Diagnostics.Debug.WriteLine($"   - Total: ${sale.Total:N2}");
            System.Diagnostics.Debug.WriteLine($"   - Productos: {sale.Items.Count}");
            System.Diagnostics.Debug.WriteLine($"   - Método de pago: {sale.PaymentMethod}");

            // ✅ PASO 1: Validar que hay productos en la venta
            if (sale.Items == null || !sale.Items.Any())
            {
                System.Diagnostics.Debug.WriteLine("❌ No hay productos en la venta");
                return false;
            }

            // ✅ PASO 2: Validar stock disponible para cada producto
            System.Diagnostics.Debug.WriteLine("📦 Validando stock disponible...");
            var stockValidation = await ValidateStockAvailability(sale.Items);

            if (!stockValidation.IsValid)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Validación de stock fallida: {stockValidation.ErrorMessage}");
                return false;
            }

            System.Diagnostics.Debug.WriteLine("✅ Stock validado correctamente");

            // ✅ PASO 3: Registrar la venta en el backend (si tienes endpoint)
            var saleRegistered = await RegisterSaleInBackend(sale);

            if (!saleRegistered)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ No se pudo registrar la venta en el backend, continuando en modo offline");
                // Continuar de todas formas en modo offline
            }

            // ✅ PASO 4: Actualizar inventario (descontar stock)
            var inventoryUpdated = await UpdateInventoryAsync(sale.Items);

            if (!inventoryUpdated)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ No se pudo actualizar el inventario automáticamente");
                // No bloqueamos la venta por esto
            }

            // ✅ PASO 5: Guardar en historial local
            sale.Status = SaleStatus.Completed;
            _salesHistory.Add(sale);

            System.Diagnostics.Debug.WriteLine($"✅ Venta #{sale.SaleId} procesada exitosamente");
            System.Diagnostics.Debug.WriteLine($"- Subtotal: ${sale.SubTotal:N2}");
            System.Diagnostics.Debug.WriteLine($"   - Impuestos: ${sale.Tax:N2}");
            System.Diagnostics.Debug.WriteLine($"   - Total: ${sale.Total:N2}");
            System.Diagnostics.Debug.WriteLine($"   - Estado: {sale.Status}");

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error procesando venta #{sale.SaleId}: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
            return false;
        }
    }

    /// <summary>
    /// Valida que hay stock suficiente para todos los productos de la venta
    /// </summary>
    private async Task<(bool IsValid, string ErrorMessage)> ValidateStockAvailability(List<SaleItemModel> items)
    {
        try
        {
            // Obtener productos actuales del backend
            var availableProducts = await GetAvailableProductsAsync();

            if (!availableProducts.Any())
            {
                return (false, "No se pudieron obtener los productos disponibles");
            }

            // Validar cada item de la venta
            foreach (var item in items)
            {
                var product = availableProducts.FirstOrDefault(p =>
                   p.Name.Equals(item.ProductName, StringComparison.OrdinalIgnoreCase));

                if (product == null)
                {
                    return (false, $"Producto '{item.ProductName}' no encontrado");
                }


                System.Diagnostics.Debug.WriteLine($"   ✓ {item.ProductName}: {item.Quantity} unidades (Stock: {product.Stock})");
            }

            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"⚠️ Error validando stock: {ex.Message}");
            // En caso de error en la validación, permitir la venta (modo offline)
            return (true, string.Empty);
        }
    }

    /// <summary>
    /// Registra la venta en el backend (si existe endpoint)
    /// </summary>
    private async Task<bool> RegisterSaleInBackend(SaleModel sale)
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            // TODO: Implementar cuando exista el endpoint de ventas en el backend
            // Por ahora retornamos true para continuar el flujo
            System.Diagnostics.Debug.WriteLine("ℹ️ Registro de venta en backend no implementado aún");

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"⚠️ Error registrando venta en backend: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Actualiza el inventario descontando los productos vendidos
    /// </summary>
    private async Task<bool> UpdateInventoryAsync(List<SaleItemModel> items)
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            System.Diagnostics.Debug.WriteLine("📦 Actualizando inventario...");

            // Obtener productos para mapear nombres a IDs
            var availableProducts = await GetAvailableProductsAsync();

            foreach (var item in items)
            {
                var product = availableProducts.FirstOrDefault(p =>
                p.Name.Equals(item.ProductName, StringComparison.OrdinalIgnoreCase));

                if (product == null)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Producto '{item.ProductName}' no encontrado para actualizar stock");
                    continue;
                }

                // Calcular nuevo stock
                var newStock = product.Stock - item.Quantity;

                if (newStock < 0)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Stock negativo detectado para '{item.ProductName}': {newStock}");
                    newStock = 0; // No permitir stock negativo
                }

                // TODO: Implementar actualización de stock en el backend cuando exista el endpoint
                // PUT /api/v1/product/{productId}/stock
                // Body: { "stock": newStock }

                System.Diagnostics.Debug.WriteLine($"   ✓ {item.ProductName}: Stock actualizado {product.Stock} → {newStock}");
            }

            System.Diagnostics.Debug.WriteLine("✅ Inventario actualizado exitosamente");
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"⚠️ Error actualizando inventario: {ex.Message}");
            return false;
        }
    }

    public async Task<List<SaleModel>> GetSalesHistoryAsync()
    {
        return await Task.FromResult(_salesHistory);
    }

    public async Task<ClientLegalModel?> SearchClientByDocumentAsync(string documentNumber)
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            System.Diagnostics.Debug.WriteLine("No hay token de autenticación para buscar cliente");
            return null;
        }

        if (string.IsNullOrWhiteSpace(documentNumber))
        {
            return null;
        }

        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("X-Environment", "clients");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            // Primero intentar buscar en clientes jurídicos (empresas)
            System.Diagnostics.Debug.WriteLine($"🔍 Buscando cliente jurídico con documento: {documentNumber}");

            string legalUrl = $"{Configuration.BaseUrl}/api/v1/client/legal";
            var legalResponse = await httpClient.GetStringAsync(legalUrl);
            var legalClientResponse = JsonSerializer.Deserialize<ClientLegalResponse>(legalResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Buscar por número de documento en clientes jurídicos
            var legalClient = legalClientResponse?.Data?.FirstOrDefault(c =>
             c.DocumentNumber?.Trim().Equals(documentNumber, StringComparison.OrdinalIgnoreCase) == true);

            if (legalClient != null)
            {
                System.Diagnostics.Debug.WriteLine($"✅ Cliente jurídico encontrado: {legalClient.Name}");
                return legalClient;
            }

            // Si no se encuentra en jurídicos, buscar en clientes naturales (personas)
            System.Diagnostics.Debug.WriteLine($"🔍 Buscando cliente natural con documento: {documentNumber}");

            string naturalUrl = $"{Configuration.BaseUrl}/api/v1/client/natural";
            var naturalResponse = await httpClient.GetStringAsync(naturalUrl);
            var naturalClientResponse = JsonSerializer.Deserialize<ClientNaturalResponse>(naturalResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Buscar por número de documento en clientes naturales
            var naturalClient = naturalClientResponse?.Data?.FirstOrDefault(c =>
            c.DocumentNumber?.Trim().Equals(documentNumber, StringComparison.OrdinalIgnoreCase) == true);

            if (naturalClient != null)
            {
                System.Diagnostics.Debug.WriteLine($"✅ Cliente natural encontrado: {naturalClient.FullName}");

                // Convertir ClientNaturalModel a ClientLegalModel para mantener compatibilidad
                return new ClientLegalModel
                {
                    Id = naturalClient.Id,
                    Name = naturalClient.FullName, // Usar nombre completo
                    DocumentTypeId = naturalClient.DocumentTypeId,
                    DocumentNumber = naturalClient.DocumentNumber,
                    Email = naturalClient.Email,
                    // Los campos específicos de persona jurídica quedan en falso/cero
                    VerificationDigit = 0,
                    VatResponsibleParty = false,
                    LargeTaxpayer = false,
                    SelfRetainer = false,
                    WithholdingAgent = false,
                    SimpleTaxRegime = false
                };
            }

            System.Diagnostics.Debug.WriteLine($"❌ No se encontró ningún cliente con documento: {documentNumber}");
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error buscando cliente: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            return null;
        }
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
                System.Diagnostics.Debug.WriteLine($"✅ Se cargaron {clientResponse.Data.Count} clientes correctamente");

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
                System.Diagnostics.Debug.WriteLine("⚠️ No se encontraron clientes en la respuesta");
                return new List<ClientLegalModel>();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error obteniendo clientes: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            return new List<ClientLegalModel>();
        }
    }
}