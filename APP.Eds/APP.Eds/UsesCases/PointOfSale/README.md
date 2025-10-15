# Punto de Venta - APP.Eds

## Descripción
Sistema de punto de venta sencillo y optimizado para pantalla táctil, diseñado para facilitar las transacciones de venta en establecimientos. Con un diseño moderno y profesional que mejora la experiencia del usuario.

## Características Principales

### ?? Gestión de Productos
- **Vista de productos en cuadrícula**: Los productos se muestran en una cuadrícula de 3 columnas optimizada para pantalla táctil
- **Diseño visual atractivo**: Tarjetas verdes con iconos distintivos y texto claro
- **Información del producto**: Nombre, precio de venta y stock disponible
- **Selección táctil**: Toque simple para agregar productos al carrito
- **Validación de stock**: Solo permite agregar productos con stock disponible
- **Bordes redondeados**: Design moderno con esquinas redondeadas de 16px

### ??? Carrito de Compras
- **Gestión de cantidades**: Botones + y - grandes y fáciles de presionar
- **Eliminar productos**: Botón X rojo intuitivo para remover productos
- **Cálculo automático**: Actualización automática de subtotales y totales
- **Validación de stock**: No permite exceder el stock disponible
- **Cards organizadas**: Cada item en una tarjeta gris claro con bordes suaves

### ?? Sistema de Facturación
- **Sección de totales destacada**: Card azul con texto blanco para mayor visibilidad
- **Cálculo de impuestos**: 16% de impuestos aplicados automáticamente
- **Subtotal y total**: Cálculo claro y visible con separadores visuales
- **Formato de moneda**: Visualización consistente en formato $XX.XX
- **Tipografía jerarquizada**: Tamaños de fuente diferenciados por importancia

### ?? Métodos de Pago
- **Botones de pago con iconos**: ?? Efectivo y ?? Tarjeta para mejor reconocimiento
- **Efectivo**: Con cálculo automático de cambio en color verde
- **Tarjeta**: Procesamiento directo sin manejo de efectivo
- **Validación**: Solo permite procesar si hay suficiente efectivo recibido
- **Sección organizada**: Card separada con campos claramente etiquetados

### ?? Diseño Visual Mejorado

