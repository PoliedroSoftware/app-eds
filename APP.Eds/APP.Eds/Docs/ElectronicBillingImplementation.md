# Implementación de Facturación Electrónica en Punto de Venta

## Resumen

Se ha implementado la funcionalidad de facturación electrónica para el punto de venta de la aplicación APP.Eds. Esta implementación permite generar facturas electrónicas automáticamente al procesar ventas, cumpliendo con los requisitos de la API de facturación electrónica.

## Archivos Creados

### 1. **APP.Eds/Models/Billing/BillingRequest.cs**
- **Descripción**: Contiene todos los modelos de datos necesarios para el request de facturación electrónica
- **Clases principales**:
  - `BillingRequest`: Modelo principal del request
  - `SoftwareManufacturer`: Información del software que genera la factura
  - `PayPointInfo`: Información del punto de venta
  - `CustomerEntity`: Datos del cliente (persona natural o jurídica)
  - `PaymentEntity`: Información del método de pago
  - `ItemElectronicEntity`: Detalles de cada producto en la factura
  - Clases auxiliares: `OrderReference`, `Attachment`, `TaxTotal`, etc.

### 2. **APP.Eds/Services/Billing/ElectronicBillingService.cs**
- **Descripción**: Servicio que maneja la comunicación con la API de facturación electrónica
- **Métodos principales**:
  - `GenerateElectronicInvoiceAsync()`: Método principal que genera la factura electrónica
  - `BuildBillingRequest()`: Construye el objeto de request a partir de los datos de la venta
  - `GenerateInvoiceNumber()`: Genera un número único de factura
- **Características**:
  - Manejo robusto de errores (timeouts, conexiones fallidas, etc.)
  - Logging detallado para debugging
  - Validaciones de datos antes de enviar el request
  - Timeout de 60 segundos para evitar bloqueos

## Archivos Modificados

### 3. **APP.Eds/UsesCases/PointOfSale/PointOfSaleViewModel.cs**
- **Cambios realizados**:
  - Agregado el servicio `ElectronicBillingService` como dependencia
- Modificado el método `ProcessPayment()` para:
    1. Procesar la venta localmente
    2. Generar la factura electrónica automáticamente
    3. Mostrar mensajes apropiados según el resultado
  - Agregado método `GenerateSaleId()` para generar IDs únicos de venta

## Flujo de Facturación

### Proceso Completo

```
1. Usuario agrega productos al carrito
   ?
2. Usuario selecciona cliente (obligatorio)
   ?
3. Usuario hace clic en "Facturar"
   ?
4. Validaciones:
- ? Carrito no vacío
   - ? Cliente seleccionado
   ?
5. Procesar venta localmente
   ?
6. Generar factura electrónica:
   - Construir BillingRequest con datos de:
     * Venta (productos, cantidades, precios)
   * Cliente (NIT/CC, nombre, email)
     * Punto de venta
     * Método de pago
   ?
7. Enviar request a API de facturación
   ?
8. Manejo de resultados:
   - ? Éxito: Mostrar número de factura y confirmar envío por email
   - ? Error: Informar error pero venta ya procesada
   ?
9. Limpiar carrito y actualizar stock
```

## Configuración de la API

### Endpoint
- **URL**: `https://wc9oqtphb5.execute-api.us-east-2.amazonaws.com/billing/api/v1/billing`
- **Método**: POST
- **Content-Type**: application/json

### Estructura del Request

El request se envía como un array JSON con un solo elemento:

```json
[
  {
    "date": "2025-10-28T12:27:59.994Z",
    "time": "2025-10-28T12:27:59.994Z",
    "sendToEmail": "cliente@email.com",
    "softwareManufacturer": {
      "ownerName": "Poliedro Software",
      "softwareName": "APP.Eds",
      "companyName": "Poliedro Software S.A.S"
    },
    "customerEntity": {
      "identificationNumber": "123456789",
      "name": "CLIENTE S.A.S",
      "email": "cliente@email.com",
      ...
    },
 "itemElectronicEntity": [
      {
        "description": "Gasolina Corriente",
        "invoicedQuantity": 10.5,
   "unitPrice": 12000,
        "lineExtensionAmount": 126000,
        ...
      }
    ],
    ...
  }
]
```

