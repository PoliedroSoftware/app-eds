# Cambios a Aplicar en ProductService.cs

## 1. Agregar métodos después de `GetAllProductTypeData()` (línea ~658)

Después del método `GetAllProductTypeData()` y antes de `AddSampleData()`, agregar:

```csharp
private async void GetAllEdsData()
{
    if (string.IsNullOrEmpty(_authToken))
    {
        System.Diagnostics.Debug.WriteLine("No authentication token found for EDS data");
        return;
    }
    try
    {
        string url = $"{Configuration.BaseUrl}/api/v1/eds";
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
        var response = await httpClient.GetStringAsync(url);
        var edsList = JsonSerializer.Deserialize<EdsResponseModel>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        UpdateEdsList(edsList?.Data ?? new List<EdsModel>());
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error cargando EDS: {ex.Message}");
    }
}

private void UpdateEdsList(IEnumerable<EdsModel> edsData)
{
    EdsList.Clear();
    foreach (var eds in edsData)
    {
        EdsList.Add(eds);
    }
}
```

## 2. Actualizar el Constructor (línea ~378)

BUSCAR:
```csharp
public ProductService()
{
    _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
    IdProduct = 0;
    OriginalProduct = new ProductResponse();
    GetProducstAsync();
    InitializeProductOptions();
    GetAllProductTypeData();
    GetByIdProductDataCommand = new Command<int>(async (productId) => await GetByIdProductDataAsync(productId));
    SaveProductDataCommand = new Command(async () => await SaveProductDataAsync(), () => IsFormValid);
    EditProductDataCommand = new Command<ProductResponse>(async (dispenser) => await EditProductAsync(dispenser));
}
```

REEMPLAZAR CON:
```csharp
public ProductService()
{
    _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
    IdProduct = 0;
    OriginalProduct = new ProductResponse();
    GetProducstAsync();
    InitializeProductOptions();
    GetAllProductTypeData();
    GetAllEdsData();  // ? AGREGAR ESTA LÍNEA
    GetByIdProductDataCommand = new Command<int>(async (productId) => await GetByIdProductDataAsync(productId));
    SaveProductDataCommand = new Command(async () => await SaveProductDataAsync(), () => IsFormValid);
    EditProductDataCommand = new Command<ProductResponse>(async (dispenser) => await EditProductAsync(dispenser));
}
```

## 3. Actualizar método `SaveProductAsync` (línea ~857)

BUSCAR:
```csharp
private async Task SaveProductAsync(bool isUpdate)
{
    var product = new ProductModel
    {
        Name = Name.Trim(),
        IdProductType = IdProductType,
        SellPrice = SellPrice,
        PurchasePrice = PurchasePrice,
        Stock = Stock
    };
    if(isUpdate)
        product.IdProduct = IdProduct;
```

REEMPLAZAR CON:
```csharp
private async Task SaveProductAsync(bool isUpdate)
{
    // Validar EDS selection (required field)
    if (SelectedEds is null)
    {
        await CustomAlert.ShowErrorAsync("Debe seleccionar un EDS (Estación de Servicio) para registrar el producto", "EDS Requerido");
        return;
    }

    var product = new ProductModel
    {
        Name = Name.Trim(),
        IdProductType = IdProductType,
        SellPrice = SellPrice,
        PurchasePrice = PurchasePrice,
        Stock = Stock,
        IdEds = SelectedEds.IdEds  // ? AGREGAR ESTA LÍNEA
    };
    if(isUpdate)
        product.IdProduct = IdProduct;
```

## 4. Actualizar método `ClearForm` (línea ~1177)

BUSCAR:
```csharp
private void ClearForm()
{
    SelectedProductOption = null;
    SelectedSpecificProductType = null;
    SelectProductType = null;
    Name = string.Empty;
    IdProductType = 0;
    PurchasePrice = 0;
    SellPrice = 0;
    Stock = 0;
}
```

REEMPLAZAR CON:
```csharp
private void ClearForm()
{
    SelectedProductOption = null;
    SelectedSpecificProductType = null;
    SelectProductType = null;
    Name = string.Empty;
    IdProductType = 0;
    PurchasePrice = 0;
    SellPrice = 0;
    Stock = 0;
    SelectedEds = null;  // ? AGREGAR ESTA LÍNEA
}
```

## Resumen de Cambios

? **Paso 1**: Agregar métodos `GetAllEdsData()` y `UpdateEdsList()` 
? **Paso 2**: Llamar a `GetAllEdsData()` en el constructor
? **Paso 3**: Validar y asignar `IdEds` en `SaveProductAsync()`
? **Paso 4**: Limpiar `SelectedEds` en `ClearForm()`

Una vez aplicados estos cambios, el ProductService estará listo para trabajar con el campo IdEds.
