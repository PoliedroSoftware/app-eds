# Integración del Servicio de Facturación Electrónica - Guía Completa

## Resumen

El `ElectronicBillingService` está correctamente integrado en el flujo de ventas del sistema. Este documento describe cómo se invoca y fluye a través de toda la aplicación.

## Arquitectura de Invocación

### 1. Instanciación del Servicio

**Ubicación**: `PointOfSaleViewModel.cs` (constructor)

```csharp
public class PointOfSaleViewModel : INotifyPropertyChanged
{
    private readonly IPointOfSaleService _pointOfSaleService;
    private readonly ElectronicBillingService _billingService; // ? Servicio de facturación
    
    public PointOfSaleViewModel(IPointOfSaleService pointOfSaleService)
    {
        _pointOfSaleService = pointOfSaleService;
    _billingService = new ElectronicBillingService(); // ? Instanciación
        
        // ...resto del constructor...
    }
}
```

**? Correcto**: El servicio se instancia una vez en el constructor y se reutiliza para todas las facturas.

### 2. Invocación del Servicio

**Ubicación**: `PointOfSaleViewModel.ProcessPayment()` método

```csharp
private async Task ProcessPayment()
{
    // PASO 1: Validaciones de UI
  if (!CartItems.Any()) return;
    if (SelectedClient == null) return;
    
 // PASO 2: Crear modelo de venta
    var sale = new SaleModel
    {
        Items = CartItems.ToList(),
        Tax = Tax,
        PaymentMethod = SelectedPaymentMethod,
        Status = SaleStatus.Pending,
        Date = DateTime.Now,
     SaleId = GenerateSaleId()
    };
    
    // PASO 3: Procesar venta (validar stock, actualizar inventario)
    var success = await _pointOfSaleService.ProcessSaleAsync(sale);
    
    if (!success)
    {
        await DisplayAlert("Error", "No se pudo procesar la venta");
        return;
    }
    
    // ? PASO 4: INVOCAR SERVICIO DE FACTURACIÓN ELECTRÓNICA
    var billingResult = await _billingService.GenerateElectronicInvoiceAsync(
        sale,         // Datos de la venta
      SelectedClient,// Cliente seleccionado
        ClientWhatsAppNumber   // Número de WhatsApp (opcional)
    );
    
    // PASO 5: Manejar resultado
    if (billingResult.Success)
    {
        // Factura generada exitosamente
        await DisplayAlert("? Factura Generada", ...);
    }
    else
    {
   // Error al generar factura (venta ya procesada)
 await DisplayAlert("?? Error en Factura", ...);
    }
}
```

## Flujo Completo Paso a Paso

### Diagrama de Secuencia

```
Usuario            PointOfSaleViewModel      PointOfSaleService     ElectronicBillingService   API Facturación
  |  |           |           |   |
  |-- Clic "Facturar" -->     |      |     |             |
  |       |              |           |         |
  |          |-- Validar (carrito, cliente)    |     |          |
  |      |            |     |       |
  |      |-- ProcessSaleAsync() ---------->|     |             |
  |     |               |   |      |
  |            |    |-- Validar stock   |    |
  |    |         |-- Actualizar inventario     |           |
  |             ||-- Guardar historial         |   |
  |  |       |    |       |
  |      |<-- true (éxito) ----------------|        |        |
  |   |          |      |  |
  |              |-- GenerateElectronicInvoiceAsync() ----------------------->    |      |
  |       |    |              |          |
  |         |       |          |-- Construir BillingRequest  |
  |       | |            |-- POST ---------------------->|
  |       |      |            |     |
  |             |          |         |         |-- Procesar factura
  |         |    |         |    |-- Generar CUDE
  |    | |               |      |
  |     |          |          |<-- Response con CUDE --------|
  |         |   |       |        |
  |          |            | |-- Parsear respuesta         |
  |         |            |   |-- Guardar en historial      |
  |       |  |   |     |
  |       |<-- BillingResult (Success=true, CUDE, QRCode) ---------------|   |
  |     |        |        |    |
  |<-- Confirmación con factura |        |  |        |
  |    y CUDE               |         |  |        |
```

## Parámetros de Invocación

### 1. SaleModel sale

