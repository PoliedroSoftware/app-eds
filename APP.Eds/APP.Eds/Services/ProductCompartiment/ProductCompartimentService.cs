using APP.Eds.Helpers;
using APP.Eds.Models.Compartiment;
using APP.Eds.Models.EdsTank;
using APP.Eds.Models.Islander;
using APP.Eds.Models.Product;
using APP.Eds.Models.ProductCompartiment;
using APP.Eds.Services.Config;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows.Input;

namespace APP.Eds.Services.ProductCompartiment;

public class ProductCompartimentService : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private string? _authToken;
    public ObservableCollection<ProductModelResponse> ProductList { get; set; } = [];
    public ObservableCollection<CompartimentModelResponse> CompartimentList { get; set; } = [];
    private ProductCompartimentRequest Request { get; set; }

    private ProductCompartimentModel _productCompartiment;
    public ProductCompartimentModel ProductCompartimentModel

    {
        get => _productCompartiment;
        set
        {
            _productCompartiment = value;
            OnPropertyChanged(nameof(ProductCompartimentModel));
        }
    }

    //SelectProduct

    private int _idIdProduct;
    public int IdIdProduct
    {
        get => _idIdProduct;
        set
        {
            _idIdProduct = value;
            OnPropertyChanged(nameof(IdIdProduct));
        }
    }

    private ProductModelResponse _selectedProduct;
    public ProductModelResponse SelectProduct
    {
        get => _selectedProduct;
        set
        {
            _selectedProduct = value;
            OnPropertyChanged(nameof(SelectProduct));
            if (_selectedProduct != null)
            {
                IdIdProduct = _selectedProduct.IdProduct;
            }
        }
    }

    //SelectCompartiment

    private int _idIdCompartiment;
    public int IdIdCompartiment
    {
        get => _idIdCompartiment;
        set
        {
            _idIdCompartiment = value;
            OnPropertyChanged(nameof(IdIdCompartiment));
        }
    }

    private CompartimentModelResponse _selectedCompartiment;
    public CompartimentModelResponse SelectCompartiment
    {
        get => _selectedCompartiment;
        set
        {
            _selectedCompartiment = value;
            OnPropertyChanged(nameof(SelectCompartiment));
            if (_selectedCompartiment != null)
            {
                IdIdCompartiment = _selectedCompartiment.IdCompartiment;
            }
        }
    }
    //AgregaStock

    private double _stock;
    public double Stock
    {
        get => _stock;
        set
        {
            _stock = value;
            OnPropertyChanged(nameof(Stock));
        }
    }

    public ProductCompartimentService()
    {
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);

        GetAllProductData();
        GetByIdProductCompartimentDataCommand = new Command<int>(async (productCompartimentId) => await GetByIdProductCompartimentDataAsync(productCompartimentId));
        SaveProductCompartimentDataCommand = new Command(async () => await SaveProductCompartimentDataAsync());

        GetAllCompartimentData();
        GetByIdProductCompartimentDataCommand = new Command<int>(async (productCompartimentId) => await GetByIdProductCompartimentDataAsync(productCompartimentId));
        SaveProductCompartimentDataCommand = new Command(async () => await SaveProductCompartimentDataAsync());

        
    }
    //GuardaProduct
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
            var ProductList = JsonSerializer.Deserialize<ProductResponse>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            UpdateProductList(ProductList.Data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando los datos: {ex.Message}");
        }
    }

    private void UpdateProductList(IEnumerable<ProductModelResponse> Data)
    {
        ProductList.Clear();
        foreach (var eds in Data)
        {
            ProductList.Add(eds);
        }
    }

    //GuardaCompartiment

    private async void GetAllCompartimentData()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return;
        }
        try
        {
            string url = $"{Configuration.BaseUrl}/api/v1/compartiment";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.GetStringAsync(url);
            var CompartimentList = JsonSerializer.Deserialize<compartimentResponse>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            UpdateCompartimentList(CompartimentList.Data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando los datos: {ex.Message}");
        }
    }

    private void UpdateCompartimentList(IEnumerable<CompartimentModelResponse> Data)
    {
        CompartimentList.Clear();
        foreach (var eds in Data)
        {
            CompartimentList.Add(eds);
        }
    }
    public async Task GetByIdProductCompartimentDataAsync(int productCompartimentId)
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
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/product-compartiment{productCompartimentId}");
            Console.WriteLine(response);

            ProductCompartimentModel = JsonSerializer.Deserialize<ProductCompartimentModel>(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo cargar el dato: {ex.Message}", "OK");
        }
    }

    public async Task SaveProductCompartimentDataAsync()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return;
        }
        try
        {
            if (SelectProduct is null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Por favor, seleccione Un Product", "OK");
                return;
            }

            if (SelectCompartiment is null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Por favor, seleccione un Compartiment", "OK");
                return;
            }

            if (Stock <= 0)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Por favor, ingrese un valor en Stock", "OK");
                return;
            }

            ProductCompartimentModel = new ProductCompartimentModel
            {
                IdProduct = SelectProduct.IdProduct,
                IdCompartiment = SelectCompartiment.Number,
                Stock = Stock
            };

            Request = new ProductCompartimentRequest
            {
                Request = ProductCompartimentModel
            };

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var json = JsonSerializer.Serialize(Request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{Configuration.BaseUrl}/api/v1/product-compartiment", content);

            if (response.IsSuccessStatusCode)
            {
                await Application.Current.MainPage.DisplayAlert("Éxito", "Datos enviados correctamente", "OK");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo enviar el dato: {response.StatusCode}\n{error}", "OK");
            }
        }

        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error al enviar los datos: {ex.Message}", "OK");
        }
    }


    public ICommand GetByIdProductCompartimentDataCommand { get; }
    public ICommand SaveProductCompartimentDataCommand { get; }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
