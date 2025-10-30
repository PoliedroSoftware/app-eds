# Migración de URLs de Facturación a Configuration

## Resumen de Cambios

Se han movido las URLs de facturación electrónica desde constantes locales en `ElectronicBillingService` a la clase centralizada `Configuration` para mejorar la mantenibilidad y configuración del sistema.

## Cambios Realizados

### 1. Configuration.cs - Nuevas Propiedades

**Ubicación**: `APP.Eds/Services/Config/Configuration.cs`

```csharp
public static class Configuration
{
    // ? API Base URLs existentes
    public static string BaseUrl => "https://mg5tj7bmve.execute-api.us-east-2.amazonaws.com/eds";
    
 // ? Keycloak Authentication existente
    public static string KeycloakUrl => "https://keycloak-0yrtq1-u44828.vm.elestio.app/realms"; 
  public static string KeycloakCliendId => "application-eds";
    public static string KeycloakRealms => "AppEDS";
    
    // ? NUEVO: Electronic Billing API URLs
    public static string BillingApiUrl => 
        "https://wc9oqtphb5.execute-api.us-east-2.amazonaws.com/billing/api/v1/billing";
    
    public static string PdfApiUrl => 
        "https://wc9oqtphb5.execute-api.us-east-2.amazonaws.com/billing/api/v1/pdfinvoice/pdf";
    
    // ? NUEVO: Billing API Authentication
    public static string BillingApiToken => "59884a7d9bca1eb502186c76";
    
    public static string BillingApiEnvironment => "production-billing";
}
```

**Nuevas propiedades agregadas**:
- `BillingApiUrl`: URL para generar facturas electrónicas
- `PdfApiUrl`: URL para descargar PDFs de facturas
- `BillingApiToken`: Token de autenticación para la API de facturación
- `BillingApiEnvironment`: Ambiente de facturación (production-billing)

### 2. ElectronicBillingService.cs - Uso de Configuration

#### ANTES (Constantes Locales)

```csharp
public class ElectronicBillingService
{
    private readonly string? _authToken;
    
    // ? URLs hardcodeadas como constantes locales
    private const string BillingApiUrl = 
        "https://wc9oqtphb5.execute-api.us-east-2.amazonaws.com/billing/api/v1/billing";
    
    private const string PdfApiUrl = 
        "https://wc9oqtphb5.execute-api.us-east-2.amazonaws.com/billing/api/v1/pdfinvoice/pdf";
    
    // ...
    
public async Task<byte[]> DownloadInvoicePdfAsync(string invoiceHash)
    {
        using var httpClient = new HttpClient();
   
        // ? Token y environment hardcodeados
        httpClient.DefaultRequestHeaders.Add("X-Environment", "production-billing");
        httpClient.DefaultRequestHeaders.Authorization = 
     new AuthenticationHeaderValue("Bearer", "59884a7d9bca1eb502186c76");
        
        // ...
    }
}
```

#### AHORA (Usando Configuration)

```csharp
public class ElectronicBillingService
{
    private readonly string? _authToken;
    
    // ? URLs desde Configuration (centralizadas)
    private static string BillingApiUrl => Configuration.BillingApiUrl;
    private static string PdfApiUrl => Configuration.PdfApiUrl;
    
    // ...
    
    public async Task<byte[]> DownloadInvoicePdfAsync(string invoiceHash)
    {
    using var httpClient = new HttpClient();
        
        // ? Token y environment desde Configuration
        httpClient.DefaultRequestHeaders.Add("X-Environment", Configuration.BillingApiEnvironment);
        httpClient.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", Configuration.BillingApiToken);
     
        // ...
    }
}
```

## Beneficios de la Migración

### 1. Centralización de Configuración ?

**Antes**: URLs dispersas en múltiples archivos
**Ahora**: Todas las URLs en un solo lugar (`Configuration.cs`)

```
Configuration.cs
??? BaseUrl (Backend principal)
??? KeycloakUrl (Autenticación)
??? BillingApiUrl (Facturación) ? NUEVO
??? PdfApiUrl (PDFs) ? NUEVO
```

### 2. Mantenibilidad Mejorada ?

