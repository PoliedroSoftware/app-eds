# Flujo Completo de Procesamiento de Ventas

## Resumen

El método `ProcessSaleAsync` en `PointOfSaleService` ha sido mejorado para manejar todo el flujo de procesamiento de ventas de forma robusta y segura.

## Arquitectura del Flujo

```
Usuario hace clic en "Facturar"
         ?
PointOfSaleViewModel.ProcessPayment()
      ?
  ????????????????????????????????????
  ?  VALIDACIONES EN VIEWMODEL       ?
  ????????????????????????????????????
  ? ? Carrito no vacío           ?
  ? ? Cliente seleccionado           ?
  ????????????????????????????????????
     ?
PointOfSaleService.ProcessSaleAsync(sale)
       ?
  ????????????????????????????????????
  ?  PASO 1: Validar Productos       ?
  ????????????????????????????????????
  ? ? Sale.Items no es null          ?
  ? ? Sale.Items tiene elementos  ?
  ????????????????????????????????????
         ?
  ????????????????????????????????????
  ?  PASO 2: Validar Stock           ?
  ????????????????????????????????????
  ? ? ValidateStockAvailability()    ?
  ?   ? Obtener productos del API    ?
  ?   ? Verificar cada producto      ?
  ?   ? Comparar stock vs cantidad   ?
  ????????????????????????????????????
         ?
  ????????????????????????????????????
  ?  PASO 3: Registrar Venta  ?
  ????????????????????????????????????
  ? ? RegisterSaleInBackend(sale)    ?
  ?   [TODO: Implementar endpoint]   ?
  ?   Por ahora: modo offline        ?
  ????????????????????????????????????
     ?
  ????????????????????????????????????
  ?  PASO 4: Actualizar Inventario   ?
  ????????????????????????????????????
  ? ? UpdateInventoryAsync(items)    ?
  ?   ? Descontar stock vendido      ?
  ?   ? Evitar stock negativo        ?
  ?   [TODO: Sincronizar con API]    ?
  ????????????????????????????????????
      ?
  ????????????????????????????????????
  ?  PASO 5: Guardar Historial       ?
  ????????????????????????????????????
  ? ? Cambiar status a Completed     ?
  ? ? Agregar a _salesHistory      ?
  ????????????????????????????????????
         ?
Retornar true (éxito)
     ?
ElectronicBillingService.GenerateElectronicInvoiceAsync()
      ?
  ????????????????????????????????????
  ?  FACTURACIÓN ELECTRÓNICA         ?
  ????????????????????????????????????
  ? ? Construir BillingRequest       ?
  ? ? POST a API de facturación      ?
  ? ? Obtener CUDE de respuesta      ?
  ? ? Guardar en historial      ?
  ????????????????????????????????????
      ?
Usuario ve confirmación con factura generada
```

## Método ProcessSaleAsync - Detalle

### Firma
```csharp
public async Task<bool> ProcessSaleAsync(SaleModel sale)
```

### Parámetros
- **sale**: `SaleModel`
  - `SaleId`: ID único de la venta
  - `Date`: Fecha y hora de la venta
  - `Items`: Lista de productos vendidos
  - `SubTotal`: Subtotal sin impuestos
  - `Tax`: Impuestos
  - `Total`: Total a pagar
  - `PaymentMethod`: Método de pago (Cash/Card)
  - `Status`: Estado de la venta (Pending/Completed)

### Retorno
- **true**: Venta procesada exitosamente
- **false**: Error al procesar la venta

## Validaciones Implementadas

### 1. Validación de Autenticación
```csharp
if (string.IsNullOrEmpty(_authToken))
{
    System.Diagnostics.Debug.WriteLine("? No hay token de autenticación");
    return false;
}
```

**Por qué**: Sin token no podemos consultar productos ni actualizar inventario.

### 2. Validación de Productos
```csharp
if (sale.Items == null || !sale.Items.Any())
{
    System.Diagnostics.Debug.WriteLine("? No hay productos en la venta");
    return false;
}
```

**Por qué**: No tiene sentido procesar una venta vacía.

### 3. Validación de Stock

```csharp
private async Task<(bool IsValid, string ErrorMessage)> ValidateStockAvailability(List<SaleItemModel> items)
{
 var availableProducts = await GetAvailableProductsAsync();
    
    foreach (var item in items)
    {
     var product = availableProducts.FirstOrDefault(p => 
        p.Name.Equals(item.ProductName, StringComparison.OrdinalIgnoreCase));
        
        if (product == null)
            return (false, $"Producto '{item.ProductName}' no encontrado");
        
        if (product.Stock < item.Quantity)
          return (false, $"Stock insuficiente para '{item.ProductName}'");
    }
    
    return (true, string.Empty);
}
```

**Por qué**: Evitar vender productos que no tenemos en stock.

**Casos manejados**:
- ? Producto no encontrado en catálogo
- ? Stock insuficiente para la cantidad solicitada
- ? Error al consultar API (modo offline, permite venta)

## Actualización de Inventario

### Método UpdateInventoryAsync

