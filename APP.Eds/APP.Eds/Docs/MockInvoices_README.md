# ?? Mock de Facturas Electrónicas - Guía de Uso

## ?? Descripción

Este sistema permite generar y visualizar facturas electrónicas de ejemplo utilizando el archivo PDF `factura.pdf` incluido en el proyecto. Es ideal para pruebas, desarrollo y demos sin necesidad de conectar con el servicio real de facturación.

## ?? Archivos Creados

### 1. `MockInvoiceService.cs`
**Ubicación:** `APP.Eds\Services\Billing\MockInvoiceService.cs`

**Funciones principales:**
- `GetMockInvoices()`: Genera 10 facturas de ejemplo
- `GetMockPdfAsync()`: Carga el PDF de ejemplo desde `factura.pdf`
- `OpenMockPdfAsync()`: Abre el PDF mock en el visor del sistema
- `ShareMockPdfAsync()`: Comparte el PDF mock
- `AddMockInvoice()`: Agrega una nueva factura mock al historial
- `ClearMockInvoices()`: Limpia todas las facturas mock

### 2. `ElectronicBillingService.cs` (Actualizado)
**Cambios realizados:**
- Agregada propiedad `UseMockData` para activar/desactivar el modo mock
- Modificada propiedad `InvoiceHistory` para cargar facturas mock cuando `UseMockData = true`

### 3. `InvoiceHistoryViewModel.cs` (Actualizado)
**Cambios realizados:**
- Métodos `ViewPdf` y `SharePdf` ahora usan PDFs mock cuando el modo mock está activo

## ?? Cómo Usar

### Opción 1: Activar Modo Mock Globalmente

En `App.xaml.cs` o `MauiProgram.cs`, agregar:

```csharp
using APP.Eds.Services.Billing;

// Activar modo mock para desarrollo/testing
ElectronicBillingService.UseMockData = true;
```

### Opción 2: Activar Modo Mock Condicionalmente

```csharp
#if DEBUG
ElectronicBillingService.UseMockData = true;
#else
ElectronicBillingService.UseMockData = false;
#endif
```

### Opción 3: Usar Directamente las Facturas Mock

```csharp
using APP.Eds.Services.Billing;

// Obtener facturas mock
var mockInvoices = MockInvoiceService.GetMockInvoices();

// Agregar una factura mock nueva
MockInvoiceService.AddMockInvoice();

// Abrir PDF mock
await MockInvoiceService.OpenMockPdfAsync("SETT-20241215-001");

// Compartir PDF mock
await MockInvoiceService.ShareMockPdfAsync("SETT-20241215-001");

// Limpiar todas las facturas mock
MockInvoiceService.ClearMockInvoices();
```

## ?? Datos de las Facturas Mock

### Clientes de Ejemplo:
1. **ABC DISTRIBUIDORA S.A.S** - NIT 900123456
2. **TRANSPORTES DEL SUR LTDA** - NIT 800234567
3. **COMERCIALIZADORA ANDINA S.A** - NIT 900345678
4. **JUAN CARLOS RODRIGUEZ** - CC 79456123
5. **MARIA FERNANDA GARCIA** - CC 52789456
6. **INVERSIONES EL TRIUNFO S.A.S** - NIT 900456789
7. **SERVICIOS INTEGRALES DEL CARIBE** - NIT 900567890
8. **PEDRO ANTONIO MARTINEZ** - CC 1015678901
9. **LOGISTICA Y TRANSPORTE NACIONAL** - NIT 900678901
10. **ANA LUCIA TORRES** - CC 41234567

### Características de las Facturas Mock:
- **Números de factura**: Generados automáticamente con formato `yyyyMMddHHmmss`
- **Prefijo**: `SETT`
- **Fechas**: Distribuidas en los últimos 30 días
- **Montos**: Entre $50,000 y $500,000 (valores aleatorios)
- **IVA**: 19% sobre el subtotal
- **Métodos de pago**: Efectivo, Tarjeta, Transferencia
- **Estados**: Todas marcadas como "Emitida"
- **CUDE**: Hash de 64 caracteres simulado (formato SHA-256)
- **QR Code**: Código base64 simulado

## ??? Flujo de Uso en la Aplicación

### 1. Ver Historial de Facturas