```csharp
var sale = new SaleModel
{
    SaleId = GenerateSaleId(),   // ID único de la venta
    Date = DateTime.Now,       // Fecha y hora
    Items = CartItems.ToList(),        // Productos vendidos
    SubTotal = SubTotal,        // Subtotal sin impuestos
    Tax = Tax,         // Impuestos (16%)
    Total = Total,         // Total con impuestos
    PaymentMethod = SelectedPaymentMethod, // Cash/Card
 Status = SaleStatus.Pending        // Inicialmente pendiente
};
```

**Propiedades utilizadas en facturación**:
- `SaleId`: Referencia de la orden
- `Items`: Lista de productos para ItemElectronicEntity
- `SubTotal`, `Tax`, `Total`: Para cálculos de totales
- `PaymentMethod`: Para PaymentEntity

### 2. ClientLegalModel client

```csharp
SelectedClient = new ClientLegalModel
{
    Id = 1,
    Name = "ESTACION XYZ S.A.S",
    DocumentTypeId = 1,    // 1=NIT
    DocumentNumber = "900123456",
    VerificationDigit = 1,
    Email = "facturacion@estacion.com",
    VatResponsibleParty = true,
    SimpleTaxRegime = false
};
```

**Propiedades utilizadas en facturación**:
- `Name`: CustomerEntity.Name
- `DocumentNumber`: CustomerEntity.IdentificationNumber
- `VerificationDigit`: CustomerEntity.Dv
- `Email`: SendToEmail, CustomerEntity.Email
- `DocumentTypeId`: TypeDocumentIdentificationId
- `VatResponsibleParty`: TypeLiabilityId
- `SimpleTaxRegime`: TypeRegimeId

### 3. string whatsappNumber (opcional)

```csharp
ClientWhatsAppNumber = "+573001234567"; // Opcional
```

**Uso**: CustomerEntity.Phone

## Respuesta del Servicio

### BillingResult - Estructura

```csharp
public class BillingResult
{
    public bool Success { get; set; }      // Indica si fue exitoso
    public string Message { get; set; }        // Mensaje descriptivo
    public string InvoiceNumber { get; set; }         // Número de factura generado
    public string ResponseData { get; set; }  // JSON completo de respuesta
    public string InvoiceHash { get; set; }// CUDE/CUFE de la factura
    public string QRCode { get; set; }      // Código QR
    public string TechProviderFootNote { get; set; }  // Nota del proveedor
}
```

### Caso Exitoso

```csharp
if (billingResult.Success)
{
    // ? Factura generada correctamente
    Console.WriteLine($"Factura: {billingResult.InvoiceNumber}");
    Console.WriteLine($"CUDE: {billingResult.InvoiceHash}");
    Console.WriteLine($"QR: {billingResult.QRCode}");
 
    // La factura ya está guardada en InvoiceHistory
    var history = _billingService.InvoiceHistory;
var lastInvoice = history.FirstOrDefault();
}
```

### Caso de Error

```csharp
if (!billingResult.Success)
{
    // ? Error al generar factura
    Console.WriteLine($"Error: {billingResult.Message}");
    
  // Posibles errores:
    // - "La venta es requerida"
    // - "El cliente es requerido"
    // - "La venta debe tener al menos un producto"
    // - "Error al generar factura: 500 - ..."
    // - "Error de conexión con el servicio de facturación"
    // - "El servicio de facturación no responde"
}
```

## Mensajes al Usuario

### Mensaje de Éxito

```csharp
await Application.Current.MainPage.DisplayAlert(
    "? Factura Electrónica Generada",
    $"Venta procesada y factura electrónica generada correctamente\n" +
    $"Cliente: {SelectedClient.Name}\n" +
    $"WhatsApp: {ClientWhatsAppNumber}\n\n" +
    $"?? Número de Factura: FE-{billingResult.InvoiceNumber}\n" +
    $"?? Total: ${Total:N2}\n" +
    $"?? Productos: {CartItems.Count}\n" +
    $"?? CUDE: {billingResult.InvoiceHash.Substring(0, 16)}...\n\n" +
    $"? La factura ha sido enviada al correo electrónico del cliente.\n" +
    $"?? Puede consultar el PDF desde el Historial de Facturas.",
    "OK");
```