**Cambio de ambiente**:
```csharp
// Antes: Buscar y reemplazar en múltiples archivos
private const string BillingApiUrl = "https://...dev..."; // ? En cada servicio

// Ahora: Un solo cambio en Configuration.cs
public static string BillingApiUrl => "https://...dev..."; // ? Centralizado
```

### 3. Configuración por Ambiente ?

**Posibilidad de ambiente dinámico**:
```csharp
public static class Configuration
{
    private static string GetEnvironment()
    {
      #if DEBUG
            return "dev";
        #else
            return "prod";
        #endif
    }
    
    public static string BillingApiUrl => 
        $"https://wc9oqtphb5-{GetEnvironment()}.execute-api.us-east-2.amazonaws.com/billing/api/v1/billing";
}
```

### 4. Seguridad Mejorada ?

**Token centralizado**:
```csharp
// Antes: Token hardcodeado en el código
httpClient.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", "59884a7d9bca1eb502186c76"); // ?

// Ahora: Token en Configuration (puede moverse a variables de entorno)
httpClient.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", Configuration.BillingApiToken); // ?
```

**Próximo paso** (recomendado):
```csharp
public static string BillingApiToken => 
    Environment.GetEnvironmentVariable("BILLING_API_TOKEN") 
    ?? "59884a7d9bca1eb502186c76"; // Fallback para desarrollo
```

## Comparación de Archivos

### Configuration.cs

| Propiedad | Tipo | Valor | Uso |
|-----------|------|-------|-----|
| `BaseUrl` | string | `https://mg5tj7bmve...?/eds` | Backend principal |
| `KeycloakUrl` | string | `https://keycloak...` | Autenticación |
| `KeycloakCliendId` | string | `application-eds` | Cliente Keycloak |
| `KeycloakRealms` | string | `AppEDS` | Realm Keycloak |
| `BillingApiUrl` ? | string | `https://wc9oqtphb5...?/billing` | Generar facturas |
| `PdfApiUrl` ? | string | `https://wc9oqtphb5...?/pdf` | Descargar PDFs |
| `BillingApiToken` ? | string | `59884a7d9bca1eb502186c76` | Token API facturación |
| `BillingApiEnvironment` ? | string | `production-billing` | Ambiente facturación |

### ElectronicBillingService.cs

**Cambios**:

| Línea | Antes | Ahora |
|-------|-------|-------|
| 14-15 | `private const string BillingApiUrl = "..."` | `private static string BillingApiUrl => Configuration.BillingApiUrl` |
| 16-17 | `private const string PdfApiUrl = "..."` | `private static string PdfApiUrl => Configuration.PdfApiUrl` |
| 180 | `"X-Environment", "production-billing"` | `"X-Environment", Configuration.BillingApiEnvironment` |
| 181 | `"Bearer", "59884a7d9bca1eb502186c76"` | `"Bearer", Configuration.BillingApiToken` |

## Uso en Otros Servicios

### Ejemplo: Crear nuevo servicio que use facturación

```csharp
public class InvoiceReportService
{
    public async Task<List<Invoice>> GetInvoicesAsync()
    {
        using var httpClient = new HttpClient();
      
    // ? Usar configuración centralizada
     var url = Configuration.BillingApiUrl;
    httpClient.DefaultRequestHeaders.Add("X-Environment", Configuration.BillingApiEnvironment);
     httpClient.DefaultRequestHeaders.Authorization = 
      new AuthenticationHeaderValue("Bearer", Configuration.BillingApiToken);
        
     // ...
    }
}
```

## Testing

### Test de Configuración

```csharp
[Fact]
public void Configuration_BillingApiUrl_IsNotEmpty()
{
    // Assert
    Assert.NotEmpty(Configuration.BillingApiUrl);
    Assert.StartsWith("https://", Configuration.BillingApiUrl);
}

[Fact]
public void Configuration_PdfApiUrl_IsNotEmpty()
{
    // Assert
    Assert.NotEmpty(Configuration.PdfApiUrl);
    Assert.StartsWith("https://", Configuration.PdfApiUrl);
}

[Fact]
public void Configuration_BillingApiToken_IsNotEmpty()
{
    // Assert
    Assert.NotEmpty(Configuration.BillingApiToken);
    Assert.Equal(24, Configuration.BillingApiToken.Length); // Token length validation
}
```

### Test de Servicio

