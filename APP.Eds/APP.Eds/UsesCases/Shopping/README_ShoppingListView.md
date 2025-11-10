# Listado de Compras - Shopping List View

## Descripción
Vista para visualizar el historial completo de compras registradas en el sistema, incluyendo detalles de productos, cantidades y precios.

## Ubicación
- **Menú**: Compras y productos ? Listado de Compras
- **Ruta**: `APP.Eds/UsesCases/Shopping/ShoppingListView.xaml`

## Funcionalidades

### 1. **Visualización de Compras**
- Lista completa de todas las compras registradas
- Información principal:
  - Número de factura
  - Fecha de compra
  - ID del proveedor
  - ID de categoría
  - Monto total
  - Cantidad de productos

### 2. **Búsqueda**
- Búsqueda por número de factura
- Filtrado en tiempo real
- Case-insensitive

### 3. **Estadísticas**
Muestra en la parte superior:
- **Total de Compras**: Cantidad total de compras registradas
- **Monto Total**: Suma de todos los montos de compras

### 4. **Detalles de Compra**
Al tocar una compra se muestra:
- Información general (fecha, proveedor, categoría, monto)
- **Lista de productos** con:
  - ID del producto
  - Cantidad en galones
  - Precio de compra
  - Precio de venta
  - Subtotal por producto
- **Resumen**:
  - Total de galones
  - Total de valor de compra

### 5. **Actualización**
- Botón para refrescar el listado
- Carga automática al abrir la vista

## Estructura de Datos

### ShoppingResponse
```csharp
{
    "idShopping": 50,
    "invoice": "Factura001",
    "date": "2025-09-02T00:00:00",
    "idProvider": 1,
    "idCategory": 1,
    "amount": 5000,
    "shoppingProducts": [
        {
            "idShoppingProduct": 123,
            "idShopping": 50,
            "idProduct": 21,
            "quantity": 5,
            "purchasePrice": 1000,
            "sellPrice": 2000,
            "idCompartment": 17
        }
    ]
}
```

## Endpoint API
```
GET https://mg5tj7bmve.execute-api.us-east-2.amazonaws.com/eds/api/v1/shopping
Authorization: Bearer {token}
```

## Archivos Relacionados

### Modelos
- `APP.Eds/Models/ShoppingProduct/ShoppingResponse.cs` - Modelo principal
- `APP.Eds/Models/ShoppingProduct/ShoppingProductDetail.cs` - Detalle de productos

### Servicios
- `APP.Eds/Services/Shopping/ShoppingService.cs` - Método `GetAllShoppingAsync()`

### Vistas
- `APP.Eds/UsesCases/Shopping/ShoppingListView.xaml` - Vista XAML
- `APP.Eds/UsesCases/Shopping/ShoppingListView.xaml.cs` - Code-behind

### Navegación
- `APP.Eds/Services/Navigation/MainService.cs` - Registro en menú

## Características Técnicas

### UI/UX
- ? Diseño moderno con Material Design
- ? Cards con sombras y bordes redondeados
- ? Colores semánticos (verde para montos, azul para estadísticas)
- ? Estados visuales (vacío, cargando, con datos)
- ? Indicador de cantidad de productos por compra

### Funcionalidad
- ? Binding bidireccional con MVVM
- ? Filtrado en tiempo real
- ? Estado vacío informativo
- ? Loading overlay durante carga
- ? Gestión de errores con DisplayAlert

### Performance
- ? ObservableCollection para updates eficientes
- ? Carga asíncrona de datos
- ? Ordenamiento descendente por fecha

## Mejoras Futuras
- [ ] Exportar listado a PDF/Excel
- [ ] Filtros avanzados (por fecha, proveedor, categoría)
- [ ] Paginación para grandes volúmenes de datos
- [ ] Gráficos de estadísticas
- [ ] Vista de detalles en página dedicada
- [ ] Obtener nombres de productos desde API
- [ ] Obtener nombres de proveedores desde API
- [ ] Obtener nombres de categorías desde API

## Notas de Desarrollo
- El servicio `ShoppingService` es un Singleton
- Los datos se cargan automáticamente al aparecer la vista
- El modelo `ShoppingResponse` fue extendido para incluir `ShoppingProducts`
- Se agregó la clase `ShoppingProductDetail` para deserializar productos