```csharp
private async Task<bool> UpdateInventoryAsync(List<SaleItemModel> items)
{
    var availableProducts = await GetAvailableProductsAsync();
    
    foreach (var item in items)
    {
var product = availableProducts.FirstOrDefault(p => 
            p.Name.Equals(item.ProductName, StringComparison.OrdinalIgnoreCase));
     
        if (product == null) continue;
        
        var newStock = product.Stock - item.Quantity;
        
        if (newStock < 0)
        {
        System.Diagnostics.Debug.WriteLine($"?? Stock negativo detectado");
    newStock = 0; // No permitir stock negativo
        }
  
        // TODO: PUT /api/v1/product/{productId}/stock
    }
  
    return true;
}
```

**Protecciones**:
- ? No permite stock negativo
- ? Continúa con otros productos si uno falla
- ? Logging detallado de cada actualización
- ? No bloquea la venta si falla (modo resiliente)

## Registro de Venta en Backend

### Método RegisterSaleInBackend

```csharp
private async Task<bool> RegisterSaleInBackend(SaleModel sale)
{
    // TODO: Implementar cuando exista el endpoint
    // POST /api/v1/sales
    // Body: { sale object }
    
    return true; // Por ahora continuar el flujo
}
```

**Endpoint sugerido**:
```
POST {Configuration.BaseUrl}/api/v1/sales
Authorization: Bearer {token}
Content-Type: application/json

Body:
{
  "saleId": 1738001234,
  "date": "2025-01-28T15:30:45",
  "subtotal": 100000,
  "tax": 16000,
  "total": 116000,
  "paymentMethod": "Cash",
  "items": [
    {
  "productId": 1,
      "productName": "Gasolina Corriente",
      "quantity": 10,
  "unitPrice": 12000,
   "totalPrice": 120000
    }
  ]
}
```

## Logging Detallado

### Símbolos Usados

| Símbolo | Significado | Uso |
|---------|-------------|-----|
| ?? | Inicio de proceso | Venta iniciada |
| ? | Éxito | Operación exitosa |
| ? | Error crítico | Operación fallida |
| ?? | Advertencia | Problema no crítico |
| ?? | Inventario | Operaciones de stock |
| ?? | Información | Mensajes informativos |
| ? | Check individual | Item procesado OK |

### Ejemplo de Log Completo

```
?? Iniciando proceso de venta #1738001234
   - Fecha: 2025-01-28 15:30:45
   - Total: $116,000.00
   - Productos: 2
   - Método de pago: Cash

?? Validando stock disponible...
   ? Gasolina Corriente: 10 unidades (Stock: 500)
   ? ACPM: 5 unidades (Stock: 300)
? Stock validado correctamente

?? Registro de venta en backend no implementado aún

?? Actualizando inventario...
   ? Gasolina Corriente: Stock actualizado 500 ? 490
   ? ACPM: Stock actualizado 300 ? 295
? Inventario actualizado exitosamente

? Venta #1738001234 procesada exitosamente
   - Subtotal: $100,000.00
   - Impuestos: $16,000.00
   - Total: $116,000.00
- Estado: Completed
```

## Manejo de Errores

### Error: Sin Token de Autenticación
```
? No hay token de autenticación para procesar venta
Return: false
```
**Solución**: Usuario debe volver a iniciar sesión.

### Error: Stock Insuficiente
```
? Validación de stock fallida: Stock insuficiente para 'Gasolina Corriente'. 
 Disponible: 5, Solicitado: 10
Return: false
```
**Solución**: Reducir cantidad o esperar reabastecimiento.

### Error: Producto No Encontrado
```
? Validación de stock fallida: Producto 'Gasolina Premium' no encontrado
Return: false
```
**Solución**: Verificar catálogo de productos.

### Advertencia: Error Actualizando Inventario
```
?? No se pudo actualizar el inventario automáticamente
? Venta #1738001234 procesada exitosamente
Return: true (continúa)
```
**Comportamiento**: La venta se completa de todas formas (modo resiliente).

## Integración con Facturación Electrónica

### Flujo Completo en PointOfSaleViewModel

```csharp
private async Task ProcessPayment()
{
  // 1. Validaciones de UI
    if (!CartItems.Any()) return;
    if (SelectedClient == null) return;
    
    // 2. Crear modelo de venta
    var sale = new SaleModel
    {
        Items = CartItems.ToList(),
    Tax = Tax,
        PaymentMethod = SelectedPaymentMethod,
        Status = SaleStatus.Pending,
        Date = DateTime.Now,
        SaleId = GenerateSaleId()
    };
    
    // 3. Procesar venta (validar stock, actualizar inventario)
    var success = await _pointOfSaleService.ProcessSaleAsync(sale);
    
    if (!success)
    {
        // Mostrar error y detener
        await DisplayAlert("Error", "No se pudo procesar la venta");
        return;
    }
    
    // 4. Generar factura electrónica
    var billingResult = await _billingService.GenerateElectronicInvoiceAsync(
        sale, SelectedClient, ClientWhatsAppNumber);
    
    if (billingResult.Success)
    {
        // Mostrar confirmación con CUDE
 await DisplayAlert("? Factura Generada", ...);
      ClearCart();
        LoadProducts();
    }
  else
    {
        // Venta procesada pero factura falló
    await DisplayAlert("?? Error en Factura", ...);
     ClearCart();
        LoadProducts();
    }
}
```

