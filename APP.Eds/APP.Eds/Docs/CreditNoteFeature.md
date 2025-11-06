# Funcionalidad de Notas Crédito

## Resumen

Se ha implementado la funcionalidad de **Notas Crédito** para anular facturas electrónicas directamente desde el historial de facturas. Una nota crédito es un documento que permite reversar o anular una factura previamente emitida.

## Ubicación del Botón

El botón de **Nota Crédito** se encuentra en el historial de facturas, junto con los botones de "Ver PDF" y "Compartir":

```
??????????????????????????????????????????
? SETT-202510099627          ? Emitida  ?
?                                        ?
? ?? TRANSPORTES DEL SUR LTDA           ?
?    800234567                           ?
?    09/10/2025 08:17                    ?
?                                        ?
? ?? Juan Carlos Rodriguez  ?? EDS Norte?
?                                        ?
? ?? Tarjeta            $227,604.16     ?
?                                        ?
? [??]  [??]  [??]  ? NUEVO BOTÓN       ?
??????????????????????????????????????????
```

## Características

### 1. **Botón Visual**
- **Icono**: ?? (emoji de documento)
- **Color**: Naranja (#FF9800)
- **Tamaño**: Igual que los otros botones de acción
- **Ubicación**: Tercera columna en la fila de acciones

### 2. **Comportamiento del Botón**
- **Habilitado**: Cuando la factura está en estado "Emitida"
- **Deshabilitado**: Cuando la factura ya está "Anulada"
  - Opacidad reducida (0.5)
  - No permite hacer clic

### 3. **Flujo de Generación de Nota Crédito**

#### Paso 1: Confirmación Inicial
Cuando el usuario hace clic en el botón ??, se muestra un diálogo de confirmación:

```
????????????????????????????????????????
? Generar Nota Crédito                 ?
????????????????????????????????????????
?                                      ?
? ¿Está seguro de generar una nota    ?
? crédito para la factura              ?
? SETT-202510099627?                   ?
?                                      ?
? Cliente: TRANSPORTES DEL SUR LTDA    ?
? Total: $227,604.16                   ?
?                                      ?
? Esta acción anulará la factura       ?
? seleccionada.                        ?
?                                      ?
? [Generar]            [Cancelar]      ?
????????????????????????????????????????
```

#### Paso 2: Solicitud de Motivo
Si el usuario confirma, se solicita el motivo de la nota crédito:

```
????????????????????????????????????????
? Motivo de la Nota Crédito            ?
????????????????????????????????????????
?                                      ?
? Por favor, indique el motivo de la   ?
? nota crédito:                        ?
?                                      ?
? ???????????????????????????????????? ?
? ? Ej: Error en la facturación,     ? ?
? ? Devolución de producto, etc.     ? ?
? ???????????????????????????????????? ?
?                                      ?
? [Aceptar]            [Cancelar]      ?
????????????????????????????????????????
```

**Validaciones**:
- El motivo es **obligatorio**
- Máximo 200 caracteres
- No puede estar vacío

#### Paso 3: Generación y Confirmación
Si todo es exitoso, se muestra un mensaje de confirmación:

```
????????????????????????????????????????
? ? Nota Crédito Generada             ?
????????????????????????????????????????
?                                      ?
? La nota crédito ha sido generada     ?
? exitosamente.                        ?
?                                      ?
? ?? Número: NC-20251009153045         ?
? ?? Monto: $227,604.16                ?
? ?? Fecha: 09/10/2025 15:30           ?
?                                      ?
? La factura SETT-202510099627 ha      ?
? sido anulada.                        ?
?                                      ?
? [OK]                                 ?
????????????????????????????????????????
```

### 4. **Cambios en la Factura Original**

Después de generar la nota crédito:

```
ANTES:
??????????????????????????????????????????
? SETT-202510099627          ? Emitida  ?
? [??]  [??]  [?? Habilitado]           ?
??????????????????????????????????????????

DESPUÉS:
??????????????????????????????????????????
? SETT-202510099627         ? Anulada   ?
? [??]  [??]  [?? Deshabilitado]        ?
??????????????????????????????????????????
```

## Arquitectura Técnica

### 1. **ViewModel**: `InvoiceHistoryViewModel.cs`

#### Comando Agregado
```csharp
public ICommand CreditNoteCommand { get; }

// Constructor
CreditNoteCommand = new Command<ElectronicInvoiceModel>(
    async (invoice) => await GenerateCreditNote(invoice)
);
```

#### Método Principal
```csharp
private async Task GenerateCreditNote(ElectronicInvoiceModel invoice)
{
    // 1. Validar factura
    // 2. Confirmar con usuario
    // 3. Solicitar motivo
    // 4. Llamar al servicio
    // 5. Mostrar resultado
    // 6. Recargar lista
}
```

### 2. **Servicio**: `ElectronicBillingService.cs`

#### Método Agregado
```csharp
public async Task<BillingResult> GenerateCreditNoteAsync(
    ElectronicInvoiceModel invoice, 
    string reason)
{
    // 1. Validar datos
    // 2. Construir request de nota crédito
    // 3. Enviar a API
    // 4. Procesar respuesta
    // 5. Actualizar estado de factura
    // 6. Retornar resultado
}
```

#### Request de Nota Crédito
```csharp
private object BuildCreditNoteRequest(
    ElectronicInvoiceModel invoice, 
    string reason)
{
    return new
    {
        date = now.ToString("yyyy-MM-dd"),
        time = now.ToString("HH:mm:ss.fffZ"),
        number = creditNoteNumber,
        prefix = "NC",
        referencedInvoiceNumber = invoice.InvoiceNumber,
        referencedInvoicePrefix = invoice.Prefix,
        referencedInvoiceCude = invoice.InvoiceHash,
        discrepancyResponseCode = "1", // 1 = Anulación
        discrepancyDescription = reason,
        // ... otros campos
    };
}
```

### 3. **Vista**: `InvoiceHistoryView.cs`

#### Botón de Nota Crédito
```csharp
var creditNoteButton = new Button
{
    Text = "??",
    BackgroundColor = Color.FromArgb("#FF9800"),
    // ... configuración
};

// Binding al comando
creditNoteButton.SetBinding(
    Button.CommandProperty, 
    new Binding(nameof(InvoiceHistoryViewModel.CreditNoteCommand), 
    source: _viewModel)
);

// Trigger para deshabilitar si está anulada
var trigger = new DataTrigger(typeof(Button))
{
    Binding = new Binding("Status"),
    Value = "Anulada"
};
trigger.Setters.Add(new Setter
{
    Property = Button.IsEnabledProperty,
    Value = false
});
```

### 4. **Modelo**: `ElectronicInvoiceModel.cs`

#### Actualización de Estado
```csharp
public string StatusIcon => Status switch
{
    "Emitida" => "?",
    "Anulada" => "?",  // ? NUEVO
    _ => "??"
};
```

### 5. **Resultado**: `BillingResult.cs`

#### Propiedad Agregada
```csharp
public class BillingResult
{
    // ...existing properties...
    public string CreditNoteNumber { get; set; } // ? NEW
}
```

## API Endpoint

### URL Esperada
```
POST {BillingApiUrl}/credit-note
```

**Nota**: La URL actual usa una transformación del endpoint de facturación:
```csharp
var creditNoteApiUrl = Configuration.BillingApiUrl
    .Replace("/billing", "/credit-note");
```

### Headers Requeridos
```http
Content-Type: application/json
X-Environment: production-billing
Authorization: Bearer {token}
```

### Request Body
```json
[{
    "date": "2025-10-09",
    "time": "15:30:45.000Z",
    "number": "20251009153045",
    "prefix": "NC",
    "referencedInvoiceNumber": "20251009099627",
    "referencedInvoicePrefix": "SETT",
    "referencedInvoiceCude": "8bee89869b7277b6...",
    "discrepancyResponseCode": "1",
    "discrepancyDescription": "Error en la facturación",
    "billingReference": {
        "number": "20251009099627",
        "uuid": "8bee89869b7277b6...",
        "issueDate": "2025-10-09"
    },
    "customerEntity": {
        "identificationNumber": "800234567",
        "name": "TRANSPORTES DEL SUR LTDA",
        "email": "facturacion@transportesdelsur.com"
    },
    "totalAmount": 227604.16,
    "taxAmount": 36264.16,
    "subtotal": 191340.00
}]
```

### Response Esperada
```json
{
    "code": 201,
    "success": true,
    "info": "Credit note emitted successfully",
    "data": {
        "cude": "abcd1234efgh5678...",
        "QRCode": "...",
        "techProviderDefaultFootNote": "..."
    }
}
```

## Validaciones Implementadas

### 1. **Validación de Factura**
```csharp
if (invoice == null)
    return new BillingResult { 
        Success = false, 
        Message = "La factura es requerida" 
    };
```

### 2. **Validación de CUDE**
```csharp
if (string.IsNullOrWhiteSpace(invoice.InvoiceHash))
    return new BillingResult { 
        Success = false, 
        Message = "La factura no tiene un CUDE válido" 
    };
```

### 3. **Validación de Motivo**
```csharp
if (string.IsNullOrWhiteSpace(reason))
    return new BillingResult { 
        Success = false, 
        Message = "El motivo es requerido" 
    };
```

### 4. **Validación de Confirmación**
```csharp
var confirmed = await Application.Current.MainPage.DisplayAlert(...);
if (!confirmed) return;
```

### 5. **Validación de Estado**
```csharp
// En la vista, el botón se deshabilita automáticamente
// si la factura ya está anulada
<DataTrigger Value="Anulada">
    <Setter Property="IsEnabled" Value="false" />
</DataTrigger>
```

## Logging y Debugging

### Información Registrada
```csharp
System.Diagnostics.Debug.WriteLine($"?? Generando nota crédito");
System.Diagnostics.Debug.WriteLine($"   - Factura: {invoice.FullInvoiceNumber}");
System.Diagnostics.Debug.WriteLine($"   - CUDE: {invoice.InvoiceHash}");
System.Diagnostics.Debug.WriteLine($"   - Motivo: {reason}");
System.Diagnostics.Debug.WriteLine($"   - Cliente: {invoice.ClientName}");
System.Diagnostics.Debug.WriteLine($"   - Monto: ${invoice.TotalAmount:N2}");
```

### Estados de la Operación
- ? `Nota crédito generada exitosamente`
- ? `Error al generar nota crédito`
- ?? `Sin CUFE de validación`
- ?? `Error de conexión`
- ?? `Timeout del servicio`

## Manejo de Errores

### 1. **Error de Conexión**
```
Error de conexión con el servicio de facturación: 
No se pudo conectar al servidor.
```

### 2. **Timeout**
```
El servicio de facturación no responde. 
Por favor, intente nuevamente.
```

### 3. **Error del API**
```
Error al generar nota crédito: 
400 - Bad Request
```

### 4. **Sin CUFE**
```
La nota crédito fue procesada pero no se pudo 
obtener el CUFE de validación.
```

## Casos de Uso

### Caso 1: Error en Facturación
```
Usuario: Cajero
Motivo: "Error al ingresar el monto, se digitó mal"
Acción: Anular factura y generar nueva correcta
```

### Caso 2: Devolución de Producto
```
Usuario: Supervisor
Motivo: "Cliente devolvió producto defectuoso"
Acción: Anular factura y procesar devolución
```

### Caso 3: Cliente Equivocado
```
Usuario: Cajero
Motivo: "Se facturó a cliente incorrecto"
Acción: Anular factura y emitir nueva al cliente correcto
```

## Pendientes / TODOs

### 1. **Endpoint Real del API**
```csharp
// TODO: Actualizar con la URL correcta del endpoint
var creditNoteApiUrl = Configuration.BillingApiUrl
    .Replace("/billing", "/credit-note");
```

### 2. **Estructura Completa del Request**
```csharp
// TODO: Ajustar según la estructura real requerida por la API
private object BuildCreditNoteRequest(...)
{
    // Completar con todos los campos requeridos
}
```

### 3. **Historial de Notas Crédito**
- Crear sección separada para ver notas crédito generadas
- Mostrar relación entre factura original y nota crédito

### 4. **PDF de Nota Crédito**
- Implementar descarga de PDF de la nota crédito
- Agregar opción para compartir nota crédito

### 5. **Reportes**
- Agregar estadísticas de facturas anuladas
- Reportes de notas crédito por período

## Pruebas Sugeridas

### Test 1: Flujo Completo Exitoso
```
1. Seleccionar factura emitida
2. Click en botón ??
3. Confirmar anulación
4. Ingresar motivo válido
5. Verificar nota crédito generada
6. Verificar estado cambiado a "Anulada"
7. Verificar botón deshabilitado
```

### Test 2: Cancelación en Confirmación
```
1. Seleccionar factura
2. Click en botón ??
3. Click en "Cancelar"
4. Verificar que no se generó nota crédito
```

### Test 3: Motivo Vacío
```
1. Seleccionar factura
2. Confirmar anulación
3. Dejar motivo vacío
4. Verificar mensaje de error
```

### Test 4: Factura Ya Anulada
```
1. Intentar generar nota crédito en factura anulada
2. Verificar que botón está deshabilitado
3. Verificar que no se puede hacer clic
```

### Test 5: Error de Red
```
1. Simular falta de conexión
2. Intentar generar nota crédito
3. Verificar mensaje de error apropiado
```

## Mejoras Futuras

### 1. **Historial de Cambios**
```
- Mostrar quién anulo la factura
- Fecha y hora de anulación
- Motivo de anulación
```

### 2. **Notificaciones**
```
- Email al cliente cuando se anula una factura
- Notificación al administrador
```

### 3. **Permisos**
```
- Solo supervisores pueden anular facturas
- Límite de tiempo para anular (ej: 24 horas)
```

### 4. **Auditoría**
```
- Log completo de todas las anulaciones
- Reportes de auditoría
```

---

**Fecha de Implementación**: Enero 2025
**Versión**: 1.0.0
**Estado**: ? Implementado y Listo para Pruebas
