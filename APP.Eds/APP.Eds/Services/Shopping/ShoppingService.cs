using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows.Input;
using System.Xml.Linq;
using APP.Eds.Helpers;
using APP.Eds.Models.Shopping;
using APP.Eds.Models.ShoppingProduct;
using APP.Eds.Services.Config;

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

    public ICommand GetByIdShoppingDataCommand { get; }
    public ICommand SaveShoppingDataCommand { get; }
    public Command AddShoppingProductCommand { get; }
    public Command HideProducts { get; }
    public ICommand GetShoppingProductCommand { get; }
    public ICommand DeleteProductCommand => new Command<ShoppingProductNestedModel>(DeleteProduct);
    public ICommand GetByIdShoppingProductDataCommand { get; }

    public ShoppingService()
    {

        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);


        GetAllProviderData();
        GetAllCategoryData();
        GetAllProductCompartimentPairsAsync();

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


    public async Task AddShoppingProductFromPopup()
    {
        // Validate stock availability before adding
        if (SelectedProductCompartimentPair == null)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Seleccione un producto", "OK");
            return;
        }

        if (!Quantity.HasValue || Quantity <= 0)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Ingrese una cantidad válida", "OK");
            return;
        }

       

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
        ResetProductForm();
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

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void ResetProductForm()
    {
        SelectedProduct = null;
        SelectedCompartiment = null;
        SelectedProductCompartimentPair = null;
        PurchasePrice = 0;
        Quantity = 0;
        SellPrice = 0;
        CurrentStock = null;
        OnPropertyChanged(nameof(PurchasePrice));
        OnPropertyChanged(nameof(Quantity));
        OnPropertyChanged(nameof(CurrentTotalAmount));
        OnPropertyChanged(nameof(SelectedProduct));
        OnPropertyChanged(nameof(SelectedCompartiment));
        OnPropertyChanged(nameof(SelectedProductCompartimentPair));
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
}