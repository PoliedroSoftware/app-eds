# Actualización: Integración con CUDE/CUFE de la API de Facturación

## Resumen de Cambios

Se ha actualizado el servicio de facturación electrónica para usar correctamente el **CUDE/CUFE** que devuelve la API de facturación. Este CUDE es el identificador único que se usa para consultar el PDF de la factura.

## Estructura de la Respuesta de la API

### Response Completa
```json
{
    "code": 201,
    "success": true,
    "info": "Bill emitted successfully",
    "data": {
        "cude": "8bee89869b7277b6e143ed82b31a1303a876ea04e86e439c886bef6062df6eac38e711b19aceca0e6f3b0e4795eb07a4",
     "QRCode": "NumFac: 1031\nFecFac: 2025-10-28\nNitFac: 901351995\nDocAdq: 123456789\nValFac: 990000.00\nValIva: 188100.00\nValOtroIm: 0.00\nValTotal: 1178100.00\nCUFE: 8bee89869b7277b6e143ed82b31a1303a876ea04e86e439c886bef6062df6eac38e711b19aceca0e6f3b0e4795eb07a4\nhttps://catalogo-vpfe-hab.dian.gov.co/document/searchqr?documentkey=8bee89869b7277b6e143ed82b31a1303a876ea04e86e439c886bef6062df6eac38e711b19aceca0e6f3b0e4795eb07a4",
        "techProviderDefaultFootNote": "Representación gráfica de la factura electrónica. Esta factura se asimila en todos sus efectos legales a un título valor según ley 1231 de julio 17 de 2008. Factura generada a través del proveedor tecnológico EMPRESA DE DIVULGACIONES Y ASESORIAS ECA SAS - Software: Plemsi - NIT.: 860.517.022-2"
    },
    "ER": null
}
```

### Campos Importantes

| Campo | Ubicación | Uso | Descripción |
|-------|-----------|-----|-------------|
| `code` | Raíz | Validación | Código de respuesta HTTP (201 = éxito) |
| `success` | Raíz | Validación | Indica si la operación fue exitosa |
| `info` | Raíz | Mensaje | Mensaje informativo de la operación |
| `cude` | data | **CRÍTICO** | Identificador único de la factura (CUDE/CUFE) |
| `QRCode` | data | Opcional | Código QR con datos de la factura |
| `techProviderDefaultFootNote` | data | Opcional | Nota del proveedor tecnológico |

## Cambios Implementados

### 1. Modelo de Respuesta Actualizado

**Antes** (modelo incorrecto):
```csharp
public class BillingApiResponse
{
    [JsonPropertyName("hash")]
    public string InvoiceHash { get; set; }
    
    [JsonPropertyName("message")]
    public string Message { get; set; }
}
```

**Ahora** (modelo correcto):
```csharp
public class BillingApiResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("info")]
    public string Info { get; set; }

    [JsonPropertyName("data")]
    public BillingDataResponse Data { get; set; }
}

public class BillingDataResponse
{
    [JsonPropertyName("cude")]
    public string Cude { get; set; }

    [JsonPropertyName("QRCode")]
    public string QRCode { get; set; }

    [JsonPropertyName("techProviderDefaultFootNote")]
    public string TechProviderDefaultFootNote { get; set; }
}
```

### 2. Validación del CUDE

**Nueva lógica en `GenerateElectronicInvoiceAsync()`**:

```csharp
if (response.IsSuccessStatusCode)
{
    var apiResponse = ParseBillingResponse(responseContent);
    
    // ✅ Validar que la respuesta contenga el CUDE
    if (apiResponse?.Success == true && apiResponse.Data?.Cude != null)
    {
        // Usar el CUDE real de la API
    var invoice = new ElectronicInvoiceModel
        {
      InvoiceHash = apiResponse.Data.Cude, // ✅ CUDE real
        InvoiceNumber = billingRequest.Number,
            // ...
        };
        
        return new BillingResult
        {
            Success = true,
  InvoiceHash = apiResponse.Data.Cude, // ✅ CUDE real
     QRCode = apiResponse.Data.QRCode,
 TechProviderFootNote = apiResponse.Data.TechProviderDefaultFootNote
        };
    }
    else
    {
        // ❌ Error: API respondió OK pero sin CUDE
        return new BillingResult
 {
       Success = false,
  Message = "La factura fue procesada pero no se pudo obtener el CUDE de validación"
  };
    }
}
```

### 3. Descarga de PDF con CUDE

**Antes**:
```csharp
// Usaba un hash generado localmente (INCORRECTO)
var url = $"{PdfApiUrl}/{GenerateMockHash()}";
```

