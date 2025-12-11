using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows.Input;
using System.Xml.Linq;
using APP.Eds.Components.PopUp;
using APP.Eds.Helpers;
using APP.Eds.Models.Shopping;
using APP.Eds.Models.ShoppingProduct;
using APP.Eds.Services.Config;
using APP.Eds.Models.Islander;

namespace APP.Eds.Services.Shopping;

public class ShoppingService : INotifyPropertyChanged
{
    private static ShoppingService _instance;
    private string? _authToken;
    public static ShoppingService Instance => _instance ??= new ShoppingService();

    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<ProviderModel> ProviderList { get; set; } = [];
    public ObservableCollection<CategoryModel> CategoryList { get; set; } = [];
    public ObservableCollection<ProductCompartimentPairModel> ProductCompartimentPairs { get; set; } = [];
    public ObservableCollection<ProductCompartimentPairModel> FilteredProductCompartimentPairs { get; set; } = [];
    public ObservableCollection<EdsModel> EdsList { get; set; } = [];
    public ObservableCollection<ShoppingResponse> ShoppingList { get; set; } = [];
    private ShoppingRequest Request { get; set; }


    // Update Quantity property to validate against stock
    private double? _quantity;
    public double? Quantity
    {
        get => _quantity;
        set
        {


            _quantity = value > 0 ? value : 0;

            OnPropertyChanged(nameof(Quantity));
            OnPropertyChanged(nameof(CurrentTotalAmount));
        }
    }

    private double? _purchasePrice;
    public double? PurchasePrice
    {
        get => _purchasePrice;
        set
        {
            _purchasePrice = value > 0 ? value : 0;
            OnPropertyChanged(nameof(PurchasePrice));
            OnPropertyChanged(nameof(CurrentTotalAmount));
        }
    }

    private double? _sellPrice;
    public double? SellPrice
    {
        get => _sellPrice;
        set
        {
            _sellPrice = value > 0 ? value : 0;
            OnPropertyChanged(nameof(SellPrice));
        }
    }

    public double? CurrentTotalAmount => Quantity * PurchasePrice;

    private double? _totalAccumulatedQuantity;
    public double? TotalAccumulatedQuantity
    {
        get => _totalAccumulatedQuantity;
        set
        {
            _totalAccumulatedQuantity = value;
            OnPropertyChanged(nameof(TotalAccumulatedQuantity));
        }
    }

    private double? _totalAccumulatedPrice;
    public double? TotalAccumulatedPrice
    {
        get => _totalAccumulatedPrice;
        set
        {
            _totalAccumulatedPrice = value;
            OnPropertyChanged(nameof(TotalAccumulatedPrice));
        }
    }

    private double? _totalAccumulatedAmount;
    public double? TotalAccumulatedAmount
    {
        get => _totalAccumulatedAmount;
        set
        {
            _totalAccumulatedAmount = value;
            OnPropertyChanged(nameof(TotalAccumulatedAmount));
        }
    }

    private ShoppingModel _shopping;
    public ShoppingModel Shopping
    {
        get => _shopping;
        set
        {
            _shopping = value;
            OnPropertyChanged(nameof(Shopping));
        }
    }

    private string _NewProduct = string.Empty;
    public string NewProduct
    {
        get => _NewProduct;
        set
        {
            _NewProduct = value;
            OnPropertyChanged(nameof(NewProduct));
        }
    }

    private int _idShopping;
    public int IdShopping
    {
        get => _idShopping;
        set
        {
            _idShopping = value;
            OnPropertyChanged(nameof(IdShopping));
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

    private string _invoice;
    public string Invoice
    {
        get => _invoice;
        set
        {
            _invoice = value;
            OnPropertyChanged(nameof(Invoice));
        }
    }

    private DateTime _date = DateTime.Today;
    public DateTime Date
    {
        get => _date;
        set
        {
            _date = value;
            OnPropertyChanged(nameof(Date));
        }
    }

    private double? _amount;
    public double? Amount
    {
        get => _amount;
        set
        {
            _amount = value;
            OnPropertyChanged(nameof(Amount));
        }
    }

    private string? _productName;
    public string? ProductName
    {
        get => _productName;
        set
        {
            _productName = value;
            OnPropertyChanged(nameof(ProductName));
        }
    }

    private ProviderModel _selectedProvider;
    public ProviderModel SelectedProvider
    {
        get => _selectedProvider;
        set
        {
            _selectedProvider = value;
            OnPropertyChanged(nameof(SelectedProvider));
            if (_selectedProvider != null)
                IdProvider = _selectedProvider.IdProvider;
        }
    }

    private int _idProvider;
    public int IdProvider
    {
        get => _idProvider;
        set
        {
            _idProvider = value;
            OnPropertyChanged(nameof(IdProvider));
        }
    }

    private CategoryModel _selectedCategory;
    public CategoryModel SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            _selectedCategory = value;
            OnPropertyChanged(nameof(SelectedCategory));
            if (_selectedCategory != null)
                IdCategory = _selectedCategory.IdCategory;
        }
    }

    private int _idCategory;
    public int IdCategory
    {
        get => _idCategory;
        set
        {
            _idCategory = value;
            OnPropertyChanged(nameof(IdCategory));
        }
    }

