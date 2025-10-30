# Historial de Facturas Electrónicas - Implementación Completa

## Resumen

Se ha implementado un sistema completo de historial de facturas electrónicas con funcionalidad para ver y compartir PDFs. Esta implementación permite a los usuarios consultar todas las facturas generadas y descargar/compartir los PDFs directamente desde la aplicación.

## Archivos Creados/Modificados

### 1. **APP.Eds/Models/Billing/ElectronicInvoiceModel.cs** (NUEVO)
Modelo de datos para representar una factura electrónica en el historial:

**Propiedades principales**:
- `InvoiceHash`: Hash único de la factura (necesario para descargar el PDF)
- `InvoiceNumber`: Número de la factura
- `Prefix`: Prefijo (ej: "FE")
- `Date`: Fecha y hora de emisión
- `ClientName`: Nombre del cliente
- `ClientDocumentNumber`: NIT/Cédula
- `TotalAmount`: Monto total
- `Status`: Estado de la factura
- `PaymentMethod`: Método de pago

**Propiedades calculadas**:
- `FullInvoiceNumber`: Prefijo + Número (ej: "FE-20250128153045")
- `DateFormatted`: Fecha formateada (ej: "28/01/2025 15:30")
- `TotalAmountFormatted`: Monto formateado (ej: "$120,000.00")
- `StatusIcon`, `PaymentMethodIcon`: Iconos visuales

### 2. **APP.Eds/Services/Billing/ElectronicBillingService.cs** (MODIFICADO)

**Nuevas funcionalidades agregadas**:

```csharp
// Historial en memoria (Observable para UI automática)
public ObservableCollection<ElectronicInvoiceModel> InvoiceHistory { get; }

// Descargar PDF de una factura
public async Task<byte[]> DownloadInvoicePdfAsync(string invoiceHash)

// Abrir PDF en visor del sistema
public async Task<bool> OpenInvoicePdfAsync(string invoiceHash, string invoiceNumber)

// Compartir PDF
public async Task<bool> ShareInvoicePdfAsync(string invoiceHash, string invoiceNumber)
```

**Integración con API de PDFs**:
- URL base: `https://wc9oqtphb5.execute-api.us-east-2.amazonaws.com/billing/api/v1/pdfinvoice/pdf`
- Headers requeridos:
  - `X-Environment: production-billing`
  - `Authorization: Bearer 82f0320dc009d80fb001cbe3`

**Almacenamiento automático**:
Cuando se genera una factura exitosamente con `GenerateElectronicInvoiceAsync()`, automáticamente:
1. Parsea la respuesta del servidor para obtener el `hash` de la factura
2. Crea un objeto `ElectronicInvoiceModel` con todos los datos
3. Lo inserta en la colección `InvoiceHistory` (al inicio para mostrar las más recientes primero)

### 3. **APP.Eds/UsesCases/Billing/InvoiceHistoryViewModel.cs** (NUEVO)

ViewModel que gestiona la lógica de la vista de historial:

**Propiedades**:
- `Invoices`: Colección observable enlazada al servicio
- `IsLoading`: Estado de carga
- `IsEmpty`: Indica si no hay facturas

**Comandos**:
- `ViewPdfCommand`: Abre el PDF en el visor del sistema
- `SharePdfCommand`: Comparte el PDF vía WhatsApp, email, etc.
- `RefreshCommand`: Actualiza la lista

### 4. **APP.Eds/UsesCases/Billing/InvoiceHistoryView.cs** (NUEVO)

Vista que muestra el historial de facturas con un diseño profesional:

**Características de UI**:
- ?? **CollectionView** para lista scrolleable
- ?? **Cards** con sombras y bordes redondeados
- ?? **Información completa** de cada factura:
  - Número de factura con prefijo
  - Cliente y documento
  - Fecha formateada
  - Total con formato moneda
  - Estado visual con íconos
  - Método de pago con íconos
- ?? **Botones de acción**:
  - "?? Ver PDF" (morado)
  - "?? Compartir" (verde)
- ?? **EmptyState** cuando no hay facturas
- ? **LoadingOverlay** durante operaciones

## Flujo de Uso

### 1. Generar una Factura (Punto de Venta)

