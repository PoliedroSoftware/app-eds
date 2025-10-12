using APP.Eds.Components.PopUp;
using APP.Eds.Helpers;
using APP.Eds.Models.Hose;
using APP.Eds.Models.Product;
using APP.Eds.Models.Shopping;
using APP.Eds.Models.ShoppingProduct;
using APP.Eds.Services.Config;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows.Input;

namespace APP.Eds.Services.Product;

// Modelo para las opciones de producto principales
public class ProductOption
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Icon { get; set; }

    public ProductOption(int id, string name, string icon)
    {
        Id = id;
        Name = name;
        Icon = icon;
    }
}

// Modelo para los tipos de producto específicos
public class SpecificProductType
{
    public int Id { get; set; }
    public string Description { get; set; }
    public int ParentProductId { get; set; }
    public string Icon { get; set; }

    public SpecificProductType(int id, string description, int parentProductId, string icon)
    {
        Id = id;
        Description = description;
        ParentProductId = parentProductId;
        Icon = icon;
    }
}

public class EnhancedProductTypeItem : ProductTypeModelResponse
{
    public string TypeIcon { get; set; } = "🏷️";
    public string CategoryDescription { get; set; } = "Categoría de producto";

    public EnhancedProductTypeItem(ProductTypeModelResponse original)
    {
        IdProductType = original.IdProductType;
        Description = original.Description;

        // Set icon based on description
        TypeIcon = GetTypeIcon(Description);
        CategoryDescription = GetCategoryDescription(Description);
    }

    private string GetTypeIcon(string description)
    {
        var desc = description?.ToLowerInvariant() ?? "";

        if (desc.Contains("combustible") || desc.Contains("gasolina") || desc.Contains("diesel") || desc.Contains("gnv"))
            return "⛽";
        else if (desc.Contains("lubricante") || desc.Contains("aceite") || desc.Contains("grasa"))
            return "🛢️";
        else if (desc.Contains("aditivo") || desc.Contains("mejorador") || desc.Contains("limpiador"))
            return "🧪";
        else if (desc.Contains("servicio") || desc.Contains("lavado") || desc.Contains("mantenimiento"))
            return "🔧";
        else if (desc.Contains("repuesto") || desc.Contains("accesorio"))
            return "🔩";
        else if (desc.Contains("alimenticio") || desc.Contains("bebida") || desc.Contains("snack"))
            return "🥤";
        else
            return "🏷️";
    }

    private string GetCategoryDescription(string description)
    {
        var desc = description?.ToLowerInvariant() ?? "";

        if (desc.Contains("combustible") || desc.Contains("gasolina") || desc.Contains("diesel"))
            return "Combustibles y carburantes";
        else if (desc.Contains("lubricante") || desc.Contains("aceite"))
            return "Lubricantes y aceites";
        else if (desc.Contains("aditivo"))
            return "Aditivos y mejoradores";
        else if (desc.Contains("servicio"))
            return "Servicios y mantenimiento";
        else if (desc.Contains("repuesto"))
            return "Repuestos y accesorios";
        else if (desc.Contains("alimenticio") || desc.Contains("bebida"))
            return "Productos alimenticios";
        else
            return "Categoría general";
    }
}