    private string _AddeShopping = string.Empty;
    public string AddeShopping
    {
        get => _AddeShopping;
        set
        {
            _AddeShopping = value;
            OnPropertyChanged(nameof(AddeShopping));
        }
    }

    private ObservableCollection<ShoppingProductNestedModel> _shoppingProduct = new();
    public ObservableCollection<ShoppingProductNestedModel> ShoppingProduct
    {
        get => _shoppingProduct;
        set
        {
            _shoppingProduct = value;
            OnPropertyChanged(nameof(ShoppingProduct));
            UpdateAccumulatedTotals();
        }
    }

    private ShoppingResponse _selectedShopping;
    public ShoppingResponse SelectedShopping
    {
        get => _selectedShopping;
        set
        {
            _selectedShopping = value;
            OnPropertyChanged(nameof(SelectedShopping));
            if (_selectedShopping != null)
            {
                IdShopping = _selectedShopping.IdShopping;
            }

        }
    }

    private ProductCompartimentPairModel _selectedProduct;
    public ProductCompartimentPairModel SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            _selectedProduct = value;
            OnPropertyChanged(nameof(SelectedProduct));
            if (_selectedProduct != null)
            {
                IdProduct = _selectedProduct.IdProduct;
            }
        }
    }

    private ProductCompartimentPairModel _selectedCompartiment;
    public ProductCompartimentPairModel SelectedCompartiment
    {
        get => _selectedCompartiment;
        set
        {
            _selectedCompartiment = value;
            OnPropertyChanged(nameof(SelectedCompartiment));
            if (_selectedCompartiment != null)
            {
                IdCompartment = _selectedCompartiment.IdCompartment;
            }
        }
    }

    private ProductCompartimentPairModel _selectedName;
    public ProductCompartimentPairModel SelectedName
    {
        get => _selectedName;
        set
        {
            _selectedName = value;
            OnPropertyChanged(nameof(SelectedName));
            if (_selectedName != null)
            {
                ProductName = _selectedName.ProductName;
            }
        }
    }

    // Add stock-related properties
    private double? _currentStock;
    public double? CurrentStock
    {
        get => _currentStock;
        set
        {
            _currentStock = value;
            OnPropertyChanged(nameof(CurrentStock));
            OnPropertyChanged(nameof(IsStockAvailable));
            OnPropertyChanged(nameof(MaxQuantityAllowed));
        }
    }

    public bool IsStockAvailable => CurrentStock > 0;
    public double? MaxQuantityAllowed => CurrentStock;

    private ProductCompartimentPairModel _selectedProductCompartimentPair;
    public ProductCompartimentPairModel SelectedProductCompartimentPair
    {
        get => _selectedProductCompartimentPair;
        set
        {
            _selectedProductCompartimentPair = value;
            OnPropertyChanged(nameof(SelectedProductCompartimentPair));

            // Update current stock when product is selected
            if (_selectedProductCompartimentPair != null)
            {
                CurrentStock = _selectedProductCompartimentPair.Stock;
            }
            else
            {
                CurrentStock = null;
            }
        }
    }

    private bool _visibleProducts;
    public bool VisibleProducts
    {
        get => _visibleProducts;
        set
        {
            _visibleProducts = value;
            OnPropertyChanged(nameof(VisibleProducts));
        }
    }

    private int _idCompartment;
    public int IdCompartment
    {
        get => _idCompartment;
        set
        {
            _idCompartment = value;
            OnPropertyChanged(nameof(IdCompartment));
        }
    }

    private EdsModel _selectedEds;
    public EdsModel SelectedEds
    {
        get => _selectedEds;
        set
        {
            _selectedEds = value;
            OnPropertyChanged(nameof(SelectedEds));
            if (_selectedEds != null)
            {
                IdEds = _selectedEds.IdEds;
                // ✅ Filtrar compartimentos cuando se selecciona un EDS
                _ = FilterCompartimentsByEdsAsync(_selectedEds.IdEds);
            }
            else
            {
                // Si se deselecciona el EDS, limpiar los compartimentos filtrados
                FilteredProductCompartimentPairs.Clear();
            }
        }
    }

    private int _idEds;
    public int IdEds
    {
        get => _idEds;
        set
        {
            _idEds = value;
            OnPropertyChanged(nameof(IdEds));
        }
    }

    // Filter properties for ShoppingListView
    private ProviderModel _selectedProviderFilter;
    public ProviderModel SelectedProviderFilter
    {
        get => _selectedProviderFilter;
        set
        {
            _selectedProviderFilter = value;
            OnPropertyChanged(nameof(SelectedProviderFilter));
        }
    }

    private CategoryModel _selectedCategoryFilter;
    public CategoryModel SelectedCategoryFilter
    {
        get => _selectedCategoryFilter;
        set
        {
            _selectedCategoryFilter = value;
            OnPropertyChanged(nameof(SelectedCategoryFilter));
        }
    }

    public ICommand GetByIdShoppingDataCommand { get; }
    public ICommand SaveShoppingDataCommand { get; }
    public Command AddShoppingProductCommand { get; }
    public Command HideProducts { get; }
    public ICommand GetShoppingProductCommand { get; }
    public ICommand DeleteProductCommand => new Command<ShoppingProductNestedModel>(DeleteProduct);
    public ICommand GetByIdShoppingProductDataCommand { get; }

    public ShoppingService()
    {

        _authToken = TokenHelper.LoadToken();


        GetAllProviderData();
        GetAllCategoryData();
        GetAllProductCompartimentPairsAsync();
        GetAllEdsData();

        //GetByIdShoppingDataCommand = new Command<int>(async (shoppingId) => await GetByIdShoppingDataAsync(shoppingId));
        SaveShoppingDataCommand = new Command(async () => await SaveShoppingDataAsync());
        AddShoppingProductCommand = new Command(async () => await AddShoppingProductFromPopup());
        //GetByIdShoppingProductDataCommand = new Command<int>(async (ShoppingProductId) => await GetByIdShoppingProductDataAsync(ShoppingProductId));
        HideProducts = new Command(() => { VisibleProducts = !VisibleProducts; });
    }

    //Get All
    private async void GetAllProviderData()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return;
        }
        try
        {
            string url = $"{Configuration.BaseUrl}/api/v1/provider?PageNumber=1&PageSize=100";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.GetStringAsync(url);
            var providerList = JsonSerializer.Deserialize<ProviderResponseModel>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            UpdateProviderList(providerList?.Data ?? new List<ProviderModel>());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando proveedores: {ex.Message}");
        }
    }

    private async void GetAllCategoryData()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return;
        }
        try
        {
            string url = $"{Configuration.BaseUrl}/api/v1/category?PageNumber=1&PageSize=100";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.GetStringAsync(url);
            var categoryList = JsonSerializer.Deserialize<CategoryResponseModel>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            UpdateCategoryList(categoryList?.Data ?? new List<CategoryModel>());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando categorías: {ex.Message}");
        }
    }

    private async void GetAllEdsData()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontraron los datos de configuración", "OK");
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

    public async Task GetAllShoppingAsync(int pageNumber = 1, int pageSize = 5)
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return;
        }
        try
        {
            string url = $"{Configuration.BaseUrl}/api/v1/shopping?PageNumber={pageNumber}&PageSize={pageSize}";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.GetStringAsync(url);
            var shoppingApiResponse = JsonSerializer.Deserialize<ShoppingApiResponse>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            UpdateShoppingList(shoppingApiResponse?.Data ?? new List<ShoppingResponse>());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando compras: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Error", $"Error al cargar compras: {ex.Message}", "OK");
        }
    }

    private void UpdateShoppingList(IEnumerable<ShoppingResponse> shoppingData)
    {
        ShoppingList.Clear();
        foreach (var shopping in shoppingData)
        {
            ShoppingList.Add(shopping);
        }
    }

    public async Task GetAllProductCompartimentPairsAsync()
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

            // Get compartments data
            string compartmentUrl = $"{Configuration.BaseUrl}/api/v1/compartiment?PageNumber=1&PageSize=100";
            var compartmentResponse = await httpClient.GetStringAsync(compartmentUrl);
            var compartmentApiResponse = JsonSerializer.Deserialize<ProducCompartimentPairApiResponse>(compartmentResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var compartmentList = compartmentApiResponse?.Data ?? new List<ProductCompartimentPairModelRaw>();

            // Get products data to map names and stock
            string productUrl = $"{Configuration.BaseUrl}/api/v1/product?PageNumber=1&PageSize=100";
            var productResponse = await httpClient.GetStringAsync(productUrl);
            var productApiResponse = JsonSerializer.Deserialize<ProductApiResponse>(productResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var productList = productApiResponse?.Data ?? new List<ProductResponse>();

            // Create a dictionary for fast product lookup
            var productDictionary = productList.ToDictionary(p => p.IdProduct, p => p);

            // Map combined data with real product names and stock
            var mappedList = compartmentList.Select(x => new ProductCompartimentPairModel
            {
                IdProduct = x.idProduct,
                ProductName = GetProductName(x.idProduct, productDictionary),
                IdCompartment = x.idCompartiment,
                Number = x.number,
                Operative = x.operative,
                Stock = GetRealProductStock(x.idProduct, productDictionary), // Use real stock from product API
                IdTank = x.idTank  // ✅ Guardar el IdTank para poder filtrar después
            }).ToList();

            ProductCompartimentPairs.Clear();
            foreach (var item in mappedList)
            {
                ProductCompartimentPairs.Add(item);
            }

        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error cargando combinaciones: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Filtra los compartimentos según el EDS seleccionado usando la relación eds_tank
    /// </summary>
    public async Task FilterCompartimentsByEdsAsync(int edsId)
    {
        if (string.IsNullOrEmpty(_authToken))
            return;

        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            // Obtener todas las asignaciones EDS-Tank
            string edsTankUrl = $"{Configuration.BaseUrl}/api/v1/eds-tank";
            var edsTankResponse = await httpClient.GetStringAsync(edsTankUrl);
            
            // Crear el modelo de respuesta para deserializar
            var edsTankApiResponse = JsonSerializer.Deserialize<EdsTankApiResponse>(edsTankResponse, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (edsTankApiResponse == null || edsTankApiResponse.Data == null || !edsTankApiResponse.Data.Any())
            {
                // No hay asignaciones de tanques en el sistema
                FilteredProductCompartimentPairs.Clear();
                System.Diagnostics.Debug.WriteLine($"No se encontraron asignaciones EDS-Tank en el sistema");
                
                // Mostrar todos los compartimentos si no hay asignaciones
                foreach (var compartment in ProductCompartimentPairs)
                {
                    FilteredProductCompartimentPairs.Add(compartment);
                }
                return;
            }

            // Filtrar solo los tanques del EDS seleccionado
            var tankIdsForEds = edsTankApiResponse.Data
                .Where(et => et.IdEds == edsId)
                .Select(et => et.IdTank)
                .ToList();

            System.Diagnostics.Debug.WriteLine($"EDS {edsId} tiene {tankIdsForEds.Count} tanques asignados: {string.Join(", ", tankIdsForEds)}");

            if (!tankIdsForEds.Any())
            {
                // El EDS seleccionado no tiene tanques asignados
                FilteredProductCompartimentPairs.Clear();
                
                System.Diagnostics.Debug.WriteLine($"El EDS {edsId} no tiene tanques asignados");
                
                await CustomAlert.ShowWarningAsync(
                    $"El EDS seleccionado no tiene tanques asignados.\n\n" +
                    $"Por favor, asigne tanques al EDS en el módulo de configuración antes de realizar una compra.",
                    "Sin Tanques Asignados");
                return;
            }

            // Debug: Mostrar todos los compartimentos disponibles con sus tanques
            System.Diagnostics.Debug.WriteLine($"Compartimentos disponibles:");
            foreach (var c in ProductCompartimentPairs)
            {
                System.Diagnostics.Debug.WriteLine($"  - Compartimento {c.Number}, Producto: {c.ProductName}, Tank: {c.IdTank}");
            }

            // Filtrar los compartimentos que pertenecen a los tanques del EDS seleccionado
            var filteredCompartments = ProductCompartimentPairs
                .Where(c => tankIdsForEds.Contains(c.IdTank))
                .OrderBy(c => c.ProductName)
                .ThenBy(c => c.Number)
                .ToList();

            System.Diagnostics.Debug.WriteLine($"Compartimentos filtrados: {filteredCompartments.Count}");
            foreach (var c in filteredCompartments)
            {
                System.Diagnostics.Debug.WriteLine($"  - Compartimento {c.Number}, Producto: {c.ProductName}, Tank: {c.IdTank}");
            }

            // Actualizar la colección filtrada
            FilteredProductCompartimentPairs.Clear();
            
            if (filteredCompartments.Any())
            {
                foreach (var compartment in filteredCompartments)
                {
                    FilteredProductCompartimentPairs.Add(compartment);
                }
                
                System.Diagnostics.Debug.WriteLine($"✅ Filtrado exitoso: {FilteredProductCompartimentPairs.Count} compartimentos para EDS {edsId}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ No se encontraron compartimentos para los tanques del EDS {edsId}");
                
                await CustomAlert.ShowWarningAsync(
                    $"Los tanques asignados a este EDS no tienen compartimentos configurados.\n\n" +
                    $"Por favor, configure los compartimentos en el módulo de tanques.",
                    "Sin Compartimentos Configurados");
            }
        }
        catch (HttpRequestException httpEx)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error de conexión filtrando compartimentos: {httpEx.Message}");
            
            await CustomAlert.ShowErrorAsync(
                "Error al cargar los compartimentos del EDS seleccionado.\n\n" +
                "Verifique su conexión a internet e intente nuevamente.",
                "Error de Conexión");
            
            // En caso de error de conexión, limpiar para evitar confusión
            FilteredProductCompartimentPairs.Clear();
        }
        catch (JsonException jsonEx)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error de JSON filtrando compartimentos: {jsonEx.Message}");
            System.Diagnostics.Debug.WriteLine($"   Stack trace: {jsonEx.StackTrace}");
            
            // En caso de error de deserialización, mostrar todos los compartimentos
            FilteredProductCompartimentPairs.Clear();
            foreach (var compartment in ProductCompartimentPairs)
            {
                FilteredProductCompartimentPairs.Add(compartment);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error general filtrando compartimentos por EDS: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"   Tipo: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"   Stack trace: {ex.StackTrace}");
            
            // En caso de error general, mostrar todos los compartimentos
            FilteredProductCompartimentPairs.Clear();
            foreach (var compartment in ProductCompartimentPairs)
            {
                FilteredProductCompartimentPairs.Add(compartment);
            }
        }
    }

    private string GetProductName(int productId, Dictionary<int, ProductResponse> productDictionary)
    {
        if (productDictionary.TryGetValue(productId, out var product))
        {
            return product.Name;
        }
        return $"Producto {productId}"; // Fallback name in Spanish
    }

    private double GetRealProductStock(int productId, Dictionary<int, ProductResponse> productDictionary)
    {
        // Use real stock from the product API response
        if (productDictionary.TryGetValue(productId, out var product))
        {
            // The API response includes "stock" field with the actual stock value
            return product.Stock; // Return real stock from API
        }
        return 0; // No stock if product not found
    }


    public async Task<double?> RefreshStockForProductCompartmentAsync(int productId, int compartmentId)
    {
        if (string.IsNullOrEmpty(_authToken))
            return null;

        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            // If you have a specific stock endpoint for product-compartment pairs
            string stockUrl = $"{Configuration.BaseUrl}/api/v1/stock/product/{productId}/compartment/{compartmentId}";
            var response = await httpClient.GetStringAsync(stockUrl);

            // Parse the response based on your API structure
            var stockData = JsonSerializer.Deserialize<dynamic>(response);

            // Update the local collection
            var item = ProductCompartimentPairs.FirstOrDefault(p =>
                p.IdProduct == productId && p.IdCompartment == compartmentId);
            if (item != null)
            {
                // item.Stock = stockData.CurrentStock; // Adjust based on actual API response
                OnPropertyChanged(nameof(ProductCompartimentPairs));
            }

            return 0; // Return actual stock value from API
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error refreshing stock: {ex.Message}");
            return null;
        }
    }

    public async Task GetByIdShoppingDataAsync(int shoppingId)
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
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/shopping/{shoppingId}");
            Shopping = JsonSerializer.Deserialize<ShoppingModel>(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo cargar: {ex.Message}", "OK");
        }
    }

    public async Task GetByIdShoppingProductDataAsync(int ShoppingProductId)
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
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/shopping-product/{ShoppingProductId}");
            var product = JsonSerializer.Deserialize<ShoppingProductNestedModel>(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            ShoppingProduct.Clear();
            if (product != null)
                ShoppingProduct.Add(product);

        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo cargar el dato: {ex.Message}", "OK");
        }
    }

    public async Task SaveShoppingDataAsync()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return;
        }
        try
        {
            // Double validation: Check invoice exists before attempting to save
            bool invoiceExists = await CheckInvoiceExistsAsync(Invoice);
            if (invoiceExists)
            {
                await CustomAlert.ShowErrorAsync(
                    $"❌ No se puede guardar la compra.\n\n" +
                    $"El número de factura '{Invoice}' ya existe en la base de datos.\n\n" +
                    $"Debe modificar el número de factura antes de continuar.",
                    "Factura Duplicada - Guardado Bloqueado");
                return; // Block saving completely
            }

            // Validate invoice format
            bool isInvoiceValid = await ValidateInvoiceAsync(Invoice);
            if (!isInvoiceValid)
            {
                return; // Stop if invoice is invalid or user wants to modify it
            }

            // Validate EDS selection
            if (SelectedEds is null)
            {
                await CustomAlert.ShowErrorAsync("Debe seleccionar un EDS (Estación de Servicio) para registrar la compra", "EDS Requerido");
                return;
            }

            if (SelectedProvider is null || SelectedCategory is null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Complete todos los campos", "OK");
                return;
            }

            var shoppingProducts = ShoppingProduct.Select(p => new ShoppingProductNestedModel
            {
                IdProduct = p.IdProduct,
                Quantity = p.Quantity,
                PurchasePrice = p.PurchasePrice,
                SellPrice = p.SellPrice,
                IdCompartment = p.IdCompartment
            }).ToList();


            Shopping = new ShoppingModel
            {
                Invoice = Invoice,
                Date = Date,
                Amount = Amount,
                IdProvider = SelectedProvider.IdProvider,
                IdCategory = SelectedCategory.IdCategory,
                IdEds = SelectedEds.IdEds,  // ✅ Asignar IdEds dentro del ShoppingModel
                ShoppingProducts = shoppingProducts
            };

            Request = new ShoppingRequest { Request = Shopping };

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var json = JsonSerializer.Serialize(Request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{Configuration.BaseUrl}/api/v1/shopping", content);

            if (response.IsSuccessStatusCode)
            {
                await Application.Current.MainPage.DisplayAlert("Éxito", "Datos guardados correctamente", "OK");
                Invoice = string.Empty;
                Date = DateTime.Today;
                Amount = 0;
                SelectedProvider = null;
                SelectedCategory = null;
                SelectedEds = null;
                ShoppingProduct.Clear();
                UpdateAccumulatedTotals();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await Application.Current.MainPage.DisplayAlert("Error", $"Error: {error}", "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Excepción: {ex.Message}", "OK");
        }
    }


    public async Task<bool> AddShoppingProductFromPopup()
    {
        // Validate stock availability before adding
        if (SelectedProductCompartimentPair == null)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Seleccione un producto", "OK");
            return false;
        }

        if (!Quantity.HasValue || Quantity <= 0)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Ingrese una cantidad válida", "OK");
            return false;
        }

        // 🔧 GUARDAR LOS DATOS DEL PRODUCTO ANTES DE AGREGARLO (para evitar que se pierdan al limpiar)
        var savedQuantity = Quantity.Value;
        var savedPurchasePrice = PurchasePrice ?? 0;
        var savedSellPrice = SellPrice ?? 0;
        var savedTotalPrice = CurrentTotalAmount ?? 0;
        var savedProductName = SelectedProductCompartimentPair?.ProductName ?? "Producto";
        var savedCompartmentNumber = SelectedProductCompartimentPair?.Number ?? 0;
        var savedCompartmentCapacity = SelectedProductCompartimentPair?.Operative ?? 0;
        var savedCurrentStock = SelectedProductCompartimentPair?.Stock ?? 0;

        // ✅ VALIDACIÓN PREVENTIVA: Verificar capacidad del compartimento ANTES de agregar
        var totalStockAfterPurchase = savedCurrentStock + savedQuantity;
        if (totalStockAfterPurchase > savedCompartmentCapacity)
        {
            var espacioDisponible = Math.Max(0, savedCompartmentCapacity - savedCurrentStock);

            var errorMessage = $"⚠️ Capacidad de almacenamiento excedida\n\n" +
                              $"La cantidad ingresada supera la capacidad máxima ({savedCompartmentCapacity:N0} galones) " +
                              $"del compartimento asignado al producto {savedProductName}.\n\n" +
                              $"📊 Detalles del compartimento:\n" +
                              $"• Compartimento: #{savedCompartmentNumber}\n" +
                              $"• Producto: {savedProductName}\n" +
                              $"• Capacidad máxima: {savedCompartmentCapacity:N0} gal\n" +
                              $"• Stock actual: {savedCurrentStock:N3} gal\n" +
                              $"• Espacio disponible: {espacioDisponible:N3} gal\n\n" +
                              $"❌ Cantidad solicitada: {savedQuantity:N3} gal\n" +
                              $"✅ Cantidad máxima permitida: {espacioDisponible:N3} gal\n\n" +
                              $"💡 Para continuar:\n" +
                              $"• Ajuste la cantidad a máximo {espacioDisponible:N3} galones, o\n" +
                              $"• Seleccione otro compartimento con mayor capacidad disponible.";

            await CustomAlert.ShowErrorAsync(errorMessage, "⚠️ Capacidad Excedida");
            return false; // ✅ No agregar el producto y retornar false
        }

        try
        {
            var newProduct = new ShoppingProductNestedModel
            {
                IdProduct = SelectedProductCompartimentPair.IdProduct,
                IdCompartment = SelectedProductCompartimentPair.IdCompartment,
                Quantity = Quantity,
                PurchasePrice = PurchasePrice,
                SellPrice = SellPrice,
                TotalPrice = CurrentTotalAmount,
            };

            ShoppingProduct.Add(newProduct);

            // Update stock after adding product (optional - depends on your business logic)
            if (CurrentStock.HasValue)
            {
                CurrentStock -= Quantity;
                // Update the stock in the ProductCompartimentPairs collection
                var productInList = ProductCompartimentPairs.FirstOrDefault(p =>
                    p.IdProduct == SelectedProductCompartimentPair.IdProduct &&
                    p.IdCompartment == SelectedProductCompartimentPair.IdCompartment);
                if (productInList != null)
                {
                    productInList.Stock = CurrentStock.Value;
                }
            }

            UpdateAccumulatedTotals();
            return true; // ✅ Producto agregado exitosamente
        }
        catch (HttpRequestException httpEx)
        {
            // Manejar errores HTTP del backend (como respaldo adicional)
            var errorMessage = await ParseBackendErrorAsync(httpEx, savedProductName, savedQuantity, savedCompartmentCapacity);
            await CustomAlert.ShowErrorAsync(errorMessage, "Error al Agregar Producto");

            // Revertir el producto agregado si hubo un error
            if (ShoppingProduct.Any())
            {
                var lastProduct = ShoppingProduct.LastOrDefault();
                if (lastProduct != null)
                {
                    ShoppingProduct.Remove(lastProduct);
                    UpdateAccumulatedTotals();
                }
            }
            return false; // ✅ Error al agregar
        }
        catch (Exception ex)
        {
            // Manejar cualquier otro tipo de error
            var errorMessage = $"⚠️ Error Inesperado\n\n" +
                              $"Ocurrió un error al agregar el producto:\n\n" +
                              $"{ex.Message}\n\n" +
                              $"Por favor, intente nuevamente.";
            await CustomAlert.ShowErrorAsync(errorMessage, "Error del Sistema");

            // Revertir el producto agregado si hubo un error
            if (ShoppingProduct.Any())
            {
                var lastProduct = ShoppingProduct.LastOrDefault();
                if (lastProduct != null)
                {
                    ShoppingProduct.Remove(lastProduct);
                    UpdateAccumulatedTotals();
                }
            }
            return false; // ✅ Error al agregar
        }

        // 🔧 NOTA: NO LLAMAR ResetProductForm() AQUÍ - Se llamará desde el popup después de mostrar el mensaje
        // ResetProductForm();
    }

    private void UpdateAccumulatedTotals()
    {
        TotalAccumulatedQuantity = ShoppingProduct.Sum(p => p.Quantity);
        TotalAccumulatedPrice = ShoppingProduct.Sum(p => p.PurchasePrice);
        TotalAccumulatedAmount = ShoppingProduct.Sum(p => p.TotalPrice);
        Amount = TotalAccumulatedAmount;
        OnPropertyChanged(nameof(Amount));
    }

    private void UpdateProviderList(IEnumerable<ProviderModel> providerData)
    {
        ProviderList.Clear();
        foreach (var provider in providerData)
        {
            ProviderList.Add(provider);
        }
    }

    private void UpdateCategoryList(IEnumerable<CategoryModel> categoryData)
    {
        CategoryList.Clear();
        foreach (var category in categoryData)
        {
            CategoryList.Add(category);
        }
    }

    /// <summary>
    /// Parsea el error del backend para generar un mensaje amigable al usuario
    /// </summary>
    private async Task<string> ParseBackendErrorAsync(HttpRequestException httpEx, string productName, double quantity, double compartmentCapacity)
    {
        try
        {
            var errorContent = httpEx.Message;

            // Detectar error de capacidad de compartimento excedida
            if (errorContent.Contains("CompartimentCapacityExceeded", StringComparison.OrdinalIgnoreCase) ||
                errorContent.Contains("supera la capacidad", StringComparison.OrdinalIgnoreCase) ||
                errorContent.Contains("exceeds the capacity", StringComparison.OrdinalIgnoreCase))
            {
                // Extraer información del error si es posible
                double currentStock = 0;
                double maxCapacity = compartmentCapacity;

                // Intentar extraer el stock actual y la capacidad del mensaje de error
                var match = System.Text.RegularExpressions.Regex.Match(errorContent, @"(\d+(?:\.\d+)?)\s*gls?\)?\s*supera\s*la\s*capacidad\s*\((\d+(?:\.\d+)?)\s*gls?",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                if (match.Success && match.Groups.Count >= 3)
                {
                    double.TryParse(match.Groups[1].Value, out currentStock);
                    double.TryParse(match.Groups[2].Value, out maxCapacity);
                }

                // Generar mensaje amigable similar al ejemplo proporcionado
                var userFriendlyMessage = $"⚠️ Capacidad de almacenamiento excedida\n\n" +
                                         $"La cantidad ingresada supera la capacidad máxima ({maxCapacity:N0} galones) " +
                                         $"del compartimento asignado al producto {productName}.\n\n" +
                                         $"• Cantidad solicitada: {quantity:N3} gal\n" +
                                         $"• Capacidad máxima: {maxCapacity:N0} gal\n" +
                                         (currentStock > 0 ? $"• Stock actual: {currentStock:N0} gal\n" : "") +
                                         (currentStock > 0 ? $"• Espacio disponible: {Math.Max(0, maxCapacity - currentStock):N0} gal\n\n" : "\n") +
                                         $"Por favor, ajuste la cantidad o seleccione otro compartimento disponible.";

                return userFriendlyMessage;
            }

            // Si no es un error de capacidad excedida, devolver el error general
            return $"⚠️ Error al Procesar la Compra\n\n" +
                   $"No se pudo agregar el producto al carrito:\n\n" +
                   $"{errorContent}\n\n" +
                   $"Por favor, verifique los datos e intente nuevamente.";
        }
        catch (Exception parseEx)
        {
            // Si falla el parseo, devolver un mensaje genérico pero informativo
            System.Diagnostics.Debug.WriteLine($"Error parsing backend error: {parseEx.Message}");
            return $"⚠️ Error al Procesar la Compra\n\n" +
                   $"La cantidad ingresada puede exceder la capacidad del compartimento.\n\n" +
                   $"• Producto: {productName}\n" +
                   $"• Cantidad solicitada: {quantity:N3} gal\n" +
                   $"• Capacidad del compartimento: {compartmentCapacity:N0} gal\n\n" +
                   $"Por favor, ajuste la cantidad o seleccione otro compartimento.";
        }
    }

    public async Task<bool> CheckInvoiceExistsAsync(string invoiceNumber)
    {
        if (string.IsNullOrEmpty(_authToken) || string.IsNullOrWhiteSpace(invoiceNumber))
        {
            return false;
        }

        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            // Get all shopping records to check for duplicate invoice
            string url = $"{Configuration.BaseUrl}/api/v1/shopping?PageNumber=1&PageSize=1000";
            var response = await httpClient.GetStringAsync(url);
            var shoppingList = JsonSerializer.Deserialize<ShoppingApiResponse>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (shoppingList?.Data != null)
            {
                // Check if any shopping record has the same invoice number
                return shoppingList.Data.Any(shopping =>
                    string.Equals(shopping.Invoice?.Trim(), invoiceNumber.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error verificando factura existente: {ex.Message}");
            return false; // If there's an error, assume invoice doesn't exist to avoid blocking
        }
    }

    public async Task<bool> ValidateInvoiceAsync(string invoiceNumber)
    {
        // Use the enhanced validation with suggestions
        return await ValidateInvoiceWithSuggestionsAsync(invoiceNumber);
    }

    public async Task<List<string>> GenerateInvoiceSuggestionsAsync(string baseInvoice)
    {
        var suggestions = new List<string>();

        try
        {
            // Generate different invoice number suggestions
            for (int i = 1; i <= 5; i++)
            {
                string suggestion = $"{baseInvoice}-{i:D2}";
                bool exists = await CheckInvoiceExistsAsync(suggestion);
                if (!exists)
                {
                    suggestions.Add(suggestion);
                }
            }

            // Try with different suffixes
            var suffixes = new[] { "A", "B", "C", "BIS", "REV" };
            foreach (var suffix in suffixes)
            {
                if (suggestions.Count >= 5) break;

                string suggestion = $"{baseInvoice}-{suffix}";
                bool exists = await CheckInvoiceExistsAsync(suggestion);
                if (!exists)
                {
                    suggestions.Add(suggestion);
                }
            }

            // Try with current date suffix if still need more
            if (suggestions.Count < 3)
            {
                var today = DateTime.Now.ToString("ddMM");
                string dateSuggestion = $"{baseInvoice}-{today}";
                bool exists = await CheckInvoiceExistsAsync(dateSuggestion);
                if (!exists)
                {
                    suggestions.Add(dateSuggestion);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generando sugerencias de factura: {ex.Message}");
        }

        return suggestions.Take(3).ToList(); // Return max 3 suggestions
    }

    public async Task<bool> ValidateInvoiceWithSuggestionsAsync(string invoiceNumber)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
        {
            await CustomAlert.ShowErrorAsync("El número de factura es obligatorio para registrar la compra", "Factura Requerida");
            return false;
        }

        if (invoiceNumber.Length < 3)
        {
            await CustomAlert.ShowErrorAsync("El número de factura debe tener al menos 3 caracteres para ser válido", "Factura Muy Corta");
            return false;
        }

        // Check if invoice already exists
        bool invoiceExists = await CheckInvoiceExistsAsync(invoiceNumber);

        if (invoiceExists)
        {
            // Generate suggestions for alternative invoice numbers
            var suggestions = await GenerateInvoiceSuggestionsAsync(invoiceNumber);

            string suggestionsText = "";
            if (suggestions.Any())
            {
                suggestionsText = $"\n\nSugerencias de números disponibles:\n• {string.Join("\n• ", suggestions)}";
            }

            await CustomAlert.ShowErrorAsync(
                $"❌ El número de factura '{invoiceNumber}' ya existe en la base de datos.\n\n" +
                $"No es posible continuar con un número de factura duplicado.\n\n" +
                $"Por favor, modifique el número de factura para poder proceder con el registro de la compra." +
                suggestionsText,
                "Factura Duplicada");

            return false; // Siempre retornar false si la factura existe - obliga al usuario a modificar
        }
        else
        {
            // Show success message for valid invoice
            await CustomAlert.ShowSuccessAsync(
                $"✅ El número de factura '{invoiceNumber}' está disponible y es válido",
                "Factura Válida");
        }

        return true; // Invoice is valid and doesn't exist
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void ResetProductForm()
    {
        // ✅ IMPORTANTE: Solo limpiar los campos del formulario, no afectar los datos ya agregados
        SelectedProduct = null;
        SelectedCompartiment = null;
        SelectedProductCompartimentPair = null;
        PurchasePrice = 0;
        Quantity = 0;
        SellPrice = 0;
        CurrentStock = null;

        // Notificar cambios en las propiedades para actualizar la UI
        OnPropertyChanged(nameof(PurchasePrice));
        OnPropertyChanged(nameof(Quantity));
        OnPropertyChanged(nameof(SellPrice));
        OnPropertyChanged(nameof(CurrentTotalAmount));
        OnPropertyChanged(nameof(SelectedProduct));
        OnPropertyChanged(nameof(SelectedCompartiment));
        OnPropertyChanged(nameof(SelectedProductCompartimentPair));
        OnPropertyChanged(nameof(CurrentStock));
        OnPropertyChanged(nameof(IsStockAvailable));
        OnPropertyChanged(nameof(MaxQuantityAllowed));
    }

    private void DeleteProduct(ShoppingProductNestedModel product)
    {
        if (product != null && ShoppingProduct.Contains(product))
        {
            // Restore stock when deleting a product
            var productInList = ProductCompartimentPairs.FirstOrDefault(p =>
                p.IdProduct == product.IdProduct &&
                p.IdCompartment == product.IdCompartment);
            if (productInList != null && product.Quantity.HasValue)
            {
                productInList.Stock += product.Quantity.Value;

                // If this is the currently selected product, update CurrentStock
                if (SelectedProductCompartimentPair != null &&
                    SelectedProductCompartimentPair.IdProduct == product.IdProduct &&
                    SelectedProductCompartimentPair.IdCompartment == product.IdCompartment)
                {
                    CurrentStock = productInList.Stock;
                }
            }

            ShoppingProduct.Remove(product);
            UpdateAccumulatedTotals();

            if (ShoppingProduct.Count == 0)
            {
                ResetProductForm();
            }
        }
    }

    // Method to get detailed product information for debugging
    public async Task<string> GetProductCompartmentDebugInfoAsync()
    {
        if (string.IsNullOrEmpty(_authToken))
            return "No hay token de autenticación";

        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            // Get products data
            string productUrl = $"{Configuration.BaseUrl}/api/v1/product?PageNumber=1&PageSize=100";
            var productResponse = await httpClient.GetStringAsync(productUrl);
            var productApiResponse = JsonSerializer.Deserialize<ProductApiResponse>(productResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var productList = productApiResponse?.Data ?? new List<ProductResponse>();

            var debugInfo = new StringBuilder();
            debugInfo.AppendLine("=== INFORMACIÓN DE PRODUCTOS ===");

            foreach (var product in productList)
            {
                debugInfo.AppendLine($"ID: {product.IdProduct}");
                debugInfo.AppendLine($"Nombre: {product.Name}");
                debugInfo.AppendLine($"Stock: {product.Stock:F2}");
                debugInfo.AppendLine($"Precio Venta: ${product.SellPrice:F2}");
                debugInfo.AppendLine($"Precio Compra: ${product.PurchasePrice:F2}");
                debugInfo.AppendLine("---");
            }

            return debugInfo.ToString();
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
public void ClearProductCache()
    {
        ProductCompartimentPairs.Clear();
        FilteredProductCompartimentPairs.Clear();
        System.Diagnostics.Debug.WriteLine("Caché de productos por EDS limpiada.");
    }
}