```
Usuario hace clic en "Facturar"
    ?
PointOfSaleViewModel.ProcessPayment()
    ?
ElectronicBillingService.GenerateElectronicInvoiceAsync()
    ?
[Envía request a API de facturación]
    ?
[Recibe respuesta con hash de factura]
    ?
[Crea ElectronicInvoiceModel]
    ?
[Agrega a InvoiceHistory automáticamente]
    ?
Usuario ve confirmación
```

### 2. Ver Historial de Facturas

```
Usuario navega a InvoiceHistoryView
    ?
ViewModel carga InvoiceHistory del servicio
    ?
CollectionView muestra todas las facturas
    ?
Usuario hace scroll para ver facturas anteriores
```

### 3. Ver PDF de una Factura

```
Usuario hace clic en "?? Ver PDF"
    ?
InvoiceHistoryViewModel.ViewPdf(invoice)
    ?
ElectronicBillingService.OpenInvoicePdfAsync(hash, number)
    ?
[Descarga PDF desde API]
    ?
[Guarda PDF temporalmente en caché]
    ?
[Abre PDF con visor del sistema]
    ?
Usuario ve el PDF
```

### 4. Compartir PDF de una Factura

```
Usuario hace clic en "?? Compartir"
    ?
InvoiceHistoryViewModel.SharePdf(invoice)
    ?
ElectronicBillingService.ShareInvoicePdfAsync(hash, number)
  ?
[Descarga PDF desde API]
    ?
[Guarda PDF temporalmente]
    ?
[Abre sheet de compartir del sistema]
    ?
Usuario selecciona app (WhatsApp, Email, etc.)
    ?
PDF compartido exitosamente
```

## Estructura de una Card de Factura

```
????????????????????????????????????????????
? FE-20250128153045     ? Emitida      ? ? Header
????????????????????????????????????????????
? ?? ESTACION XYZ S.A.S     ?
?    900123456-1     ? ? Cliente
?    28/01/2025 15:30               ?
????????????????????????????????????????????
? ?? Efectivo        $120,000.00       ? ? Totales
????????????????????????????????????????????
? [?? Ver PDF]  [?? Compartir]      ? ? Acciones
????????????????????????????????????????????
```

## Integración con API de Facturación

### Endpoint para generar factura
```
POST https://wc9oqtphb5.execute-api.us-east-2.amazonaws.com/billing/api/v1/billing
```

**Respuesta exitosa**:
```json
{
  "hash": "eda03fac259eec231663c7032b76c11008e7468e774e417af8a086b9e8efffb2788e7f5f46aab86ca7b691524b8652d2",
  "cude": "...",
  "message": "Factura generada exitosamente",
  "success": true
}
```

### Endpoint para descargar PDF
```
GET https://wc9oqtphb5.execute-api.us-east-2.amazonaws.com/billing/api/v1/pdfinvoice/pdf/{hash}

Headers:
  X-Environment: production-billing
  Authorization: Bearer 82f0320dc009d80fb001cbe3
```

**Respuesta**: PDF binario (application/pdf)

## Navegación

Para navegar al historial de facturas desde cualquier parte de la app:

```csharp
await Navigation.PushAsync(new InvoiceHistoryView());
```

Ejemplo de integración en un menú:

```csharp
var menuButton = new Button
{
    Text = "?? Historial de Facturas",
    Command = new Command(async () =>
    {
        await Navigation.PushAsync(new InvoiceHistoryView());
    })
};
```

## Casos de Uso

### Caso 1: Usuario Consulta Facturas del Día

**Escenario**: Usuario quiere ver todas las facturas emitidas hoy

**Pasos**:
1. Abrir "Historial de Facturas"
2. Las facturas más recientes aparecen primero
3. Filtrar visualmente por fecha

**Resultado**: Usuario ve lista ordenada cronológicamente

### Caso 2: Usuario Necesita Reenviar una Factura

**Escenario**: Cliente solicita reenvío de factura por WhatsApp

**Pasos**:
1. Abrir "Historial de Facturas"
2. Buscar la factura por número o cliente
3. Hacer clic en "?? Compartir"
4. Seleccionar WhatsApp
5. Seleccionar contacto del cliente
6. Enviar

**Resultado**: PDF enviado exitosamente por WhatsApp

### Caso 3: Auditoría Requiere PDF de Factura

**Escenario**: Auditor solicita ver una factura específica

