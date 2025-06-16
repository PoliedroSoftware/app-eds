using APP.Eds.Helpers;
using APP.Eds.Models.Product;
using APP.Eds.Services.Config;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows.Input;

namespace APP.Eds.Services.Product;

public class ProductService : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<ProductTypeModelResponse> ProductTypeList { get; set; } = [];
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
        }
    }

    private int _idIdProductType;
    public int IdIdProductType
    {
        get => _idIdProductType;
        set
        {
            _idIdProductType = value;
            OnPropertyChanged(nameof(IdIdProductType));
        }
    }

    private ProductTypeModelResponse _selectedProductType;
    public ProductTypeModelResponse SelectProductType
    {
        get => _selectedProductType;
        set
        {
            _selectedProductType = value;
            OnPropertyChanged(nameof(SelectProductType));
            if (_selectedProductType != null)
            {
                IdIdProductType = _selectedProductType.IdProductType;
            }
        }
    }


    private double _price;
    public double Price
    {
        get => _price;
        set
        {
            _price = value;
            OnPropertyChanged(nameof(Price));
        }
    }

    public ProductService()
    {
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);

        GetAllProductTypeData();
        GetByIdProductDataCommand = new Command<int>(async (productId) => await GetByIdProductDataAsync(productId));
        SaveProductDataCommand = new Command(async () => await SaveProductDataAsync());
        
    }

    private async void GetAllProductTypeData()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return;
        }
        try
        {
            string url = $"{Configuration.BaseUrl}/api/v1/producttype";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.GetStringAsync(url);
            var ProductTypeList = JsonSerializer.Deserialize<ProductTypeResponse>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            UpdateProducTypeList(ProductTypeList.Data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando los datos: {ex.Message}");
        }
    }

    private void UpdateProducTypeList(IEnumerable<ProductTypeModelResponse> Data)
    {
        ProductTypeList.Clear();
        foreach (var eds in Data)
        {
            ProductTypeList.Add(eds);
        }
    }
    public async Task GetByIdProductDataAsync(int productId)
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
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/product{ productId}");
            Console.WriteLine(response);

            ProductModel = JsonSerializer.Deserialize<ProductModel>(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo cargar el dato: {ex.Message}", "OK");
        }
    }

    public async Task SaveProductDataAsync()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return;
        }
        try
        {
            if (SelectProductType is null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Por favor, seleccione un Product Type", "OK");
                return;
            }

           
            ProductModel = new ProductModel
            {
               Name = Name,
               IdProductType = SelectProductType.IdProductType,
               Price = Price
            };

            Request = new ProductRequest
            {
               Request = ProductModel
            };

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var json = JsonSerializer.Serialize(Request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{Configuration.BaseUrl}/api/v1/product", content);

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

   
    public ICommand GetByIdProductDataCommand { get; }
    public ICommand SaveProductDataCommand { get; }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