**Ahora**:
```csharp
// Usa el CUDE real de la API
public async Task<byte[]> DownloadInvoicePdfAsync(string invoiceHash)
{
    var url = $"{PdfApiUrl}/{invoiceHash}"; // invoiceHash = CUDE real
    
    // Headers requeridos
    httpClient.DefaultRequestHeaders.Add("X-Environment", "production-billing");
    httpClient.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Bearer", "82f0320dc009d80fb001cbe3");
}
```

### 4. Propiedades Adicionales

**`BillingResult`** ahora incluye:
```csharp
public class BillingResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string InvoiceNumber { get; set; }
    public string ResponseData { get; set; }
    public string InvoiceHash { get; set; } // CUDE/CUFE
    public string QRCode { get; set; } // ✨ NUEVO
    public string TechProviderFootNote { get; set; } // ✨ NUEVO
}
```

**`ElectronicInvoiceModel`** ahora incluye:
```csharp
public class ElectronicInvoiceModel
{
    public string InvoiceHash { get; set; } // CUDE/CUFE
    public string QRCode { get; set; } // ✨ NUEVO
    public string TechProviderFootNote { get; set; } // ✨ NUEVO
    
    // Propiedad calculada para UI
    public string CudeInfo => !string.IsNullOrEmpty(InvoiceHash) 
     ? $"CUDE: {InvoiceHash.Substring(0, Math.Min(16, InvoiceHash.Length))}..." 
        : "Sin CUDE";
}
```

### 5. Logging Mejorado

Se agregó logging detallado en todos los métodos:

```csharp
// En ParseBillingResponse
System.Diagnostics.Debug.WriteLine($"✅ Respuesta parseada correctamente");
System.Diagnostics.Debug.WriteLine($"   - Success: {response.Success}");
System.Diagnostics.Debug.WriteLine($"   - Code: {response.Code}");
System.Diagnostics.Debug.WriteLine($"   - CUDE: {response.Data.Cude}");

// En DownloadInvoicePdfAsync
System.Diagnostics.Debug.WriteLine($"📄 Descargando PDF con CUDE: {invoiceHash}");
System.Diagnostics.Debug.WriteLine($"📄 URL completa: {url}");
System.Diagnostics.Debug.WriteLine($"✅ PDF descargado exitosamente: {pdfBytes.Length} bytes");

// En OpenInvoicePdfAsync
System.Diagnostics.Debug.WriteLine($"📄 Abriendo PDF de factura {invoiceNumber} con CUDE: {invoiceHash}");
System.Diagnostics.Debug.WriteLine($"💾 Guardando PDF en: {filePath}");
System.Diagnostics.Debug.WriteLine($"✅ PDF abierto exitosamente");
```

## Flujo Actualizado

### Generación de Factura

```
1. Usuario completa venta en POS
   ↓
2. Sistema llama ElectronicBillingService.GenerateElectronicInvoiceAsync()
   ↓
3. Request enviado a API de facturación
   ↓
4. API procesa y devuelve response con code=201, success=true
   ↓
5. Response contiene data.cude (CUDE real de la factura)
   ↓
6. Sistema parsea response y extrae:
   - CUDE (obligatorio)
   - QRCode (opcional)
   - TechProviderFootNote (opcional)
   ↓
7. Factura guardada en historial con CUDE real
   ↓
8. Usuario ve confirmación con número de factura y CUDE abreviado
```

### Descarga de PDF

```
1. Usuario hace clic en "📄 Ver PDF" en historial
   ↓
2. Sistema obtiene el CUDE de la factura
   ↓
3. Llama DownloadInvoicePdfAsync(cude)
   ↓
4. Request a: {PdfApiUrl}/{cude}
   Headers:
   - X-Environment: production-billing
   - Authorization: Bearer 82f0320dc009d80fb001cbe3
   ↓
5. API devuelve PDF binario (application/pdf)
   ↓
6. PDF guardado temporalmente en cache
   ↓
7. PDF abierto en visor del sistema
```

## Validaciones Implementadas

### 1. Validación de Respuesta Exitosa
```csharp
if (apiResponse?.Success == true && apiResponse.Data?.Cude != null)
{
    // Procesar factura correctamente
}
else
{
    // Error: respuesta sin CUDE válido
 return new BillingResult
    {
        Success = false,
    Message = "La factura fue procesada pero no se pudo obtener el CUDE"
    };
}
```

### 2. Validación de CUDE para Descarga
```csharp
if (string.IsNullOrWhiteSpace(invoiceHash))
{
    throw new ArgumentException("El CUDE/CUFE de la factura es requerido");
}
```