public class ProductService : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<ProductTypeModelResponse> ProductTypeList { get; set; } = [];
    public ObservableCollection<EnhancedProductTypeItem> EnhancedProductTypeList { get; set; } = [];
    public ObservableCollection<ProductResponse> ProductList { get; set; } = [];

    // Nuevas colecciones para el sistema de productos específicos
    public ObservableCollection<ProductOption> ProductOptions { get; set; } = [];
    public ObservableCollection<SpecificProductType> AvailableProductTypes { get; set; } = [];
    public ObservableCollection<SpecificProductType> FilteredProductTypes { get; set; } = [];

    private ProductRequest Request { get; set; }
    private ProductModel _product;
    private string? _authToken;

    public ProductModel ProductModel
    {
        get => _product;
        set
        {
            _product = value;
            OnPropertyChanged(nameof(ProductModel));
        }
    }

    private string _name;
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(IsFormValid)); // Notificar cambio en validez del formulario
        }
    }

    private int _idProductType;
    public int IdProductType
    {
        get => _idProductType;
        set
        {
            _idProductType = value;
            OnPropertyChanged(nameof(IdProductType));
            OnPropertyChanged(nameof(IsFormValid)); // Notificar cambio en validez del formulario
        }
    }

    private int _idProduct;
    public int IdProduct
    {
        get => _idProduct;
        set
        {
            _idProduct = value;
            OnPropertyChanged(nameof(IdProduct));
        }
    }

    private EnhancedProductTypeItem _selectedProductType;
    public EnhancedProductTypeItem SelectProductType
    {
        get => _selectedProductType;
        set
        {
            _selectedProductType = value;
            OnPropertyChanged(nameof(SelectProductType));
            if (_selectedProductType != null)
            {
                IdProductType = _selectedProductType.IdProductType;
            }
        }
    }

    // Nuevas propiedades para el sistema de productos específicos
    private ProductOption _selectedProductOption;
    public ProductOption SelectedProductOption
    {
        get => _selectedProductOption;
        set
        {
            _selectedProductOption = value;
            OnPropertyChanged(nameof(SelectedProductOption));
            OnProductOptionChanged();
        }
    }

    private SpecificProductType _selectedSpecificProductType;
    public SpecificProductType SelectedSpecificProductType
    {
        get => _selectedSpecificProductType;
        set
        {
            _selectedSpecificProductType = value;
            OnPropertyChanged(nameof(SelectedSpecificProductType));

            if (_selectedSpecificProductType != null && SelectedProductOption != null)
            {
                // Lógica especial para evitar duplicación en ACPM
                if (SelectedProductOption.Id == 2) // ACPM
                {
                    // Para ACPM, solo mostrar "ACPM" sin duplicar
                    Name = "ACPM";
                }
                else
                {
                    // Para otros productos (como Gasolina), construir el nombre completo
                    var productName = $"{SelectedProductOption.Name} {_selectedSpecificProductType.Description}".Trim();
                    Name = productName;
                }

                // Establecer el tipo de producto correcto
                UpdateProductTypeFromSelection();
            }
        }
    }

    // Propiedades de visibilidad para la interfaz
    public bool IsProductTypeSelectionVisible => SelectedProductOption?.Id == 1; // Gasolina
    public bool IsAcpmSelected => SelectedProductOption?.Id == 2; // ACPM

    // Propiedad para validar si el formulario está listo para enviar
    public bool IsFormValid => !string.IsNullOrWhiteSpace(Name) && IdProductType > 0;

    private double _sellPrice;
    public double SellPrice
    {
        get => _sellPrice;
        set
        {
            // Si el valor es menor que 0, establecer como 0
            _sellPrice = value < 0 ? 0 : value;
            OnPropertyChanged(nameof(SellPrice));
        }
    }
    // Fuerza el IdProductType y el Name según lo seleccionado / escrito.
    // Devuelve true si pudo fijar un tipo válido.
    private bool ForceProductTypeIfConsistent()
    {
        // 1) Atajos por selección explícita
        if (SelectedProductOption?.Id == 2) // ACPM
        {
            IdProductType = 2;
            if (string.IsNullOrWhiteSpace(Name)) Name = "ACPM";
            return true;
        }

        if (SelectedProductOption?.Id == 1) // Gasolina
        {
            var term = (SelectedSpecificProductType?.Description ?? "").Trim().ToLowerInvariant();

            if (term == "extra")
            {
                IdProductType = 9;
                if (string.IsNullOrWhiteSpace(Name)) Name = "Gasolina Extra";
                return true;
            }

            if (term == "corriente")
            {
                IdProductType = 1;
                if (string.IsNullOrWhiteSpace(Name)) Name = "Gasolina Corriente";
                return true;
            }
        }

        // 2) Inferencia por nombre escrito
        var n = (Name ?? "").Trim().ToLowerInvariant();
        if (n.Contains("acpm"))
        {
            IdProductType = 2; return true;
        }
        if (n.Contains("extra"))
        {
            IdProductType = 9; return true;
        }
        if (n.Contains("corriente"))
        {
            IdProductType = 1; return true;
        }

        return false;
    }

    private double _purchasePrice;
    public double PurchasePrice
    {
        get => _purchasePrice;
        set
        {
            // Si el valor es menor que 0, establecer como 0
            _purchasePrice = value < 0 ? 0 : value;
            OnPropertyChanged(nameof(PurchasePrice));
        }
    }

    private int _stock;
    public int Stock
    {
        get => _stock;
        set
        {
            // Si el valor es menor que 0, establecer como 0
            _stock = value < 0 ? 0 : value;
            OnPropertyChanged(nameof(Stock));
        }
    }

    // Nueva propiedad para mostrar la unidad de medida del stock
    public string StockUnit => "galones";
    public string StockPlaceholder => "Ingrese el stock inicial en galones";
    public string StockLabel => "Stock Inicial (Galones)";
    public string StockHint => "El stock debe ser especificado en galones (G). Ejemplo: 1000 galones";

    public ProductService()
    {
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
        IdProduct = 0;
        GetProducstAsync();
        InitializeProductOptions();
        GetAllProductTypeData();
        GetByIdProductDataCommand = new Command<int>(async (productId) => await GetByIdProductDataAsync(productId));
        SaveProductDataCommand = new Command(async () => await SaveProductDataAsync(), () => IsFormValid);
        EditProductDataCommand = new Command<ProductResponse>(async (dispenser) => await EditProductAsync(dispenser));
    }

    private void InitializeProductOptions()
    {
        // Inicializar las opciones de producto principales
        ProductOptions.Clear();
        ProductOptions.Add(new ProductOption(1, "Gasolina", "⛽"));
        ProductOptions.Add(new ProductOption(2, "ACPM", "🚛"));

        // Inicializar los tipos específicos
        AvailableProductTypes.Clear();
        AvailableProductTypes.Add(new SpecificProductType(1, "Corriente", 1, "⛽")); // Gasolina Corriente
        AvailableProductTypes.Add(new SpecificProductType(2, "Extra", 1, "✨"));     // Gasolina Extra
        AvailableProductTypes.Add(new SpecificProductType(3, "ACPM", 2, "🚛"));     // ACPM

        OnPropertyChanged(nameof(ProductOptions));
        OnPropertyChanged(nameof(AvailableProductTypes));
    }

    private void OnProductOptionChanged()
    {
        if (SelectedProductOption == null)
        {
            FilteredProductTypes.Clear();
            SelectedSpecificProductType = null;
        }
        else
        {
            // Filtrar tipos de producto según la opción seleccionada
            FilteredProductTypes.Clear();
            var filteredTypes = AvailableProductTypes.Where(pt => pt.ParentProductId == SelectedProductOption.Id);

            foreach (var type in filteredTypes)
            {
                FilteredProductTypes.Add(type);
            }

            // Si es ACPM, seleccionar automáticamente
            if (SelectedProductOption.Id == 2) // ACPM
            {
                SelectedSpecificProductType = FilteredProductTypes.FirstOrDefault();
            }
            else
            {
                SelectedSpecificProductType = null;
            }
        }

        // Notificar cambios en las propiedades de visibilidad
        OnPropertyChanged(nameof(FilteredProductTypes));
        OnPropertyChanged(nameof(IsProductTypeSelectionVisible));
        OnPropertyChanged(nameof(IsAcpmSelected));
    }

    // Método para actualizar el tipo de producto basado en la selección
    private void UpdateProductTypeFromSelection()
    {
        if (SelectedSpecificProductType == null || SelectedProductOption == null)
            return;

        // Buscar en la lista de tipos de producto existentes
        var searchTerm = SelectedSpecificProductType.Description.ToLowerInvariant();
        var matchingType = EnhancedProductTypeList.FirstOrDefault(pt =>
        {
            var ptDesc = pt.Description.ToLowerInvariant();

            // Para gasolina, buscar coincidencia exacta con el tipo (corriente/extra)
            if (SelectedProductOption.Id == 1) // Gasolina
            {
                return ptDesc.Contains("gasolina") && ptDesc.Contains(searchTerm);
            }
            // Para ACPM, buscar cualquier tipo que contenga ACPM
            else if (SelectedProductOption.Id == 2) // ACPM
            {
                return ptDesc.Contains("acpm");
            }

            return false;
        });

        if (matchingType != null)
        {
            IdProductType = matchingType.IdProductType;
            SelectProductType = matchingType;
        }
        else
        {
            // Fallback: usar el primer tipo disponible como respaldo
            IdProductType = EnhancedProductTypeList.FirstOrDefault()?.IdProductType ?? 1;
        }
    }

    // Validación del formulario antes del envío
    // Validación del formulario antes del envío (ahora autocorrige el tipo)
    private async Task<bool> ValidateFormAsync()
    {
        var errors = new List<string>();

        // Auto-corrección fuerte del tipo ANTES de validar
        ForceProductTypeIfConsistent();

        // Nombre: obligatorio (si falta, lo intentamos generar a partir de la selección)
        if (string.IsNullOrWhiteSpace(Name))
        {
            // último intento de generación de nombre
            if (SelectedProductOption?.Id == 2) Name = "ACPM";
            else if (SelectedProductOption?.Id == 1 && SelectedSpecificProductType != null)
                Name = $"Gasolina {SelectedSpecificProductType.Description}".Trim();

            if (string.IsNullOrWhiteSpace(Name))
                errors.Add("• El nombre del producto es obligatorio");
        }

        // Si aún no logramos un tipo, como último recurso:
        if (IdProductType <= 0)
        {
            // Sólo si escogió Gasolina sin subtipo, asumimos Corriente
            if (SelectedProductOption?.Id == 1 && SelectedSpecificProductType == null)
            {
                IdProductType = 1; // Corriente por defecto
                if (string.IsNullOrWhiteSpace(Name)) Name = "Gasolina Corriente";
            }
        }

        // Validaciones numéricas
        if (PurchasePrice < 0) errors.Add("• El precio de compra no puede ser negativo");
        if (SellPrice < 0) errors.Add("• El precio de venta no puede ser negativo");
        if (Stock < 0) errors.Add("• El stock no puede ser negativo");

        // Advertencia de stock alto
        if (Stock > 50000)
        {
            bool confirm = await CustomAlert.ShowConfirmAsync(
                $"⚠️ Stock Muy Alto\n\nHa ingresado {Stock:N0} galones.\n\n¿Está seguro?",
                "Confirmar Stock Alto", "Continuar", "Revisar");
            if (!confirm) return false;
        }

        if (errors.Any())
        {
            var msg = "⚠️ Errores de Validación\n\n" + string.Join("\n", errors);
            await CustomAlert.ShowErrorAsync(msg, "Formulario Incompleto");
            return false;
        }

        // A esta altura garantizamos que Name no es vacío.
        // El IdProductType puede haber quedado fijado por autocorrección
        // (1 Corriente / 9 Extra / 2 ACPM).
        return true;
    }


    private async void GetAllProductTypeData()
    {
        try
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                System.Diagnostics.Debug.WriteLine("No authentication token found, using sample data");
                AddSampleData();
                return;
            }

            string url = $"{Configuration.BaseUrl}/api/v1/producttype";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            var response = await httpClient.GetStringAsync(url);
            var productTypeResponse = JsonSerializer.Deserialize<ProductTypeResponse>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (productTypeResponse?.Data != null)
            {
                UpdateProductTypeList(productTypeResponse.Data);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No data received from API, using sample data");
                AddSampleData();
            }
        }
        catch (HttpRequestException httpEx)
        {
            System.Diagnostics.Debug.WriteLine($"HTTP error loading product types: {httpEx.Message}");
            AddSampleData();
        }
        catch (JsonException jsonEx)
        {
            System.Diagnostics.Debug.WriteLine($"JSON parsing error: {jsonEx.Message}");
            AddSampleData();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"General error loading product types: {ex.Message}");
            AddSampleData();
        }
    }

    private void AddSampleData()
    {
        try
        {
            var sampleTypes = new List<ProductTypeModelResponse>
            {
                new ProductTypeModelResponse { IdProductType = 1, Description = "Gasolina Corriente" },
                new ProductTypeModelResponse { IdProductType = 2, Description = "Gasolina Extra" },
                new ProductTypeModelResponse { IdProductType = 3, Description = "ACPM" },
                new ProductTypeModelResponse { IdProductType = 4, Description = "Lubricantes" }
            };
            UpdateProductTypeList(sampleTypes);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error adding sample data: {ex.Message}");
        }
    }

    private void UpdateProductTypeList(IEnumerable<ProductTypeModelResponse> data)
    {
        try
        {
            ProductTypeList.Clear();
            EnhancedProductTypeList.Clear();

            if (data != null)
            {
                foreach (var item in data)
                {
                    ProductTypeList.Add(item);
                    EnhancedProductTypeList.Add(new EnhancedProductTypeItem(item));
                }
            }

            // Notify that collections have changed
            OnPropertyChanged(nameof(ProductTypeList));
            OnPropertyChanged(nameof(EnhancedProductTypeList));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating product type list: {ex.Message}");
        }
    }

    public async Task GetByIdProductDataAsync(int productId)
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await CustomAlert.ShowErrorAsync("No se encontró el token de autenticación", "Error de Autenticación");
            return;
        }
        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/product/{productId}");
            Console.WriteLine(response);

            ProductModel = JsonSerializer.Deserialize<ProductModel>(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            // Cargar datos en el formulario
            if (ProductModel != null)
            {
                Name = ProductModel.Name;
                IdProductType = ProductModel.IdProductType;
                SellPrice = ProductModel.SellPrice;
                PurchasePrice = ProductModel.PurchasePrice;
                Stock = ProductModel.Stock;

                // Seleccionar el tipo de producto correspondiente
                SelectProductType = EnhancedProductTypeList.FirstOrDefault(pt => pt.IdProductType == ProductModel.IdProductType);
            }
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"No se pudo cargar el producto:\n\n{ex.Message}", "Error de Carga");
        }
    }

    public async Task SaveProductDataAsync()
    {
        // Refuerzo de mapeo antes de cualquier cosa
        ForceProductTypeIfConsistent();

        if (string.IsNullOrEmpty(_authToken))
        {
            await CustomAlert.ShowErrorAsync("No se encontró el token de autenticación", "Error de Autenticación");
            return;
        }

        // Valida (con autocorrección integrada)
        if (!await ValidateFormAsync()) return;

        try
        {
            if (IdProduct > 0)
            {
                await UpdateAsync();
            }
            else
            {
                await CreateAsync();
            }
           
        }
        catch (HttpRequestException)
        {
            await CustomAlert.ShowErrorAsync("🌐 Error de Conexión\n\nNo se pudo conectar con el servidor.", "Error de Conexión");
        }
        catch (JsonException)
        {
            await CustomAlert.ShowErrorAsync("📝 Error de Formato\n\nError al procesar la respuesta del servidor.", "Error de Datos");
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"❗ Error Inesperado\n\n{ex.Message}", "Error del Sistema");
        }
    }

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

        object payload = isUpdate
            ? product
            : new ProductRequest { Request = product };

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _authToken);

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(payload, jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var url = $"{Configuration.BaseUrl}/api/v1/product";
        HttpResponseMessage response = isUpdate
            ? await httpClient.PutAsync(url, content)
            : await httpClient.PostAsync(url, content);

        if (response.IsSuccessStatusCode)
            await HandleSuccessAsync(isUpdate);
        else
            await HandleErrorAsync(response);
    }

    private async Task HandleSuccessAsync(bool isUpdate)
    {
        var action = isUpdate ? "Actualizado" : "Registrado";
        var successMessage = $"✅ Producto {action}\n\nEl producto '{Name}' ha sido {action.ToLower()} exitosamente";

        var details = new List<string>();
        if (PurchasePrice > 0) details.Add($"• Precio de compra: ${PurchasePrice:F2}");
        if (SellPrice > 0) details.Add($"• Precio de venta: ${SellPrice:F2}");
        if (Stock > 0) details.Add($"• Stock inicial: {Stock:N0} galones");

        if (details.Any())
            successMessage += ":\n\n" + string.Join("\n", details);
        else
            successMessage += "\n\n📋 Configuración:\n• Sin precios definidos\n• Sin stock inicial\n\nPuede actualizar precios y agregar stock (en galones) posteriormente.";

        await CustomAlert.ShowSuccessAsync(successMessage, "¡Éxito!");
        ClearForm();
        await GetProducstAsync();
        IdProduct = 0;
    }

    private async Task HandleErrorAsync(HttpResponseMessage response)
    {
        var serverError = await response.Content.ReadAsStringAsync();
        var userFriendlyError = TranslateServerError(serverError, Name, SelectedProductOption?.Name, SelectedSpecificProductType?.Description);

        var title = response.RequestMessage?.Method == HttpMethod.Post
            ? "Error al Registrar Producto"
            : "Error al Actualizar Producto";

        await CustomAlert.ShowErrorAsync(userFriendlyError, title);
    }

    private Task CreateAsync() => SaveProductAsync(false);
    private Task UpdateAsync() => SaveProductAsync(true);

    private string TranslateServerError(string serverError, string productName, string productType, string productSubtype)
    {
        if (string.IsNullOrEmpty(serverError))
        {
            return "❌ Error Desconocido\n\n" +
                   "No se pudo registrar el producto. El servidor no proporcionó detalles del error.\n\n" +
                   "Por favor, intente de nuevo más tarde.";
        }

        var errorLower = serverError.ToLowerInvariant();

        // Detectar errores de producto duplicado - patrones más específicos
        if (errorLower.Contains("duplicate") ||
            errorLower.Contains("already exists") ||
            errorLower.Contains("unique constraint") ||
            errorLower.Contains("duplicado") ||
            errorLower.Contains("ya existe") ||
            errorLower.Contains("product_unique") ||
            errorLower.Contains("violates unique constraint") ||
            errorLower.Contains("duplicate key") ||
            errorLower.Contains("unique key constraint") ||
            errorLower.Contains("cannot insert duplicate key") ||
            errorLower.Contains("duplicate entry") ||
            errorLower.Contains("constraint violation") ||
            errorLower.Contains("ix_") ||  // Índices únicos SQL Server
            errorLower.Contains("uc_") ||  // Unique constraints
            errorLower.Contains("pk_") ||  // Primary key violations
            errorLower.Contains("23505") || // PostgreSQL unique violation
            errorLower.Contains("2627") ||  // SQL Server unique constraint violation
            errorLower.Contains("1062") ||  // MySQL duplicate entry
            (errorLower.Contains("status") && errorLower.Contains("500") && errorLower.Contains("processing")) ||
            (errorLower.Contains("internalservererror") && (errorLower.Contains("product") || errorLower.Contains("name"))))
        {
            var productTypeDisplay = !string.IsNullOrEmpty(productSubtype) ?
                $"{productType} {productSubtype}" :
                (productType ?? "el tipo seleccionado");

            return $"⚠️ Producto Ya Existente\n\n" +
                   $"Ya existe un producto con el nombre '{productName}' del tipo '{productTypeDisplay}' en el sistema.\n\n" +
                   $"🚫 No se pueden registrar productos duplicados\n\n" +
                   $"💡 Soluciones disponibles:\n\n" +
                   $"✅ Cambiar el nombre del producto:\n" +
                   $"   • Agregar marca o proveedor\n" +
                   $"   • Incluir características específicas\n" +
                   $"   • Usar numeración (ej: '{productName} 2')\n\n" +
                   $"✅ Verificar productos existentes:\n" +
                   $"   • El producto podría ya estar registrado\n" +
                   $"   • Revisar la lista de productos actuales\n\n" +
                   $"✅ Seleccionar un tipo diferente:\n" +
                   $"   • Si el producto es de otro tipo\n" +
                   $"   • Verificar la categoría correcta\n\n" +
                   $"📞 Si necesita ayuda, contacte al soporte técnico.";
        }

        // Detectar errores de campos requeridos
        if (errorLower.Contains("name") && (errorLower.Contains("required") || errorLower.Contains("null")))
        {
            return "📝 Nombre Obligatorio\n\n" +
                   "El nombre del producto es obligatorio y no puede estar vacío.\n\n" +
                   "Por favor, seleccione el tipo de combustible para generar automáticamente el nombre, " +
                   "o ingrese un nombre manualmente.";
        }

        if (errorLower.Contains("producttype") && (errorLower.Contains("required") || errorLower.Contains("invalid") || errorLower.Contains("null")))
        {
            return "🏷️ Tipo de Producto Obligatorio\n\n" +
                   "Debe seleccionar un tipo de producto válido.\n\n" +
                   "Por favor, seleccione un tipo de combustible (Gasolina o ACPM) " +
                   "de las opciones disponibles.";
        }

        // Detectar errores de validación de precios
        if (errorLower.Contains("price") && errorLower.Contains("invalid"))
        {
            return "💰 Precio Inválido\n\n" +
                   "Los precios ingresados no son válidos.\n\n" +
                   "✅ Asegúrese de que:\n" +
                   "• Los precios sean números positivos o cero\n" +
                   "• Use punto decimal (.) para decimales\n" +
                   "• No incluya símbolos de moneda\n" +
                   "• Los campos de precio son opcionales";
        }

        // Detectar errores de validación de stock
        if (errorLower.Contains("stock") && errorLower.Contains("invalid"))
        {
            return "📦 Stock Inválido\n\n" +
                   "La cantidad de stock ingresada no es válida.\n\n" +
                   "✅ Requisitos del stock:\n" +
                   "• Debe ser un número entero positivo o cero\n" +
                   "• Se debe especificar en galones (G)\n" +
                   "• Ejemplo: 1500 (equivale a 1,500 galones)\n" +
                   "• Este campo es opcional";
        }

        // Detectar errores de autorización
        if (errorLower.Contains("unauthorized") || errorLower.Contains("forbidden") || errorLower.Contains("401") || errorLower.Contains("403"))
        {
            return "🔐 Sin Autorización\n\n" +
                   "No tiene permisos suficientes para registrar productos.\n\n" +
                   "Contacte al administrador del sistema para obtener los permisos necesarios.";
        }

        // Detectar errores de límite
        if (errorLower.Contains("limit") || errorLower.Contains("maximum") || errorLower.Contains("quota"))
        {
            return "🚫 Límite Alcanzado\n\n" +
                   "Ha alcanzado el límite máximo de productos que puede registrar.\n\n" +
                   "Contacte al administrador para aumentar su cuota de productos.";
        }

        // Detectar errores de servidor interno (500) genéricos
        if ((errorLower.Contains("internalservererror") || errorLower.Contains("internal server error") ||
             errorLower.Contains("status") && errorLower.Contains("500")) &&
            !errorLower.Contains("duplicate") && !errorLower.Contains("unique"))
        {
            return $"⚠️ Error del Servidor\n\n" +
                   $"Se produjo un error interno en el servidor al procesar su solicitud.\n\n" +
                   $"🔄 Posibles causas:\n" +
                   $"• Problemas temporales del servidor\n" +
                   $"• Sobrecarga del sistema\n" +
                   $"• Error en el procesamiento de datos\n\n" +
                   $"💡 Recomendaciones:\n" +
                   $"• Espere unos minutos e intente nuevamente\n" +
                   $"• Verifique que todos los campos estén correctos\n" +
                   $"• Si persiste, contacte al soporte técnico\n\n" +
                   $"🔧 Si necesita ayuda inmediata, proporcione estos detalles al soporte:\n" +
                   $"'{serverError.Substring(0, Math.Min(serverError.Length, 200))}'";
        }

        // Detectar errores de validación generales
        if (errorLower.Contains("validation") || errorLower.Contains("invalid"))
        {
            return $"❌ Error de Validación\n\n" +
                   $"Los datos ingresados no cumplen con los requisitos del sistema:\n\n" +
                   $"✅ Campos obligatorios:\n" +
                   $"• Nombre: obligatorio, no vacío\n" +
                   $"• Tipo de producto: obligatorio, debe ser válido\n\n" +
                   $"📋 Campos opcionales:\n" +
                   $"• Precios: deben ser números positivos (en pesos)\n" +
                   $"• Stock: debe ser número entero positivo (en galones)\n\n" +
                   $"💡 Ejemplo de stock: 1500 = 1,500 galones\n\n" +
                   $"🔧 Para soporte técnico, detalle del error:\n" +
                   $"'{serverError.Substring(0, Math.Min(serverError.Length, 150))}'";
        }

        // Error genérico mejorado para cualquier otro caso
        return $"❗ Error Inesperado\n\n" +
               $"Se produjo un error al registrar el producto que no pudimos identificar específicamente.\n\n" +
               $"🔄 Recomendaciones:\n" +
               $"• Verifique que toda la información esté correcta\n" +
               $"• Intente registrar el producto nuevamente\n" +
               $"• Si el error persiste, contacte al soporte técnico\n\n" +
               $"🔧 Información del error para soporte técnico:\n" +
               $"'{serverError.Substring(0, Math.Min(serverError.Length, 200))}'\n\n" +
               $"📞 Al contactar soporte, proporcione esta información junto con:\n" +
               $"• Nombre del producto: '{productName}'\n" +
               $"• Tipo: '{productType ?? "No especificado"}'\n" +
               $"• Fecha y hora del intento";
    }

    // Delete product
    public async Task<bool> DeleteProductAsync(int idProduct)
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await CustomAlert.ShowErrorAsync("No se encontró el token de autenticación", "Error de Autenticación");
            return false;
        }
        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.DeleteAsync($"{Configuration.BaseUrl}/api/v1/product/{idProduct}");
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                var serverError = await response.Content.ReadAsStringAsync();
                var userFriendlyError = TranslateDeleteError(serverError);
                await CustomAlert.ShowErrorAsync(userFriendlyError, "Error al Eliminar Producto");
            }
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error al eliminar el producto:\n\n{ex.Message}", "Error del Sistema");
        }
        return false;
    }

    private string TranslateDeleteError(string serverError)
    {
        if (string.IsNullOrEmpty(serverError))
        {
            return "No se pudo eliminar el producto. Por favor, intente de nuevo más tarde.";
        }

        var errorLower = serverError.ToLowerInvariant();

        // Detectar restricciones de integridad referencial
        if (errorLower.Contains("foreign key") ||
            errorLower.Contains("constraint") ||
            errorLower.Contains("referenced") ||
            errorLower.Contains("in use"))
        {
            return "🔗 Producto en Uso\n\n" +
                   "No se puede eliminar este producto porque está siendo utilizado por:\n\n" +
                   "• Dispensadores o mangueras\n" +
                   "• Inventarios existentes\n" +
                   "• Transacciones históricas\n\n" +
                   "Para eliminarlo, primero debe:\n" +
                   "• Remover todas las referencias al producto\n" +
                   "• O desactivar el producto en lugar de eliminarlo";
        }

        // Detectar errores de autorización
        if (errorLower.Contains("unauthorized") || errorLower.Contains("forbidden"))
        {
            return "🔐 Sin Permisos\n\n" +
                   "No tiene permisos suficientes para eliminar productos.\n\n" +
                   "Contacte al administrador del sistema.";
        }

        // Producto no encontrado
        if (errorLower.Contains("not found") || errorLower.Contains("no existe"))
        {
            return "❓ Producto No Encontrado\n\n" +
                   "El producto que intenta eliminar ya no existe en el sistema.\n\n" +
                   "Es posible que ya haya sido eliminado anteriormente.";
        }

        return $"❗ Error al Eliminar\n\n" +
               $"Se produjo un error inesperado:\n\n{serverError}\n\n" +
               $"Si el problema persiste, contacte al soporte técnico.";
    }

    // Update product
    public async Task<bool> UpdateProductAsync(int idProduct, ProductModel model)
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await CustomAlert.ShowErrorAsync("No se encontró el token de autenticación", "Error de Autenticación");
            return false;
        }
        try
        {
            var request = new ProductRequest { Request = model };
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PutAsync($"{Configuration.BaseUrl}/api/v1/product/{idProduct}", content);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                var serverError = await response.Content.ReadAsStringAsync();
                var userFriendlyError = TranslateUpdateError(serverError, model.Name);
                await CustomAlert.ShowErrorAsync(userFriendlyError, "Error al Actualizar Producto");
            }
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error al actualizar el producto:\n\n{ex.Message}", "Error del Sistema");
        }
        return false;
    }

    private string TranslateUpdateError(string serverError, string productName)
    {
        if (string.IsNullOrEmpty(serverError))
        {
            return "No se pudo actualizar el producto. Por favor, intente de nuevo más tarde.";
        }

        var errorLower = serverError.ToLowerInvariant();

        // Detectar errores de producto duplicado en actualización
        if (errorLower.Contains("duplicate") ||
            errorLower.Contains("already exists") ||
            errorLower.Contains("unique constraint") ||
            errorLower.Contains("duplicado") ||
            errorLower.Contains("ya existe") ||
            errorLower.Contains("product_unique"))
        {
            return $"⚠️ Conflicto de Nombres\n\n" +
                   $"Ya existe otro producto con el nombre '{productName}' del mismo tipo.\n\n" +
                   $"Para actualizar este producto, use un nombre único o cambie el tipo.\n\n" +
                   $"Sugerencias:\n" +
                   $"• Agregar especificaciones al nombre\n" +
                   $"• Verificar productos existentes\n" +
                   $"• Usar un nombre completamente diferente";
        }

        // Producto no encontrado
        if (errorLower.Contains("not found") || errorLower.Contains("no existe"))
        {
            return "❓ Producto No Encontrado\n\n" +
                   "El producto que intenta actualizar no existe en el sistema.\n\n" +
                   "Es posible que haya sido eliminado por otro usuario.";
        }

        // Detectar errores de validación
        if (errorLower.Contains("validation") || errorLower.Contains("invalid"))
        {
            return $"❌ Datos Inválidos\n\n" +
                   $"Los datos de actualización no son válidos:\n\n{serverError}\n\n" +
                   $"Por favor, revise la información e intente nuevamente.";
        }

        // Detectar errores de autorización
        if (errorLower.Contains("unauthorized") || errorLower.Contains("forbidden"))
        {
            return "🔐 Sin Permisos\n\n" +
                   "No tiene permisos suficientes para actualizar productos.\n\n" +
                   "Contacte al administrador del sistema.";
        }

        return $"❗ Error de Actualización\n\n" +
               $"Se produjo un error inesperado:\n\n{serverError}\n\n" +
               $"Si el problema persiste, contacte al soporte técnico.";
    }

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

    public async Task RefreshProductTypesAsync()
    {
        try
        {
            GetAllProductTypeData();
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error refreshing product types: {ex.Message}");
        }
    }

    public ICommand GetByIdProductDataCommand { get; }
    public ICommand SaveProductDataCommand { get; }
    public ICommand EditProductDataCommand { get; private set; }

    private async Task EditProductAsync(ProductResponse product)
    {
        try
        {
            Name = product.Name;
            IdProductType = product.IdProductType;
            SellPrice = product.SellPrice;
            PurchasePrice = product.PurchasePrice;
            Stock = (int)product.Stock;
            IdProduct = product.IdProduct;

            // Find and select the corresponding items in the dropdowns
            SelectedProductOption = ProductOptions.FirstOrDefault(x => x.Id == product.IdProductType);
            
            await Application.Current.MainPage.DisplayAlert("Modo Edicion", $"Datos del producto '{product.Name}' cargados para edicion", "OK");
            await ValidateFormAsync();
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error editando producto: {ex.Message}", "OK");
        }
    }

    public async Task GetProducstAsync()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return;
        }
        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/product");
            var products = JsonSerializer.Deserialize<ProductApiResponse>(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            ProductList.Clear();
            foreach (var product in products.Data)
            {
                ProductList.Add(product);
            }
            EnrichProductListWithNames();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private void EnrichProductListWithNames()
    {
        foreach (var product in ProductList)
        {
            product.ProductTypeName = ProductOptions.FirstOrDefault(p => p.Id == product.IdProductType)?.Name ?? string.Empty;
        }
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Actualizar el estado del comando de guardar cuando cambie la validez del formulario
        if (propertyName == nameof(IsFormValid))
        {
            ((Command)SaveProductDataCommand).ChangeCanExecute();
        }
    }
}