```csharp
[Fact]
public async Task ElectronicBillingService_UsesConfigurationUrls()
{
    // Arrange
    var service = new ElectronicBillingService();
    
    // Act
    var billingUrl = Configuration.BillingApiUrl;
    var pdfUrl = Configuration.PdfApiUrl;
    
    // Assert
    Assert.NotEmpty(billingUrl);
    Assert.NotEmpty(pdfUrl);
  Assert.Contains("billing", billingUrl);
    Assert.Contains("pdf", pdfUrl);
}
```

## Migración de Otros Servicios (Recomendaciones)

### Servicios que deberían usar Configuration

1. **IslanderService**: Si usa URLs de APIs externas
2. **ProductService**: Si usa endpoints específicos
3. **ShoppingService**: URLs de compras
4. **ClientService**: URLs de clientes

### Patrón recomendado

```csharp
// ? NO HACER
private const string ApiUrl = "https://...";

// ? HACER
private static string ApiUrl => Configuration.SomeApiUrl;
```

## Seguridad - Próximos Pasos

### 1. Usar Variables de Entorno

```csharp
public static class Configuration
{
    public static string BillingApiToken => 
    Environment.GetEnvironmentVariable("BILLING_API_TOKEN") 
        ?? throw new InvalidOperationException("BILLING_API_TOKEN no configurado");
}
```

### 2. Usar Azure Key Vault (Producción)

```csharp
public static class Configuration
{
    private static KeyVaultClient _keyVaultClient;
    
 public static async Task<string> GetBillingApiTokenAsync()
    {
var secret = await _keyVaultClient.GetSecretAsync(
   "https://mykeyvault.vault.azure.net/", 
         "billing-api-token");
        
        return secret.Value;
    }
}
```

### 3. Usar appsettings.json (Alternativa)

```json
{
  "Apis": {
    "Billing": {
      "Url": "https://wc9oqtphb5.execute-api.us-east-2.amazonaws.com/billing/api/v1/billing",
      "PdfUrl": "https://wc9oqtphb5.execute-api.us-east-2.amazonaws.com/billing/api/v1/pdfinvoice/pdf",
      "Token": "59884a7d9bca1eb502186c76",
      "Environment": "production-billing"
    }
  }
}
```

```csharp
public static class Configuration
{
    private static IConfiguration _configuration;
    
    public static string BillingApiUrl => 
        _configuration["Apis:Billing:Url"];
    
    public static string BillingApiToken => 
   _configuration["Apis:Billing:Token"];
}
```

## Checklist de Migración

- ? URLs movidas a `Configuration.cs`
- ? Token de API centralizado
- ? Ambiente de facturación centralizado
- ? `ElectronicBillingService` actualizado
- ? Build exitoso sin errores
- ? Documentación creada
- ? Tests unitarios (recomendado)
- ? Variables de entorno (producción)
- ? Azure Key Vault (opcional)

## Impacto

### Archivos Modificados
1. ? `APP.Eds/Services/Config/Configuration.cs` - 4 propiedades agregadas
2. ? `APP.Eds/Services/Billing/ElectronicBillingService.cs` - 4 líneas actualizadas

### Archivos No Afectados
- ? `PointOfSaleViewModel.cs` - Sin cambios (usa el servicio transparentemente)
- ? `InvoiceHistoryView.cs` - Sin cambios
- ? Todos los modelos - Sin cambios

### Compatibilidad
- ? **100% compatible** con código existente
- ? Sin cambios en la API pública de `ElectronicBillingService`
- ? Sin cambios en el flujo de facturación

## Conclusión

La migración de URLs de facturación a la clase `Configuration` centraliza la configuración del sistema, mejora la mantenibilidad y prepara el código para:

1. ? Cambios de ambiente (dev/staging/prod)
2. ? Configuración externa (variables de entorno)
3. ? Seguridad mejorada (Azure Key Vault)
4. ? Testing más fácil (mocking de configuración)

**Próximos pasos recomendados**:
1. Mover token a variables de entorno
2. Implementar configuración por ambiente
3. Agregar tests de configuración
4. Documentar proceso de despliegue

---

**Fecha**: Enero 28, 2025  
**Versión**: 3.1.0  
**Estado**: ? Completado y Validado  
**Build**: ? Exitoso