**Pasos**:
1. Abrir "Historial de Facturas"
2. Localizar factura FE-20250128153045
3. Hacer clic en "?? Ver PDF"
4. PDF se abre en visor del sistema
5. Auditor revisa el documento

**Resultado**: PDF visualizado correctamente

## Ventajas de la Implementación

? **Almacenamiento Eficiente**: Facturas guardadas en memoria (Observable para UI reactiva)

? **Descarga Automática**: PDFs descargados on-demand (no ocupan espacio permanente)

? **UI Profesional**: Diseño moderno con cards, iconos y colores

? **Multiplataforma**: Funciona en Android, iOS, Windows, macOS

? **Compartir Nativo**: Usa el sheet de compartir del sistema operativo

? **Sin Dependencias Adicionales**: Usa APIs nativas de .NET MAUI

? **Error Handling**: Manejo robusto de errores en descarga/visualización

## Consideraciones Técnicas

### Almacenamiento
- Las facturas se almacenan en memoria (`ObservableCollection`)
- Los PDFs se guardan temporalmente en `FileSystem.CacheDirectory`
- El sistema limpia el caché automáticamente cuando es necesario

### Seguridad
- El hash de la factura actúa como token de seguridad
- Solo quien tiene el hash puede descargar el PDF
- Bearer token protege la API de PDFs

### Performance
- Descarga lazy de PDFs (solo cuando el usuario lo solicita)
- CollectionView con virtualización para listas largas
- Imágenes y datos en memoria para acceso rápido

## Extensiones Futuras

### Posibles Mejoras

1. **Persistencia en Base de Datos Local**
   - Guardar historial en SQLite
   - Sincronización con servidor

2. **Búsqueda y Filtros**
  - Buscar por número de factura
   - Filtrar por cliente
   - Filtrar por rango de fechas
   - Filtrar por monto

3. **Exportar Reporte**
   - Generar Excel con historial
   - PDF consolidado de múltiples facturas

4. **Notificaciones**
   - Notificar cuando se emite una factura
   - Alertas de facturas pendientes

5. **Sincronización en Nube**
   - Backup automático en servidor
   - Acceso desde múltiples dispositivos

6. **Analytics**
   - Dashboard de ventas
   - Gráficos de facturación
   - Reportes por período

## Troubleshooting

### Problema: PDF no se descarga

**Síntoma**: Error "No se pudo descargar el PDF"

**Soluciones**:
1. Verificar conectividad a internet
2. Confirmar que el hash de la factura es válido
3. Verificar que la API de PDFs esté operativa
4. Revisar que los headers de autenticación sean correctos

### Problema: No se puede abrir el PDF

**Síntoma**: PDF descargado pero no abre

**Soluciones**:
1. Verificar que el dispositivo tiene visor de PDF
2. Instalar Adobe Reader u otro visor
3. Verificar permisos de la app para archivos

### Problema: Historial vacío

**Síntoma**: No aparecen facturas en el historial

**Soluciones**:
1. Verificar que se hayan generado facturas
2. Revisar que `InvoiceHistory` esté poblado
3. Confirmar que el binding de datos funciona

## Testing

### Datos de Prueba

**Factura de Ejemplo**:
```
Hash: eda03fac259eec231663c7032b76c11008e7468e774e417af8a086b9e8efffb2788e7f5f46aab86ca7b691524b8652d2
Número: FE-20250128153045
Cliente: ESTACION XYZ S.A.S
NIT: 900123456-1
Total: $120,000.00
Método Pago: Efectivo
```

### Casos de Prueba

1. ? Generar factura y verificar que aparece en historial
2. ? Descargar PDF de factura
3. ? Abrir PDF en visor
4. ? Compartir PDF por WhatsApp
5. ? Compartir PDF por Email
6. ? Verificar ordenamiento cronológico
7. ? Verificar empty state cuando no hay facturas
8. ? Verificar loading durante descarga

## Soporte

Para soporte técnico o consultas:

- **Equipo**: Poliedro Software
- **Repositorio**: https://github.com/PoliedroSoftware/app-eds
- **Branch**: feature/poliedro
- **Documentación**: /APP.Eds/Docs/

---

**Fecha de Implementación**: Enero 28, 2025
**Versión**: 2.0.0
**Estado**: ? Implementado y Probado
