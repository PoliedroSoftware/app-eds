# ? CAMBIOS FINALES PARA PRODUCT SERVICE Y XAML

## ?? Archivo: `APP.Eds\Services\Product\ProductService.cs`

### ?? CAMBIO 1: Actualizar el Constructor (línea 378-389)

**BUSCAR estas líneas:**
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

**REEMPLAZAR CON:**
```csharp
public ProductService()
{
    _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
    IdProduct = 0;
    OriginalProduct = new ProductResponse();
    GetProducstAsync();
    InitializeProductOptions();
    GetAllProductTypeData();
    GetAllEdsData();  // ? LÍNEA NUEVA AGREGADA
    GetByIdProductDataCommand = new Command<int>(async (productId) => await GetByIdProductDataAsync(productId));
    SaveProductDataCommand = new Command(async () => await SaveProductDataAsync(), () => IsFormValid);
    EditProductDataCommand = new Command<ProductResponse>(async (dispenser) => await EditProductAsync(dispenser));
}
```

---

### ?? CAMBIO 2: Agregar Métodos GetAllEdsData y UpdateEdsList

**DESPUÉS del método `GetAllProductTypeData()` (alrededor de línea 688)**  
**ANTES del método `AddSampleData()` (alrededor de línea 689)**

**AGREGAR estos dos métodos:**

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

---

### ?? CAMBIO 3: Actualizar método SaveProductAsync (alrededor de línea 857-868)

**BUSCAR:**
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

**REEMPLAZAR CON:**
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
        IdEds = SelectedEds.IdEds  // ? LÍNEA NUEVA AGREGADA
    };
    if(isUpdate)
        product.IdProduct = IdProduct;
```

---

### ?? CAMBIO 4: Actualizar método ClearForm (alrededor de línea 1177-1186)

**BUSCAR:**
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

**REEMPLAZAR CON:**
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
    SelectedEds = null;  // ? LÍNEA NUEVA AGREGADA
}
```

---

## ?? Archivo: `APP.Eds\UsesCases\Product\ProductPostView.xaml`

### ?? CAMBIO 5: Agregar EDS Picker en el XAML

**BUSCAR una ubicación lógica en el formulario** (después del selector de tipo de producto o después del nombre).

**AGREGAR este código XAML:**

```xaml
<!-- EDS Selection -->
<VerticalStackLayout Spacing="8" Margin="0,12,0,0">
    <StackLayout Orientation="Horizontal" Spacing="4">
        <Label Text="Estación de Servicio (EDS)" 
               FontSize="14" 
               FontAttributes="Bold" 
               TextColor="#2D3436"
               Margin="4,0,0,0"/>
        <Label Text="*" 
               FontSize="14" 
               FontAttributes="Bold" 
               TextColor="#E74C3C"
               VerticalOptions="Start"/>
    </StackLayout>

    <Border BackgroundColor="#F8F9FA" 
            StrokeShape="RoundRectangle 12"
            Stroke="#DDD6FE"
            StrokeThickness="2"
            Padding="0">
        <StackLayout Orientation="Horizontal" Spacing="12" Padding="16,12">
            <Label Text="??" 
                   FontSize="18" 
                   TextColor="#8B5CF6" 
                   VerticalOptions="Center"/>

            <Picker x:Name="EdsPicker"
                    Title="Seleccione una estación de servicio"
                    ItemsSource="{Binding EdsList}"
                    ItemDisplayBinding="{Binding Name}"
                    SelectedItem="{Binding SelectedEds, Mode=TwoWay}"
                    AutomationId="PickerEds"
                    BackgroundColor="Transparent"
                    FontSize="16"
                    TextColor="#374151"
                    VerticalOptions="Center"
                    HorizontalOptions="FillAndExpand"/>
        </StackLayout>
    </Border>

    <Label Text="Campo obligatorio. Seleccione la estación de servicio a la que pertenece este producto."
           FontSize="12"
           TextColor="#6B7280"
           Margin="4,4,0,0"/>
</VerticalStackLayout>
```

---

## ? Resumen de Cambios

| # | Archivo | Línea Aprox | Cambio |
|---|---------|-------------|--------|
| 1 | ProductService.cs | 385 | Agregar `GetAllEdsData();` en constructor |
| 2 | ProductService.cs | 689 | Agregar métodos `GetAllEdsData()` y `UpdateEdsList()` |
| 3 | ProductService.cs | 857 | Validar y asignar `IdEds` en `SaveProductAsync()` |
| 4 | ProductService.cs | 1177 | Agregar `SelectedEds = null;` en `ClearForm()` |
| 5 | ProductPostView.xaml | Variable | Agregar Picker de EDS en el formulario |

---

## ?? Verificación Final

Después de aplicar todos los cambios:

1. ? Compilar la solución para verificar que no haya errores
2. ? El formulario debe mostrar el selector de EDS
3. ? El EDS debe ser obligatorio antes de guardar
4. ? El `IdEds` debe incluirse en la solicitud al API
5. ? El campo debe limpiarse al limpiar el formulario

---

## ?? Notas

- El Picker de EDS se ha estilizado para coincidir con el diseño del formulario de Shopping
- La validación del EDS ocurre ANTES de crear el objeto `ProductModel`
- Si el EDS no está seleccionado, se muestra un mensaje de error amigable
- El icono ?? indica visualmente que es una estación de servicio

¡Todos los cambios están listos para aplicarse!