```csharp
// En InvoiceHistoryView
ElectronicBillingService.UseMockData = true;
var viewModel = new InvoiceHistoryViewModel();
// El historial se cargará automáticamente con 10 facturas mock
```

### 2. Visualizar PDF

Cuando el usuario hace clic en "?? Ver PDF":
1. El sistema detecta si `UseMockData = true`
2. Si está activo, carga `factura.pdf` desde los recursos del proyecto
3. Guarda una copia en `FileSystem.CacheDirectory`
4. Abre el PDF en el visor del sistema operativo

### 3. Compartir PDF

Cuando el usuario hace clic en "?? Compartir":
1. Carga `factura.pdf` desde los recursos
2. Muestra el sheet de compartir del sistema operativo
3. Permite enviar por WhatsApp, email, etc.

## ?? Notas Importantes

### ? Ventajas del Sistema Mock:
- ? **No requiere conexión a internet**
- ? **No consume créditos del servicio de facturación real**
- ? **Ideal para demos y presentaciones**
- ? **Permite probar la UI sin dependencias externas**
- ? **Datos realistas y variados**

### ?? Consideraciones:
- El PDF mock es el mismo para todas las facturas
- Los datos (cliente, monto, fecha) son simulados
- El CUDE es aleatorio y no válido para DIAN
- **Recuerda desactivar el modo mock en producción**

## ?? Cambiar entre Modo Mock y Modo Real

```csharp
// Desarrollo/Testing
ElectronicBillingService.UseMockData = true;

// Producción
ElectronicBillingService.UseMockData = false;
```

## ?? Ubicación del PDF de Ejemplo

El archivo `factura.pdf` debe estar en:
```
APP.Eds/factura.pdf
```

Y debe estar configurado como **MauiAsset** en el `.csproj`:
```xml
<MauiAsset Include="factura.pdf" />
```

O en **Resources/Raw**:
```
APP.Eds/Resources/Raw/factura.pdf
```

## ?? Ejemplo Completo de Uso

```csharp
using APP.Eds.Services.Billing;
using APP.Eds.Models.Billing;

// 1. Activar modo mock
ElectronicBillingService.UseMockData = true;

// 2. Obtener servicio de facturación
var billingService = new ElectronicBillingService();

// 3. Cargar facturas mock (se hace automáticamente al acceder a InvoiceHistory)
var invoices = billingService.InvoiceHistory;

// 4. Ver la primera factura
var firstInvoice = invoices.FirstOrDefault();
if (firstInvoice != null)
{
    // Ver PDF
    await MockInvoiceService.OpenMockPdfAsync(firstInvoice.FullInvoiceNumber);
    
    // O compartir
    await MockInvoiceService.ShareMockPdfAsync(firstInvoice.FullInvoiceNumber);
}

// 5. Agregar una nueva factura mock
MockInvoiceService.AddMockInvoice();

// 6. Desactivar modo mock cuando termines
ElectronicBillingService.UseMockData = false;
```

## ?? Personalización

### Modificar Datos de Clientes Mock:

Editar el array `clients` en `MockInvoiceService.GenerateMockInvoice()`:

```csharp
var clients = new[]
{
    new { Name = "TU EMPRESA", DocNumber = "123456789", DocType = "NIT", Email = "email@empresa.com" },
    // ... más clientes
};
```

### Cambiar Rangos de Montos:

```csharp
var subtotal = _random.Next(TU_MINIMO, TU_MAXIMO);
```

### Agregar Más Métodos de Pago:

```csharp
var paymentMethods = new[] { "Efectivo", "Tarjeta", "Transferencia", "TuNuevoMétodo" };
```

## ?? Troubleshooting

### Problema: El PDF no se carga
**Solución:** Verificar que `factura.pdf` esté en `Resources/Raw` y configurado como `MauiAsset`

### Problema: Error al abrir PDF
**Solución:** Verificar que el dispositivo tenga un visor de PDF instalado

### Problema: Las facturas mock no aparecen
**Solución:** Verificar que `ElectronicBillingService.UseMockData = true`

## ?? Soporte

Para más información o problemas, contactar al equipo de desarrollo de Poliedro Software.

---

**Versión:** 1.0  
**Fecha:** Diciembre 2024  
**Autor:** Poliedro Software