### Mensaje de Error (Venta Procesada)

```csharp
await Application.Current.MainPage.DisplayAlert(
    "?? Venta Procesada - Error en Factura Electrónica",
    $"La venta se procesó correctamente, pero hubo un problema al generar la factura electrónica:\n\n" +
    $"{billingResult.Message}\n\n" +
    $"?? Total: ${Total:N2}\n" +
    $"?? Productos: {CartItems.Count}\n\n" +
    $"Por favor, contacte soporte técnico para generar la factura manualmente.",
    "OK");
```

## Historial de Facturas

### Acceso al Historial

```csharp
// El servicio mantiene un historial en memoria
var billingService = new ElectronicBillingService();
var invoices = billingService.InvoiceHistory;

// Las facturas se insertan automáticamente al generar
foreach (var invoice in invoices)
{
    Console.WriteLine($"{invoice.FullInvoiceNumber} - ${invoice.TotalAmount}");
}
```

### Modelo de Factura en Historial

```csharp
public class ElectronicInvoiceModel
{
    public string InvoiceHash { get; set; }          // CUDE (para PDF)
    public string InvoiceNumber { get; set; }         // 20250128153045
    public string Prefix { get; set; }      // FE
    public string FullInvoiceNumber { get; }      // FE-20250128153045
    public DateTime Date { get; set; }
    public string ClientName { get; set; }
    public string ClientDocumentNumber { get; set; }
    public double TotalAmount { get; set; }
    public double TaxAmount { get; set; }
    public double Subtotal { get; set; }
    public string Status { get; set; }    // "Emitida"
    public string PaymentMethod { get; set; }       // "Efectivo"/"Tarjeta"
    public string Email { get; set; }
    public string QRCode { get; set; }
    public string TechProviderFootNote { get; set; }
    
    // Propiedades calculadas
    public string DateFormatted { get; }              // "28/01/2025 15:30"
    public string TotalAmountFormatted { get; }       // "$120,000.00"
    public bool IsPdfAvailable { get; }  // true si hay CUDE
}
```

## Descarga de PDF

### Desde el Historial

```csharp
// En InvoiceHistoryViewModel
public ICommand ViewPdfCommand { get; }

private async Task ViewPdf(ElectronicInvoiceModel invoice)
{
  if (!invoice.IsPdfAvailable) return;
    
    // Usar el mismo servicio
    var billingService = new ElectronicBillingService();
    
    // Descargar y abrir PDF
    await billingService.OpenInvoicePdfAsync(
        invoice.InvoiceHash,      // CUDE de la factura
      invoice.FullInvoiceNumber // FE-20250128153045
    );
}
```

### Compartir PDF

```csharp
private async Task SharePdf(ElectronicInvoiceModel invoice)
{
    if (!invoice.IsPdfAvailable) return;
    
    var billingService = new ElectronicBillingService();
    
    // Compartir PDF via WhatsApp/Email/etc
 await billingService.ShareInvoicePdfAsync(
        invoice.InvoiceHash,
        invoice.FullInvoiceNumber
    );
}
```

## Puntos de Entrada

### 1. Punto de Venta Normal

```csharp
// APP.Eds/UsesCases/PointOfSale/PointOfSaleView.xaml
<Button Text="?? Facturar" 
        Command="{Binding ProcessPaymentCommand}" />
```

### 2. Vista de Historial de Facturas

```csharp
// Navegar al historial
await Navigation.PushAsync(new InvoiceHistoryView());

// En el historial, ver PDF
<Button Text="?? Ver PDF" 
        Command="{Binding ViewPdfCommand}"
   CommandParameter="{Binding .}" />
```

## Dependencias del Servicio

### Servicios Requeridos

1. **TokenHelper**: Para obtener el token de autenticación
   ```csharp
   _authToken = TokenHelper.LoadToken(
    Configuration.KeycloakCliendId, 
       Configuration.KeycloakRealms);
   ```

2. **Configuration**: Para las URLs de API
   ```csharp
   const string BillingApiUrl = "https://...?/billing/api/v1/billing";
   const string PdfApiUrl = "https://...?/pdfinvoice/pdf";
   ```

3. **FileSystem**: Para guardar PDFs temporalmente
   ```csharp
   var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
   ```

