# Control IoT - Válvula AWS

## ?? Descripción

Esta funcionalidad permite controlar remotamente válvulas mediante AWS IoT desde la aplicación móvil. El administrador puede abrir o cerrar válvulas con un simple toggle desde el menú principal.

## ?? Ubicación en el Menú

La opción **"Control IoT"** se encuentra en el menú principal de la aplicación, disponible solo para usuarios con rol de **Administrador**.

### Ruta de acceso:
```
Menú Principal ? Control IoT ???
```

### Posición en el menú:
- Después de "Power BI Dashboard"
- Antes de "Dispensadores y mangueras"

## ?? Características

### ? Funcionalidades Principales

1. **Toggle de Control**: Un switch principal para activar/desactivar la válvula
2. **Botones Directos**: Botones separados para "Abrir" y "Cerrar"
3. **Indicadores Visuales**:
   - Estado actual de la válvula (Abierta/Cerrada)
   - Color dinámico (Verde = Abierta, Rojo = Cerrada)
   - Valores de Output 1 y Output 2
   - Valores de Input 1 e Input 2

4. **Información de Conexión**:
   - Endpoint: AWS IoT Core
   - Topic: `psr/dev/psr-4g-at/cmd/`
   - Autenticación: Bearer Token (del usuario actual)

### ?? Interfaz de Usuario

#### Estado Abierto (Verde)
- **Toggle**: Activado
- **Output 1**: 1
- **Output 2**: 0
- **Input 1**: 0
- **Input 2**: 0
- **Color**: Verde (#4CAF50)
- **Mensaje**: ? Válvula Abierta

#### Estado Cerrado (Rojo)
- **Toggle**: Desactivado
- **Output 1**: 0
- **Output 2**: 1
- **Input 1**: 0
- **Input 2**: 0
- **Color**: Rojo (#F44336)
- **Mensaje**: ? Válvula Cerrada

## ?? Funcionamiento Técnico

### Request API IoT

**Válvula ABIERTA:**
```json
{
  "message": {
    "input1": "0",
    "input2": "0",
    "output1": "1",
    "output2": "0"
  },
  "topic": "psr/dev/psr-4g-at/cmd/"
}
```

**Válvula CERRADA:**
```json
{
  "message": {
    "input1": "0",
    "input2": "0",
    "output1": "0",
    "output2": "1"
  },
  "topic": "psr/dev/psr-4g-at/cmd/"
}
```

**Notas Importantes**: 
- **Input1** e **Input2** siempre se envían como `"0"`
- **Output1** y **Output2** alternan según el estado:
  - **Abierta**: `output1="1"`, `output2="0"`
  - **Cerrada**: `output1="0"`, `output2="1"`

**Función de cada parámetro:**
- `input1`: Siempre "0" (sin uso actual)
- `input2`: Siempre "0" (sin uso actual)
- `output1`: "1" = válvula abierta, "0" = válvula cerrada
- `output2`: "0" = válvula abierta, "1" = válvula cerrada (inverso de output1)

### Response API IoT

Respuesta exitosa:
```json
{
  "statusCode": 200,
  "success": true,
  "message": null,
  "data": {
    "success": true,
    "message": "IoT message published successfully",
    "topic": "psr/dev/psr-4g-at/cmd/"
  }
}
```

## ??? Seguridad

- **Autenticación**: Requiere Bearer Token válido
- **Autorización**: Solo usuarios con rol "Admin" pueden acceder
- **Timeout**: 30 segundos para operaciones IoT
- **Validación**: Verificación de respuesta del servidor antes de actualizar UI

## ?? Manejo de Errores

La aplicación maneja los siguientes tipos de errores:

1. **Error de Conexión**: No se puede conectar con AWS IoT
2. **Timeout**: La operación tarda más de 30 segundos
3. **Error de Autenticación**: Token inválido o expirado
4. **Error del Servidor**: Respuesta incorrecta de la API
5. **Error de Formato**: Respuesta JSON inválida

Todos los errores se muestran mediante alertas visuales con información detallada.

## ?? Uso

### Paso 1: Acceder al Control IoT
1. Abrir la aplicación
2. Navegar al menú principal
3. Tocar en **"Control IoT ???"**

### Paso 2: Controlar la Válvula

#### Opción A: Usar el Toggle
1. Tocar el switch para cambiar el estado
2. El color del indicador cambiará inmediatamente (Verde/Rojo)
3. Los valores de Output 1 y Output 2 se actualizarán automáticamente

#### Opción B: Usar Botones Directos
1. Tocar **"Abrir"** para abrir la válvula (color verde)
2. Tocar **"Cerrar"** para cerrar la válvula (color rojo)
3. El cambio es inmediato sin mensajes de confirmación

### Paso 3: Verificar Estado
- Observar el color del indicador principal (Verde = Abierta, Rojo = Cerrada)
- Revisar los valores de Output 1 y Output 2
- Leer el mensaje de estado en la parte superior

**Nota**: No se muestran mensajes de confirmación para agilizar la operación. Solo se muestran alertas en caso de error.

## ?? Troubleshooting

### Problema: No puedo ver la opción "Control IoT"
**Solución**: Verifica que tu usuario tenga rol de "Admin"

### Problema: Error de autenticación
**Solución**: Cierra sesión y vuelve a iniciar sesión

### Problema: La válvula no responde
**Soluciones**:
1. Verifica tu conexión a internet
2. Verifica que el topic IoT sea correcto
3. Contacta al soporte técnico

### Problema: Timeout en la operación
**Soluciones**:
1. Verifica tu conexión a internet
2. Intenta nuevamente después de unos segundos
3. Verifica el estado del servidor AWS IoT

## ?? Tabla de Estados Completa

| Estado | Output 1 | Output 2 | Input 1 | Input 2 | Color | Mensaje |
|--------|----------|----------|---------|---------|-------|---------|
| **Abierta** | 1 | 0 | 0 | 0 | ?? Verde | ? Válvula Abierta |
| **Cerrada** | 0 | 1 | 0 | 0 | ?? Rojo | ? Válvula Cerrada |

## ?? Estructura de Archivos

```
APP.Eds/
??? Models/IoT/
?   ??? IoTRequest.cs                 # Modelos de request/response IoT
??? Services/IoT/
?   ??? IoTService.cs                 # Servicio de comunicación IoT
??? Converters/
?   ??? IoTConverters.cs              # Convertidores para XAML
??? UsesCases/IoT/
?   ??? ValveControlPage.xaml         # Interfaz de usuario
?   ??? ValveControlPage.xaml.cs      # Lógica de la página
??? Services/Navigation/
    ??? MainService.cs                # Menú principal (actualizado)
```

## ?? Personalización

### Cambiar el Topic IoT
Editar en `IoTService.cs`:
```csharp
Topic = "tu/nuevo/topic/aqui"
```

### Cambiar la URL del Endpoint
Editar en `IoTService.cs`:
```csharp
private const string IoT_API_URL = "https://tu-url-aqui.com";
```

### Personalizar Colores
En `ValveControlPage.xaml`:
- Verde (Abierto): `#4CAF50`
- Rojo (Cerrado): `#F44336`
- Azul (Título): `#1E88E5`

## ?? Soporte

Para soporte técnico o reportar problemas:
- Email: soporte@poliedrosoftware.com
- GitHub Issues: https://github.com/PoliedroSoftware/app-eds/issues

## ?? Licencia

Este código es propiedad de Poliedro Software S.A.S.

---

**Última actualización**: 2024
**Versión**: 1.0.2 (Final)