#### Paleta de Colores Moderna
- **Verde (#10B981)**: Productos, confirmaciones y botones de procesar
- **Azul (#3B82F6)**: Controles de cantidad, totales y botones de tarjeta
- **Naranja (#F97316)**: Botón de limpiar carrito
- **Rojo (#EF4444)**: Botones de eliminar
- **Grises suaves**: Fondos y elementos secundarios
- **Blanco**: Cards y elementos principales

#### Tipografía y Espaciado
- **Jerarquía visual clara**: Títulos principales (22px), subtítulos (20px), texto normal (15-16px)
- **Fuentes bold**: Para elementos importantes como precios y totales
- **Espaciado consistente**: 16px entre secciones, 12px entre elementos
- **Márgenes generosos**: 20px padding en cards principales

#### Elementos Visuales
- **Bordes redondeados**: 16px para cards principales, 12px para elementos internos
- **Sombras sutiles**: Efectos de profundidad en cards importantes
- **Iconos emoji**: ??? para productos, ???? para métodos de pago
- **Feedback visual**: Diferentes colores para diferentes acciones

## Estructura del Código

### Modelos
- **`SaleItemModel`**: Representa un producto en el carrito con cantidad y precios
- **`SaleModel`**: Representa una venta completa con items, impuestos y método de pago
- **`PaymentMethod`**: Enumeración de métodos de pago (Efectivo, Tarjeta, Mixto)
- **`SaleStatus`**: Estados de la venta (Pendiente, Completada, Cancelada)

### Servicios
- **`IPointOfSaleService`**: Interfaz del servicio de punto de venta
- **`PointOfSaleService`**: Implementación con productos simulados y procesamiento de ventas

### Vista y ViewModel
- **`PointOfSaleViewModel`**: Lógica de presentación con comandos y validaciones
- **`PointOfSaleView`**: Interfaz de usuario creada completamente en C# con estilos modernos

### Estilos y Recursos
- **`Colors.xaml`**: Paleta de colores específica para POS
- **`Styles.xaml`**: Estilos específicos para componentes del punto de venta
- **Estilos CSS-like**: Naming convention similar a TailwindCSS para consistencia

## Funcionalidades Implementadas

### ? Gestión de Productos
- [x] Cargar productos disponibles
- [x] Mostrar información del producto (nombre, precio, stock)
- [x] Agregar productos al carrito con validación de stock
- [x] Design atractivo con cards verdes y iconos

### ? Carrito de Compras
- [x] Agregar productos al carrito
- [x] Modificar cantidades (aumentar/disminuir)
- [x] Eliminar productos del carrito
- [x] Limpiar carrito completo
- [x] Cálculo automático de totales
- [x] Cards organizadas por item

### ? Procesamiento de Pagos
- [x] Selección de método de pago con iconos
- [x] Cálculo de impuestos (16%)
- [x] Manejo de efectivo con cálculo de cambio
- [x] Validación de transacciones
- [x] Procesamiento de ventas
- [x] Sección de pago organizada

### ? Interfaz de Usuario
- [x] Diseño optimizado para pantalla táctil
- [x] Navegación desde el menú principal
- [x] Indicadores de carga
- [x] Mensajes de confirmación y error
- [x] Paleta de colores moderna y consistente
- [x] Tipografía jerárquica
- [x] Espaciado y padding optimizados

## Uso

### Acceso al Punto de Venta
1. Iniciar sesión como administrador
2. Seleccionar "Punto de Venta" desde el menú principal
3. La aplicación cargará los productos disponibles en cards verdes

### Realizar una Venta
1. **Seleccionar productos**: Tocar las cards verdes de productos para agregarlos al carrito
2. **Ajustar cantidades**: Usar los botones azules + y - para modificar cantidades
3. **Revisar total**: Verificar subtotal, impuestos y total en la sección azul
4. **Seleccionar método de pago**: Elegir entre ?? Efectivo o ?? Tarjeta
5. **Ingresar efectivo recibido** (solo para pagos en efectivo)
6. **Procesar pago**: Tocar "Procesar Pago" (botón verde) para completar la transacción

### Gestión del Carrito
- **Agregar producto**: Tocar la card verde del producto
- **Aumentar cantidad**: Tocar el botón azul "?"
- **Disminuir cantidad**: Tocar el botón azul "?"
- **Eliminar producto**: Tocar el botón rojo "?"
- **Limpiar carrito**: Tocar "Limpiar Carrito" (botón naranja)

## Configuración

### Productos Simulados
El sistema incluye productos de ejemplo con precios actualizados:
- Coca Cola 500ml - $2.50
- Agua 1L - $1.00
- Chips - $1.50
- Chocolate - $3.00
- Sandwich - $5.00
- Café - $2.00
- Jugo Naranja - $2.25
- Galletas - $1.75

### Impuestos
- Tasa de impuestos: 16% (configurable en `PointOfSaleViewModel`)

### Colores del Sistema
```xml
<!-- Colores principales del POS -->
<Color x:Key="POSProductGreen">#10B981</Color>
<Color x:Key="POSBlue">#3B82F6</Color>
<Color x:Key="POSOrange">#F97316</Color>
<Color x:Key="POSRed">#EF4444</Color>
<Color x:Key="POSBackground">#F8FAFC</Color>
```

## Integración

### Inyección de Dependencias
El servicio está registrado en `MauiProgram.cs`:
```csharp
builder.Services.AddSingleton<IPointOfSaleService, PointOfSaleService>();
```

### Navegación
Agregado al menú principal en `MainService.cs` como navegación directa para acceso rápido.

### Estilos
- Estilos específicos en `Styles.xaml` con prefijo `POS`
- Colores específicos en `Colors.xaml`
- Sistema de design consistente en toda la aplicación

## Futuras Mejoras

### ?? Posibles Extensiones
- [ ] Integración con base de datos real
- [ ] Códigos de barras/QR
- [ ] Múltiples métodos de pago en una transacción
- [ ] Descuentos y promociones
- [ ] Reportes de ventas con gráficos
- [ ] Sincronización en tiempo real del inventario
- [ ] Impresión de recibos
- [ ] Gestión de clientes
- [ ] Programa de puntos/lealtad
- [ ] Modo oscuro para el POS

### ?? Mejoras Técnicas
- [ ] Persistencia de datos with SQLite
- [ ] Sincronización offline/online
- [ ] Optimización de rendimiento
- [ ] Pruebas unitarias
- [ ] Logging y monitoreo
- [ ] Configuración avanzada
- [ ] Animaciones de transición
- [ ] Haptic feedback

### ?? Mejoras Visuales
- [ ] Animaciones entre estados
- [ ] Transiciones suaves en cards
- [ ] Loading skeletons
- [ ] Micro-interacciones
- [ ] Theme switcher
- [ ] Personalización de colores por negocio

## Notas Técnicas

### Arquitectura
- **Patrón MVVM**: Separación clara entre vista, lógica de presentación y datos
- **Inyección de dependencias**: Servicios desacoplados y testeable
- **Data Binding**: Actualización automática de la interfaz
- **Comandos**: Acciones de usuario manejadas de manera reactiva

### Rendimiento
- **Collections observables**: Actualización eficiente de listas
- **Binding optimizado**: Mínimo uso de recursos
- **UI creada en C#**: Evita problemas de parsing XAML y mejora rendimiento
- **Lazy loading**: Carga eficiente de elementos visuales

### Accesibilidad
- **Botones grandes**: Mínimo 44px para fácil toque
- **Contraste alto**: Colores que cumplen WCAG guidelines
- **Labels descriptivos**: Para lectores de pantalla
- **Feedback visual**: Estados claros para todas las acciones

Este punto de venta proporciona una base sólida y extensible para operaciones de venta en el sistema APP.Eds, con un diseño moderno y profesional que mejora significativamente la experiencia del usuario.