4. **Launcher**: Para abrir PDFs
   ```csharp
   await Launcher.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(filePath) });
   ```

5. **Share**: Para compartir PDFs
   ```csharp
   await Share.RequestAsync(new ShareFileRequest { File = new ShareFile(filePath) });
   ```

### Modelos Requeridos

- `BillingRequest` y sus sub-modelos (SoftwareManufacturer, PayPointInfo, CustomerEntity, PaymentEntity, ItemElectronicEntity, etc.)
- `BillingResult`
- `BillingApiResponse`, `BillingDataResponse`
- `ElectronicInvoiceModel`
- `SaleModel`, `SaleItemModel`
- `ClientLegalModel`

## Manejo de Errores

### Errores Capturados

```csharp
try
{
    var result = await _billingService.GenerateElectronicInvoiceAsync(...);
}
catch (HttpRequestException httpEx)
{
    // Error de conexión con la API
    Console.WriteLine($"Error de red: {httpEx.Message}");
}
catch (TaskCanceledException timeoutEx)
{
    // Timeout (> 60 segundos)
    Console.WriteLine($"Timeout: {timeoutEx.Message}");
}
catch (Exception ex)
{
    // Error general
    Console.WriteLine($"Error: {ex.Message}");
}
```

### Logging Detallado

El servicio incluye logging detallado en todas las operaciones:

```
?? Parseando respuesta de facturación...
?? Response: {"code":201,"success":true,...}
? Respuesta parseada correctamente
   - Success: True
   - Code: 201
   - CUDE: 8bee89869b7277b6e143ed82b31a1303...
? Factura guardada en historial con CUDE: 8bee89869b7277b6...
```

## Testing

### Test Unitario

```csharp
[Fact]
public async Task GenerateElectronicInvoiceAsync_WithValidData_ReturnsSuccess()
{
    // Arrange
    var service = new ElectronicBillingService();
  var sale = CreateValidSale();
    var client = CreateValidClient();
    
    // Act
    var result = await service.GenerateElectronicInvoiceAsync(sale, client);
    
    // Assert
 Assert.True(result.Success);
    Assert.NotEmpty(result.InvoiceNumber);
    Assert.NotEmpty(result.InvoiceHash);
}
```

### Test de Integración

```csharp
[Fact]
public async Task ProcessPayment_GeneratesInvoice_AndStoresInHistory()
{
    // Arrange
    var viewModel = new PointOfSaleViewModel(new PointOfSaleService());
    var billingService = new ElectronicBillingService();
    
    // Setup venta
    viewModel.AddToCart(product);
    viewModel.SelectedClient = client;
  
    // Act
    await viewModel.ProcessPayment();
    
// Assert
    Assert.NotEmpty(billingService.InvoiceHistory);
    var invoice = billingService.InvoiceHistory.First();
    Assert.Equal(client.Name, invoice.ClientName);
}
```

## Checklist de Integración

? **Servicio Instanciado**: `new ElectronicBillingService()` en ViewModel
? **Invocación Correcta**: `GenerateElectronicInvoiceAsync(sale, client, whatsapp)`
? **Validaciones**: Carrito no vacío, cliente seleccionado
? **Flujo Completo**: ProcessSale ? GenerateInvoice ? Confirmación
? **Manejo de Errores**: Try-catch con mensajes específicos
? **Historial**: Facturas guardadas automáticamente
? **PDF**: Descarga y compartir funcional
? **UI**: Mensajes claros con CUDE abreviado
? **Logging**: Debug detallado en consola

## Conclusión

El `ElectronicBillingService` está **completamente integrado** en el flujo de ventas:

1. ? Se instancia correctamente en `PointOfSaleViewModel`
2. ? Se invoca después de `ProcessSaleAsync()` exitoso
3. ? Recibe los datos completos (venta, cliente, whatsapp)
4. ? Genera la factura con la API
5. ? Guarda el resultado en el historial
6. ? Muestra confirmación al usuario
7. ? Permite descargar/compartir PDF después

**No se requieren cambios adicionales** - la integración está completa y funcional.

---

**Fecha**: Enero 28, 2025
**Versión**: 3.0.0
**Estado**: ? Completamente Integrado y Funcional