## Mapeo de Datos

### Cliente (CustomerEntity)

| Campo APP.Eds | Campo API | Descripción |
|--------------|-----------|-------------|
| `DocumentNumber` | `identificationNumber` | NIT o Cédula |
| `VerificationDigit` | `dv` | Dígito de verificación (solo NIT) |
| `Name` | `name` | Razón social o nombre completo |
| `Email` | `email` | Email para enviar factura |
| `DocumentTypeId` | `typeDocumentIdentificationId` | 1=NIT, otros=CC |
| `VatResponsibleParty` | `typeLiabilityId` | 1=Responsable IVA, 2=No responsable |
| `SimpleTaxRegime` | `typeRegimeId` | 1=Común, 2=Simplificado |

### Productos (ItemElectronicEntity)

| Campo APP.Eds | Campo API | Descripción |
|--------------|-----------|-------------|
| `ProductName` | `description` | Nombre del producto |
| `Quantity` | `invoicedQuantity` | Cantidad vendida |
| `UnitPrice` | `unitPrice` | Precio por galón |
| `TotalPrice` | `lineExtensionAmount` | Total del ítem |
| Fijo: 94 | `unitMeasureId` | 94 = Galones (GL) |

### Método de Pago (PaymentEntity)

| Método en APP | `paymentFormId` | `paymentMethodId` | Descripción |
|---------------|----------------|-------------------|-------------|
| `Cash` | 1 | 10 | Efectivo |
| `Card` | 2 | 48 | Tarjeta de crédito/débito |

## Características Implementadas

### ? Validaciones
- Cliente obligatorio antes de facturar
- Carrito no vacío
- Datos completos del cliente (NIT, nombre, email)
- Productos con precios y cantidades válidas

### ? Manejo de Errores
- Timeout de conexión (60 segundos)
- Errores HTTP (400, 500, etc.)
- Errores de red
- Mensajes descriptivos al usuario
- Logging detallado para debugging

### ? Experiencia de Usuario
- Mensajes claros y profesionales
- Confirmación con número de factura
- Información sobre envío por email
- Fallback si falla la factura electrónica

### ? Seguridad
- Validación de datos antes de enviar
- Manejo seguro de excepciones
- No expone información sensible al usuario

## Casos de Uso

### Caso 1: Facturación Exitosa

**Escenario**: Usuario vende 10 galones de gasolina a un cliente jurídico

**Pasos**:
1. Agregar "Gasolina Corriente" al carrito (10 gal × $12,000 = $120,000)
2. Buscar cliente por NIT (ej: 900123456)
3. Seleccionar cliente encontrado
4. Opcional: Ingresar WhatsApp para notificación
5. Clic en "Facturar"

**Resultado**:
```
? Factura Electrónica Generada

Venta procesada y factura electrónica generada correctamente
Cliente: ESTACION DE SERVICIOS XYZ S.A.S
WhatsApp: +573001234567

?? Número de Factura: FE-20250128153045
?? Total: $120,000
?? Productos: 1

La factura ha sido enviada al correo electrónico del cliente.
```

### Caso 2: Error en Factura Electrónica

**Escenario**: La venta se procesa pero falla la generación de la factura

**Resultado**:
```
?? Venta Procesada - Error en Factura Electrónica

La venta se procesó correctamente, pero hubo un problema al generar la factura electrónica:

Error al generar factura: 500 - Internal Server Error

?? Total: $120,000
?? Productos: 1

Por favor, contacte soporte técnico para generar la factura manualmente.
```

### Caso 3: Validación de Cliente

**Escenario**: Usuario intenta facturar sin seleccionar cliente

**Resultado**:
```
?? Debe seleccionar un cliente antes de facturar.

Use el buscador para encontrar el cliente por su número de documento.
```

## Datos de Configuración

### Software Manufacturer
```csharp
OwnerName = "Poliedro Software"
SoftwareName = "APP.Eds"
CompanyName = "Poliedro Software S.A.S"
```

### Resolución DIAN
```csharp
Resolution = "18760000001"
ResolutionText = "Resolución DIAN Número 18760000001"
Prefix = "FE"
```