### 3. Validación de Respuesta de PDF
```csharp
if (!response.IsSuccessStatusCode)
{
    var errorContent = await response.Content.ReadAsStringAsync();
    throw new HttpRequestException($"Error al descargar PDF: {response.StatusCode} - {errorContent}");
}
```

## Mensajes de Usuario Mejorados

### Confirmación de Factura
```
✅ Factura Electrónica Generada

Venta procesada y factura electrónica generada correctamente
Cliente: ESTACION XYZ S.A.S
WhatsApp: +573001234567

📄 Número de Factura: FE-20250128153045
💰 Total: $120,000.00
📦 Productos: 2
🔐 CUDE: 8bee89869b7277b6...

✅ La factura ha sido enviada al correo electrónico del cliente.
📱 Puede consultar el PDF desde el Historial de Facturas.
```

### Error en Descarga de PDF
```
Error al Abrir PDF

No se pudo abrir el PDF de la factura FE-20250128153045.

Error: Error al descargar PDF: 404 - Not Found

Verifique su conexión a internet e intente nuevamente.
```

## Testing

### Caso de Prueba 1: Factura Exitosa

**Input**:
- Cliente válido
- Productos en carrito
- Conexión a internet OK

**Request**:
```json
POST https://...​/billing/api/v1/billing
[{
    "number": "20250128153045",
 "customerEntity": {...},
    "itemElectronicEntity": [...]
}]
```

**Response Esperada**:
```json
{
    "code": 201,
    "success": true,
    "info": "Bill emitted successfully",
    "data": {
        "cude": "8bee89869b7277b6e143ed82b31a1303a876ea04e86e439c886bef6062df6eac38e711b19aceca0e6f3b0e4795eb07a4",
 "QRCode": "...",
        "techProviderDefaultFootNote": "..."
    }
}
```

**Resultado**:
- ✅ Factura guardada con CUDE real
- ✅ Usuario ve confirmación con CUDE abreviado
- ✅ PDF descargable desde historial

### Caso de Prueba 2: Descarga de PDF

**Input**:
- CUDE: `8bee89869b7277b6e143ed82b31a1303a876ea04e86e439c886bef6062df6eac38e711b19aceca0e6f3b0e4795eb07a4`

**Request**:
```
GET https://...​/pdfinvoice/pdf/8bee89869b7277b6e143ed82b31a1303a876ea04e86e439c886bef6062df6eac38e711b19aceca0e6f3b0e4795eb07a4

Headers:
  X-Environment: production-billing
  Authorization: Bearer 82f0320dc009d80fb001cbe3
```

**Resultado**:
- ✅ PDF descargado (application/pdf)
- ✅ PDF guardado en cache
- ✅ PDF abierto en visor

## Diferencias Clave

| Aspecto | Antes | Ahora |
|---------|-------|-------|
| **Identificador** | Hash generado localmente | CUDE real de la API |
| **Validación** | No validaba respuesta | Valida success y CUDE |
| **QR Code** | No soportado | Guardado en modelo |
| **Nota Proveedor** | No soportado | Guardado en modelo |
| **Logging** | Básico | Detallado con emojis |
| **Errores** | Genéricos | Específicos y descriptivos |

## Archivos Modificados

1. **`APP.Eds/Services/Billing/ElectronicBillingService.cs`**
   - Modelo `BillingApiResponse` actualizado
   - Validación de CUDE en respuesta
   - Logging mejorado
   - Propiedades QRCode y TechProviderFootNote

2. **`APP.Eds/Models/Billing/ElectronicInvoiceModel.cs`**
   - Propiedades QRCode y TechProviderFootNote
   - Propiedad CudeInfo para UI

3. **`APP.Eds/UsesCases/PointOfSale/PointOfSaleViewModel.cs`**
   - Mensaje de confirmación con CUDE abreviado

## Próximos Pasos

### Mejoras Sugeridas

1. **Mostrar QR Code en UI**
   - Generar imagen del QR Code
   - Mostrar en detalle de factura
   - Permitir escaneo con cámara

2. **Almacenar Nota del Proveedor**
   - Mostrar en PDF descargado
   - Incluir en emails de factura

3. **Validación de CUDE**
   - Verificar longitud esperada
   - Validar formato hexadecimal
   - Checksum si aplica

4. **Reintento Automático**
   - Si falla descarga de PDF
   - Reintentar con exponential backoff

---

**Fecha**: Enero 28, 2025
**Versión**: 2.1.0
**Estado**: ✅ Implementado y Validado