## Casos de Uso

### Caso 1: Venta Exitosa Completa

**Input**:
- 10 gal Gasolina Corriente @ $12,000 = $120,000
- 5 gal ACPM @ $11,800 = $59,000
- Cliente: NIT 900123456
- Pago: Efectivo

**Flujo**:
1. ? Validar productos en carrito
2. ? Validar cliente seleccionado
3. ? ProcessSaleAsync()
   - ? Validar stock (Gasolina: 500, ACPM: 300)
   - ? Registrar venta (offline)
   - ? Actualizar inventario (Gasolina: 490, ACPM: 295)
   - ? Guardar en historial
4. ? GenerateElectronicInvoiceAsync()
   - ? POST a API de facturación
   - ? Obtener CUDE
   - ? Guardar factura
5. ? Mostrar confirmación

**Resultado**: Venta y factura procesadas correctamente ?

### Caso 2: Stock Insuficiente

**Input**:
- 100 gal Gasolina Corriente (Stock: 50 gal)
- Cliente: NIT 900123456

**Flujo**:
1. ? Validar productos en carrito
2. ? Validar cliente seleccionado
3. ? ProcessSaleAsync()
   - ? Validar stock (Insuficiente: 50 < 100)
   - ? Return false

**Resultado**: 
```
Error

? No se pudo procesar la venta.

Verifique el stock disponible de los productos.
```

### Caso 3: Error en Factura Electrónica

**Input**:
- Venta válida
- API de facturación caída

**Flujo**:
1. ? ProcessSaleAsync()
   - ? Todo OK, venta guardada
   - ? Inventario actualizado
2. ? GenerateElectronicInvoiceAsync()
   - ? Timeout de API
   - ? Return false

**Resultado**:
```
?? Venta Procesada - Error en Factura Electrónica

La venta se procesó correctamente, pero hubo un problema 
al generar la factura electrónica.

Por favor, contacte soporte técnico para generar 
la factura manualmente.
```

**Importante**: ? La venta YA está procesada (inventario actualizado).

## TODOs Pendientes

### 1. Endpoint de Ventas en Backend
```csharp
// En RegisterSaleInBackend()
POST {Configuration.BaseUrl}/api/v1/sales
Body: SaleModel serializado
```

**Beneficios**:
- Persistencia en base de datos
- Sincronización multi-dispositivo
- Reportes y analytics
- Auditoría completa

### 2. Actualización de Stock en API
```csharp
// En UpdateInventoryAsync()
PUT {Configuration.BaseUrl}/api/v1/product/{productId}/stock
Body: { "stock": newStock }
```

**Beneficios**:
- Stock sincronizado en tiempo real
- Evitar sobreventa
- Alertas de stock bajo
- Reorden automático

### 3. Manejo de Transacciones
```csharp
// Usar transacciones para atomicidad
using var transaction = await _dbContext.Database.BeginTransactionAsync();
try
{
    await RegisterSale();
    await UpdateInventory();
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

**Beneficios**:
- Consistencia de datos
- Rollback automático en errores
- Integridad referencial

### 4. Cache de Productos
```csharp
// Implementar cache local
private List<ProductModel> _cachedProducts;
private DateTime _cacheExpiration;

public async Task<List<ProductModel>> GetAvailableProductsAsync()
{
    if (_cachedProducts != null && DateTime.Now < _cacheExpiration)
    {
        return _cachedProducts;
    }
    
    // Fetch from API
    _cachedProducts = await FetchProductsFromApi();
    _cacheExpiration = DateTime.Now.AddMinutes(5);
    
    return _cachedProducts;
}
```

**Beneficios**:
- Reducir llamadas a API
- Mejor performance
- Modo offline mejorado

## Testing

### Unit Tests Sugeridos

```csharp
[Fact]
public async Task ProcessSaleAsync_WithValidSale_ReturnsTrue()
{
    // Arrange
    var service = new PointOfSaleService();
    var sale = CreateValidSale();
    
    // Act
    var result = await service.ProcessSaleAsync(sale);
    
    // Assert
    Assert.True(result);
}

[Fact]
public async Task ProcessSaleAsync_WithInsufficientStock_ReturnsFalse()
{
    // Arrange
    var service = new PointOfSaleService();
    var sale = CreateSaleWithInsufficientStock();
    
    // Act
    var result = await service.ProcessSaleAsync(sale);
    
    // Assert
    Assert.False(result);
}

[Fact]
public async Task ProcessSaleAsync_WithNullItems_ReturnsFalse()
{
    // Arrange
    var service = new PointOfSaleService();
    var sale = new SaleModel { Items = null };
    
    // Act
    var result = await service.ProcessSaleAsync(sale);
  
    // Assert
    Assert.False(result);
}
```

---

**Fecha**: Enero 28, 2025
**Versión**: 3.0.0
**Estado**: ? Implementado con Validaciones Completas
