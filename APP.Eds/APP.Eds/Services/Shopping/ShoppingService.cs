using APP.Eds.Models.Shopping;
using System.ComponentModel;
using System.Text.Json;
using System.Text;
using System.Windows.Input;
using System.Collections.ObjectModel;
using APP.Eds.Services.Config;
using APP.Eds.Models.ShoppingProduct;
using System.Linq;
using APP.Eds.Helpers;
using System.Net.Http.Headers;

namespace APP.Eds.Services.Shopping;

public class ShoppingService : INotifyPropertyChanged
{
    private static ShoppingService _instance;
    private string? _authToken;
    public static ShoppingService Instance => _instance ??= new ShoppingService();

    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<ProviderModel> ProviderList { get; set; } = [];
    public ObservableCollection<CategoryModel> CategoryList { get; set; } = [];
    public ObservableCollection<ShoppingProductResponse> ShoppingProductList { get; set; } = [];
    public ObservableCollection<ProductResponse> ProductList { get; set; } = [];

    private ShoppingRequest Request { get; set; }

    // Current product being edited
    private double _quantity;
    public double Quantity
    {
        get => _quantity;
        set
        {
            _quantity = value > 0 ? value : 0;
            OnPropertyChanged(nameof(Quantity));
            OnPropertyChanged(nameof(CurrentTotalPrice));
        }
    }

    private double _price;
    public double Price
    {
        get => _price;
        set
        {
            _price = value > 0 ? value : 0;
            OnPropertyChanged(nameof(Price));
            OnPropertyChanged(nameof(CurrentTotalPrice));
        }
    }

    public double CurrentTotalPrice => Quantity * Price;

    // Accumulated totals for all products
    private double _totalAccumulatedQuantity;
    public double TotalAccumulatedQuantity
    {
        get => _totalAccumulatedQuantity;
        set
        {
            _totalAccumulatedQuantity = value;
            OnPropertyChanged(nameof(TotalAccumulatedQuantity));
        }
    }

    private double _totalAccumulatedPrice;
    public double TotalAccumulatedPrice
    {
        get => _totalAccumulatedPrice;
        set
        {
            _totalAccumulatedPrice = value;
            OnPropertyChanged(nameof(TotalAccumulatedPrice));
        }
    }

    private double _totalAccumulatedAmount;
    public double TotalAccumulatedAmount
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

    private double _amount;
    public double Amount
    {
        get => _amount;
        set
        {
            _amount = value;
            OnPropertyChanged(nameof(Amount));
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

    private ObservableCollection<ShoppingProductModel> _shoppingProduct = new();
    public ObservableCollection<ShoppingProductModel> ShoppingProduct
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
                IdShopping = _selectedShopping.IdShopping;
        }
    }

    private ProductResponse _selectedProduct;
    public ProductResponse SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            _selectedProduct = value;
            OnPropertyChanged(nameof(SelectedProduct));
            if (_selectedProduct != null)
                IdProduct = _selectedProduct.IdProduct;
        }
    }

    private bool _VisibleReceipts;
    public bool VisibleReceipts
    {
        get => _VisibleReceipts;
        set
        {
            _VisibleReceipts = value;
            OnPropertyChanged(nameof(VisibleReceipts));
        }
    }

    public ICommand GetByIdShoppingDataCommand { get; }
    public ICommand SaveShoppingDataCommand { get; }
    public Command AddShoppingProductCommand { get; }
    public Command HideProductList { get; }
    public ICommand GetShoppingProductCommand { get; }
    public ICommand DeleteProductCommand => new Command<ShoppingProductModel>(DeleteProduct);

    public ShoppingService()
    {

        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);

        GetAllProviderData();
        GetAllCategoryData();
        GetByIdShoppingDataCommand = new Command<int>(async (shoppingId) => await GetByIdShoppingDataAsync(shoppingId));
        SaveShoppingDataCommand = new Command(async () => await SaveShoppingDataAsync());
        AddShoppingProductCommand = new Command(async () => await AddShoppingProductFromPopup());
        HideProductList = new Command(() => VisibleReceipts = !VisibleReceipts);
        GetAllProductData();
        GetShoppingProductCommand = new Command(async () => await GetShoppingProductAsync());
       
    }

    private void UpdateAccumulatedTotals()
    {
        TotalAccumulatedQuantity = ShoppingProduct.Sum(p => p.Quantity);
        TotalAccumulatedPrice = ShoppingProduct.Sum(p => p.Price);
        TotalAccumulatedAmount = ShoppingProduct.Sum(p => p.TotalPrice);
        Amount = TotalAccumulatedAmount;
        OnPropertyChanged(nameof(Amount));
    }

    private async void GetAllProviderData()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return;
        }
        try
        {
            string url = $"{Configuration.BaseUrl}/api/v1/provider";
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
            string url = $"{Configuration.BaseUrl}/api/v1/category";
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

            Shopping = new ShoppingModel
            {
                Invoice = Invoice,
                Date = Date,
                Amount = Amount,
                IdProvider = SelectedProvider.IdProvider,
                IdCategory = SelectedCategory.IdCategory
            };

            Request = new ShoppingRequest { Request = Shopping };

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var json = JsonSerializer.Serialize(Request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{Configuration.BaseUrl}/api/v1/shopping", content);

            if (!response.IsSuccessStatusCode)
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

    public async Task AddShoppingProductFromPopup()
    {
        if (SelectedProduct == null || Quantity <= 0 || Price <= 0)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Complete todos los campos", "OK");
            return;
        }

        var newProduct = new ShoppingProductModel
        {
            IdProduct = SelectedProduct.IdProduct,
            Quantity = Quantity,
            Price = Price,
            Name = SelectedProduct.Name,
            TotalPrice = CurrentTotalPrice
        };

        ShoppingProduct.Add(newProduct);
        UpdateAccumulatedTotals();

        SelectedProduct = null;
        OnPropertyChanged(nameof(SelectedProduct));
    }

    private async void GetAllProductData()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return;
        }
        try
        {
            string url = $"{Configuration.BaseUrl}/api/v1/product";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.GetStringAsync(url);
            var productList = JsonSerializer.Deserialize<ProductApiResponse>(response,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            UpdateProductList(productList?.Data ?? new List<ProductResponse>());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando productos: {ex.Message}");
        }
    }

    private void UpdateProductList(IEnumerable<ProductResponse> products)
    {
        ProductList.Clear();
        foreach (var product in products)
        {
            ProductList.Add(product);
        }
    }

    public async Task GetShoppingProductAsync()
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
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/shopping-product");
            var shoppingProduct = JsonSerializer.Deserialize<ShoppingProductApiResponse>(response,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            ShoppingProductList.Clear();
            foreach (var item in shoppingProduct.Data)
            {
                ShoppingProductList.Add(item);
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error: {ex.Message}", "OK");
        }
    }

    public void ResetProductForm()
    {
        SelectedProduct = null;
        Price = 0;
        Quantity = 0;
        OnPropertyChanged(nameof(Price));
        OnPropertyChanged(nameof(Quantity));
        OnPropertyChanged(nameof(CurrentTotalPrice));
    }

    private void DeleteProduct(ShoppingProductModel product)
    {
        if (product != null && ShoppingProduct.Contains(product))
        {
            ShoppingProduct.Remove(product);
            UpdateAccumulatedTotals();

            if (ShoppingProduct.Count == 0)
            {
                ResetProductForm();
            }
        }
    }
}