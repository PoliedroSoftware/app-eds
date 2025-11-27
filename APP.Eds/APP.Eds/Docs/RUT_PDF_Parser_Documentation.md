# RUT PDF Parser - Documentación de Uso

## Descripción General

Se ha implementado una funcionalidad para analizar documentos RUT (Registro Único Tributario) en formato PDF y extraer automáticamente la información del tercero para llenar el formulario de registro de isleros.

## Componentes Creados

### 1. **Modelo de Datos** - `RutParseModel.cs`
   - **Ubicación**: `APP.Eds\Models\Rut\RutParseModel.cs`
   - **Descripción**: Define las estructuras de datos para mapear la respuesta del API de análisis de RUT
   - **Clases principales**:
     - `RutParseResponse`: Respuesta principal con todos los datos del RUT
     - `RutFullName`: Información del nombre completo
     - `RutEconomicActivity`: Actividades económicas
     - `RutResponsibility`: Responsabilidades fiscales

### 2. **Servicio de Análisis** - `RutParserService.cs`
   - **Ubicación**: `APP.Eds\Services\Rut\RutParserService.cs`
   - **Métodos principales**:
     - `ParseRutFromPdfAsync(string filePath)`: Analiza un PDF desde una ruta local
     - `ParseRutFromBytesAsync(byte[] fileBytes, string fileName)`: Analiza un PDF desde bytes

### 3. **Integración en IslanderService**
   - **Archivo modificado**: `APP.Eds\Services\Islander\IslanderService.cs`
   - **Comando agregado**: `ParseRutFromPdfCommand`
   - **Método principal**: `ParseRutFromPdfAsync()`

## Cómo Usar la Funcionalidad

### Integración en XAML

Para agregar el botón de carga de RUT en tu vista XAML, agrega el siguiente código dentro del formulario de registro de isleros:

```xml
<!-- Botón para cargar RUT desde PDF -->
<Button 
    Text="?? Cargar desde RUT"
    Command="{Binding ParseRutFromPdfCommand}"
    BackgroundColor="#4CAF50"
    TextColor="White"
    FontAttributes="Bold"
    CornerRadius="8"
    Margin="0,10,0,10"
    HeightRequest="50">
    <Button.Shadow>
        <Shadow Brush="Black" Offset="2,2" Radius="4" Opacity="0.3"/>
    </Button.Shadow>
</Button>

<!-- Texto de ayuda -->
<Label 
    Text="?? Seleccione un archivo RUT en PDF para llenar automáticamente los datos del islero"
    FontSize="12"
    TextColor="Gray"
    HorizontalOptions="Center"
    Margin="0,0,0,10"/>
```

### Ejemplo de uso ubicando el botón después del selector de EDS:

```xml
<VerticalStackLayout Spacing="15" Padding="20">
    
    <!-- Selector de EDS existente -->
    <Border StrokeShape="RoundRectangle 8" Stroke="#E0E0E0" StrokeThickness="1">
        <Picker 
            x:Name="EdsPicker"
            Title="Seleccionar EDS"
            ItemsSource="{Binding EdsList}"
            ItemDisplayBinding="{Binding Name}"
            SelectedItem="{Binding SelectedEds}"/>
    </Border>

    <!-- ? NUEVO: Botón para cargar RUT -->
    <Button 
        Text="?? Cargar datos desde RUT"
        Command="{Binding ParseRutFromPdfCommand}"
        BackgroundColor="#4CAF50"
        TextColor="White"
        FontAttributes="Bold"
        CornerRadius="8"
        HeightRequest="50"/>

    <Label 
        Text="?? Puede cargar los datos automáticamente desde un documento RUT en PDF"
        FontSize="12"
        TextColor="Gray"
        HorizontalOptions="Center"/>

    <!-- Campos del formulario (se llenan automáticamente) -->
    <Entry 
        Placeholder="Nombre completo"
        Text="{Binding Name}"/>
    
    <Entry 
        Placeholder="Primer nombre"
        Text="{Binding FirstName}"/>
    
    <Entry 
        Placeholder="Apellidos"
        Text="{Binding LastName}"/>
    
    <Entry 
        Placeholder="Email"
        Text="{Binding Email}"
        Keyboard="Email"/>
    
    <Entry 
        Placeholder="Contraseña"
        Text="{Binding Password}"
        IsPassword="True"/>

    <!-- Botón de guardar existente -->
    <Button 
        Text="Registrar Islero"
        Command="{Binding SaveIslanderDataCommand}"/>
        
</VerticalStackLayout>
```