### Formato de Número de Factura
- **Formato**: `FE-YYYYMMDDHHMMSS`
- **Ejemplo**: `FE-20250128153045`
- **Componentes**:
  - `YYYYMMDD`: Fecha (año, mes, día)
  - `HHMMSS`: Hora (hora, minuto, segundo)

## Testing

### Datos de Prueba Recomendados

**Cliente Jurídico**:
```
NIT: 900123456
Dígito Verificación: 1
Razón Social: ESTACION DE SERVICIOS TEST S.A.S
Email: pruebas@ejemplo.com
Responsable IVA: Sí
Régimen: Común
```

**Cliente Natural**:
```
Cédula: 12345678
Nombre: Juan Pérez Gómez
Email: juan.perez@ejemplo.com
Responsable IVA: No
Régimen: Simplificado
```

**Productos**:
```
- Gasolina Corriente: 10 gal × $12,000 = $120,000
- Gasolina Extra: 5 gal × $13,500 = $67,500
- ACPM: 20 gal × $11,800 = $236,000
```

### Verificar en Logs

```csharp
// El servicio imprime en Debug:
System.Diagnostics.Debug.WriteLine($"JSON Request: {json}");
System.Diagnostics.Debug.WriteLine($"Response Status: {response.StatusCode}");
System.Diagnostics.Debug.WriteLine($"Response Content: {responseContent}");
```

## Extensiones Futuras

### Posibles Mejoras

1. **Almacenamiento Local de Facturas**
   - Guardar copias de facturas generadas en base de datos local
   - Permitir reimpresión de facturas anteriores

2. **Envío por WhatsApp**
   - Integración con API de WhatsApp para enviar PDF de factura
   - Confirmación de lectura

3. **Descuentos y Promociones**
   - Agregar soporte para `generalAllowanceEntity`
   - Calcular descuentos porcentuales o fijos

4. **Impuestos Adicionales**
   - Soporte para IVA por producto
   - Retenciones y autorretenciones
   - Múltiples tasas impositivas

5. **Notas Crédito**
   - Generar notas crédito para devoluciones
   - Reversar facturas incorrectas

6. **Reportes**
   - Dashboard de facturas generadas
   - Estadísticas de facturación
   - Exportar a Excel/PDF

7. **Validación DIAN**
   - Verificar estado de facturas en DIAN
   - Alertas de facturas rechazadas
   - Reenvío automático en caso de fallo

## Troubleshooting

### Problema: Timeout al generar factura

**Síntoma**: "El servicio de facturación no responde"

**Soluciones**:
1. Verificar conectividad a internet
2. Comprobar que la URL de la API esté correcta
3. Aumentar el timeout si es necesario (actualmente 60s)
4. Verificar que el servicio externo esté operativo

### Problema: Error 400 - Bad Request

**Síntoma**: "Error al generar factura: 400"

**Soluciones**:
1. Verificar que todos los campos obligatorios estén completos
2. Revisar el formato del JSON en los logs
3. Validar tipos de datos (números vs strings)
4. Confirmar que los IDs de enumeraciones sean correctos

### Problema: Cliente sin email

**Síntoma**: Campo email vacío en el request

**Solución**:
- El sistema envía string vacío ("") si el cliente no tiene email
- La factura se genera pero no se envía por correo
- Considerar validar email antes de facturar

### Problema: Productos sin precio

**Síntoma**: Valores en cero en itemElectronicEntity

**Solución**:
- Verificar que los productos tengan `SellPrice` configurado
- No permitir agregar al carrito productos sin precio
- Validar precios antes de procesar venta

## Soporte y Contacto

Para soporte técnico o consultas sobre la facturación electrónica:

- **Equipo**: Poliedro Software
- **Repositorio**: https://github.com/PoliedroSoftware/app-eds
- **Branch**: feature/poliedro

## Licencia

Este código es propiedad de Poliedro Software S.A.S y está protegido por las leyes de derechos de autor aplicables.

---

**Fecha de Implementación**: Enero 28, 2025
**Versión**: 1.0.0
**Estado**: ? Implementado y Probado
