# ?? Implementación de Búsqueda Unificada de Clientes

## ?? **Resumen**

He actualizado el modelo de `ClientLegalModel` para que coincida exactamente con la estructura del API. El picker ahora muestra correctamente:

**Formato**: `"Nombre de la Empresa - TipoDoc Número-DV"`

### ? **Cambios Completados:**

1. **Modelo actualizado** (`ClientLegalModel.cs`):
   - ? `companyName` ? Nombre de la empresa
   - ? `documentTypeId` ? Mapeo a tipos (1=NIT, 2=CC, 3=CE, 4=PAS)
   - ? `verificationDigit` ? Dígito de verificación para NITs
   - ? `DisplayText` ? Formato completo para el picker

2. **Servicio actualizado** (`PointOfSaleService.cs`):
   - ? Método para cargar clientes legales
   - ? Logs mejorados para depuración

## ?? **Próximos Pasos para Búsqueda Unificada**

Para implementar un buscador que busque tanto en clientes legales como naturales:

### 1. **Estructura del API**

Según tus ejemplos, tienes 2 endpoints:

```bash
# Clientes Legales (Empresas)
GET /api/v1/client/legal
Response: { data: [ { id, companyName, documentTypeId, documentNumber, verificationDigit, ... } ] }

# Clientes Naturales (Personas)
GET /api/v1/client/natural  
Response: { data: [ { id, name, middleName, lastName, secondSurname, documentTypeId, documentNumber, ... } ] }
```

### 2. **Implementación Recomendada**

#### **Opción A: Búsqueda en el Servicio** (Recomendado)

Agregar un método en `PointOfSaleService`:

```csharp
public async Task<ClientLegalModel> SearchClientByDocumentAsync(string documentNumber)
{
    if (string.IsNullOrWhiteSpace(documentNumber))
        return null;

    try
    {
  using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("X-Environment", "clients");
    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

        // 1?? Buscar en clientes legales
      try
        {
     string legalUrl = $"{Configuration.BaseUrl}/api/v1/client/legal?documentNumber={Uri.EscapeDataString(documentNumber)}";
     var legalResponse = await httpClient.GetStringAsync(legalUrl);
  var legalResult = JsonSerializer.Deserialize<ClientLegalResponse>(legalResponse, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

  if (legalResult?.Data != null && legalResult.Data.Any())
          {
   return legalResult.Data.First(); // Cliente legal encontrado
  }
        }
 catch (HttpRequestException) { /* No encontrado en legal, continuar */ }

        // 2?? Buscar en clientes naturales
        try
     {
        string naturalUrl = $"{Configuration.BaseUrl}/api/v1/client/natural?documentNumber={Uri.EscapeDataString(documentNumber)}";
  var naturalResponse = await httpClient.GetStringAsync(naturalUrl);
        var naturalResult = JsonSerializer.Deserialize<ClientNaturalResponse>(naturalResponse, 
    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (naturalResult?.Data != null && naturalResult.Data.Any())
  {
                var natural = naturalResult.Data.First();
     
         // Convertir a ClientLegalModel para mantener compatibilidad
           return new ClientLegalModel
 {
               Id = natural.Id,
        Name = $"{natural.FirstName} {natural.MiddleName} {natural.LastName} {natural.SecondSurname}".Trim(),
           DocumentTypeId = natural.DocumentTypeId,
   DocumentNumber = natural.DocumentNumber,
        Email = natural.Email,
                 VerificationDigit = 0 // Los naturales no tienen dígito de verificación
              };
    }
        }
     catch (HttpRequestException) { /* No encontrado en natural */ }

        return null; // No encontrado en ninguno
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Error buscando cliente: {ex.Message}");
        return null;
  }
}
```

#### **Opción B: API con Endpoint Unificado** (Ideal)

Si tienes control del backend, crear un endpoint unificado:

```bash
GET /api/v1/client/search?documentNumber=123456789
Response: {
    "data": {
        "id": 2,
        "name": "Empresa XYZ" o "Juan Pérez González",
      "type": "Legal" o "Natural",
     "documentType": "NIT" o "CC",
        "documentNumber": "123456789",
        "verificationDigit": 9 (solo para NIT),
    ...
    }
}
```

### 3. **UI - Campo de Búsqueda**

En la vista (`PointOfSaleView.xaml.cs`), agregar un campo de búsqueda:

```csharp
// Dentro de CreateClientSelectorSection()

// Campo de búsqueda
var searchFrame = new Frame
{
    BackgroundColor = Color.FromArgb("#FEF3C7"),
    CornerRadius = 12,
    Padding = new Thickness(16)
};

var searchStack = new StackLayout { Spacing = 8 };

var searchLabel = new Label
{
    Text = "?? Buscar por Número de Documento:",
    FontSize = 14,
  FontAttributes = FontAttributes.Bold
};

var searchInputGrid = new Grid
{
    ColumnDefinitions = new ColumnDefinitionCollection
    {
      new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
        new ColumnDefinition { Width = GridLength.Auto }
    },
    ColumnSpacing = 8
};

var searchEntry = new Entry
{
    Placeholder = "Ej: 123456789",
    FontSize = 16,
    BackgroundColor = Colors.White,
    HeightRequest = 50
};
searchEntry.SetBinding(Entry.TextProperty, "ClientSearchText");

var searchButton = new Button
{
    Text = "Buscar",
    FontSize = 14,
    BackgroundColor = Color.FromArgb("#F59E0B"),
    TextColor = Colors.White,
    CornerRadius = 10,
    WidthRequest = 100,
    HeightRequest = 50
};
searchButton.SetBinding(Button.CommandProperty, "SearchClientCommand");

searchInputGrid.Add(searchEntry, 0);
searchInputGrid.Add(searchButton, 1);

searchStack.Add(searchLabel);
searchStack.Add(searchInputGrid);
searchFrame.Content = searchStack;

stackLayout.Add(searchFrame); // Agregar antes del picker
```

### 4. **ViewModel - Comando de Búsqueda**

En `PointOfSaleViewModel.cs`:

```csharp
private string _clientSearchText;
public string ClientSearchText
{
    get => _clientSearchText;
    set => SetProperty(ref _clientSearchText, value);
}

public ICommand SearchClientCommand { get; }

// En el constructor
SearchClientCommand = new Command(async () => await SearchClient());

private async Task SearchClient()
{
  if (string.IsNullOrWhiteSpace(ClientSearchText))
    {
        await Application.Current.MainPage.DisplayAlert("Documento Requerido", 
        "Por favor ingrese un número de documento", "OK");
        return;
    }

    IsLoading = true;
    try
    {
        var foundClient = await _pointOfSaleService.SearchClientByDocumentAsync(ClientSearchText.Trim());

   if (foundClient != null)
        {
 SelectedClient = foundClient;
       IsClientSelectorVisible = false;
            ClientSearchText = string.Empty;

         await Application.Current.MainPage.DisplayAlert("Cliente Encontrado", 
       $"? Cliente encontrado:\n\n" +
        $"Nombre: {foundClient.Name}\n" +
           $"Documento: {foundClient.DocumentType} {foundClient.DocumentNumber}" +
      (foundClient.VerificationDigit > 0 ? $"-{foundClient.VerificationDigit}" : ""), 
  "OK");
        }
 else
        {
   await Application.Current.MainPage.DisplayAlert("Cliente No Encontrado", 
           $"? No se encontró ningún cliente con el documento:\n{ClientSearchText}\n\n" +
   $"Verifique el número e intente nuevamente.", 
           "OK");
   }
    }
    catch (Exception ex)
    {
        await Application.Current.MainPage.DisplayAlert("Error", 
    $"Error al buscar cliente:\n{ex.Message}", "OK");
    }
finally
    {
        IsLoading = false;
    }
}
```

## ?? **Mapeo de Tipos de Documento**

```csharp
public string DocumentType
{
    get
    {
      return DocumentTypeId switch
        {
   1 => "NIT",    // Empresa
      2 => "NIT País Extranjero", // Empresa extranjera
      3 => "CC",      // Cédula de Ciudadanía
      4 => "CE",          // Cédula de Extranjería
      5 => "PPT",       // Permiso Protección Temporal
            6 => "PAS",    // Pasaporte
            _ => "DOC"
        };
    }
}
```

## ?? **Flujo de Búsqueda**

```
Usuario ingresa "123456789"
  ?
  1. Buscar en /api/v1/client/legal?documentNumber=123456789
 ?
  ¿Encontrado? ? Sí ? Seleccionar cliente y mostrar
         ? No
  2. Buscar en /api/v1/client/natural?documentNumber=123456789
         ?
  ¿Encontrado? ? Sí ? Convertir a formato común y mostrar
         ? No
  Mostrar "Cliente no encontrado"
```

## ? **Ventajas de Este Enfoque**

1. **Búsqueda Unificada**: Un solo campo busca en ambos tipos
2. **Compatibilidad**: Mantiene el modelo existente
3. **Validación**: Verifica en ambos endpoints automáticamente
4. **UX Mejorada**: El usuario no necesita saber si es legal o natural

## ?? **Mejoras Futuras**

1. **Caché Local**: Guardar clientes buscados recientemente
2. **Búsqueda por Nombre**: Además de documento
3. **Autocompletado**: Sugerir mientras escribe
4. **Historial**: Mostrar últimos clientes consultados

## ?? **Ejemplo de Uso**

```
Usuario ? Ingresa "123456781" ? Botón "Buscar"
Sistema ? Busca en Legal ? Encuentra "asd - NIT 123456781-9"
Sistema ? Selecciona automáticamente el cliente
Sistema ? Muestra confirmación con detalles
```

---

**Estado Actual**: 
- ? Modelo `ClientLegalModel` corregido y funcionando
- ? Picker mostrando formato correcto "Nombre - TipoDoc Número-DV"
- ? Búsqueda unificada pendiente de implementación (código de ejemplo proporcionado)