## Flujo de Uso

1. **El usuario hace clic en "Cargar desde RUT"**
   - Se abre un selector de archivos filtrado para PDF
   - Solo acepta archivos `.pdf`

2. **Selecciona el archivo RUT**
   - El sistema muestra un mensaje de procesamiento
   - Se envía el PDF al API de extracción

3. **API analiza el documento**
   - URL: `https://mg5tj7bmve.execute-api.us-east-2.amazonaws.com/eds/api/v1/rut/parse`
   - Header: `X-Environment: extractor-pdf`
   - Authorization: Bearer token del usuario

4. **Datos extraídos se mapean al formulario**
   - **Nombre completo** ? `Name`
   - **Primer nombre** ? `FirstName`
   - **Apellidos** ? `LastName` (incluye segundo apellido si existe)
   - **Email** ? `Email`
   - **Contraseña sugerida** ? `Password` (basada en el documento, el usuario debe cambiarla)

5. **Usuario revisa y completa los datos faltantes**
   - Seleccionar EDS (no viene en el RUT)
   - Seleccionar rol (no viene en el RUT)
   - Cambiar contraseña si lo desea
   - Agregar número de teléfono si lo desea

6. **Guarda el islero con el botón "Registrar"**

## Ejemplo de Respuesta del API

```json
{
    "source": "DIAN-RUT",
    "nit": "10916585513",
    "dv": "3",
    "documentType": "Cédula de Ciudadanía",
    "documentNumber": "1091658551",
    "fullName": {
        "firstName": "EDUAR",
        "middleNames": "LEONARDO",
        "lastName": "SANCHEZ",
        "secondLastName": "PACHECO",
        "display": "SANCHEZ PACHECO EDUAR LEONARDO"
    },
    "email": "leosanchez_19@hotmail.com",
    "address": "CR 16 8 109 BRR SAN CAYETANO",
    "city": "Ocaña",
    "department": "Norte de Santander"
}
```

## Mensajes al Usuario

La funcionalidad muestra mensajes informativos en cada etapa:

### ? Éxito
```
? RUT Procesado Exitosamente

Se ha extraído la siguiente información:

• NIT: 10916585513
• Documento: 1091658551
• Nombre: SANCHEZ PACHECO EDUAR LEONARDO
• Email: leosanchez_19@hotmail.com
• Dirección: CR 16 8 109 BRR SAN CAYETANO
• Ciudad: Ocaña

Los datos han sido cargados en el formulario.
```

### ?? Error
```
Error al procesar el archivo RUT:

No se pudo extraer la información del RUT.

Verifique que el archivo sea un RUT válido de la DIAN.
```

## Notas Importantes

1. **Contraseña generada**: Se genera automáticamente como `{documentNumber}@Rut`, pero el usuario debería cambiarla
2. **Campos no incluidos en RUT**: 
   - Número de teléfono (no está en el RUT)
   - EDS asignada (debe seleccionarse manualmente)
   - Rol (debe seleccionarse manualmente)
3. **Formato de archivo**: Solo acepta PDFs oficiales del RUT de la DIAN
4. **Conexión requerida**: Necesita conexión a internet para consultar el API de extracción

## Extensión para Otros Terceros

Esta implementación puede extenderse para crear terceros (clientes, proveedores, etc.) siguiendo el mismo patrón:

1. Crear el comando en el servicio correspondiente
2. Usar `RutParserService` para analizar el PDF
3. Mapear los datos a los campos del modelo específico
4. Agregar el botón en la vista XAML

## Seguridad

- El token de autenticación se toma del usuario actual (`TokenHelper.LoadToken()`)
- Todas las comunicaciones con el API usan HTTPS
- Se valida el tipo de archivo antes de procesar

## Soporte

Para más información sobre el API de extracción de RUT, contacte al equipo de backend o consulte la documentación del API en:
`https://mg5tj7bmve.execute-api.us-east-2.amazonaws.com/eds/api/v1/rut/parse`
