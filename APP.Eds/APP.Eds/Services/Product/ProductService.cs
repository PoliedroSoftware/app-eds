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
                new ProductTypeModelResponse { IdProductType = 1, Description = "Combustibles" },
                new ProductTypeModelResponse { IdProductType = 2, Description = "Lubricantes" },
                new ProductTypeModelResponse { IdProductType = 3, Description = "Aditivos" },
                new ProductTypeModelResponse { IdProductType = 4, Description = "Servicios" }
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
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return;
        }
        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/product{productId}");
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
            // Enhanced validation
            if (string.IsNullOrWhiteSpace(Name))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Por favor, ingrese el nombre del producto", "OK");
                return;
            }

            if (SelectProductType is null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Por favor, seleccione un tipo de producto", "OK");
                return;
            }

            if (Price <= 0)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Por favor, ingrese un precio válido mayor que 0", "OK");
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
                await Application.Current.MainPage.DisplayAlert("Éxito", 
                    $"Producto '{Name}' registrado correctamente con precio ${Price:F2}", "OK");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await Application.Current.MainPage.DisplayAlert("Error", 
                    $"No se pudo registrar el producto: {response.StatusCode}\n{error}", "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", 
                $"Error al registrar el producto: {ex.Message}", "OK");
        }
    }

    // Delete product
    public async Task<bool> DeleteProductAsync(int idProduct)
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
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
                var error = await response.Content.ReadAsStringAsync();
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo eliminar: {response.StatusCode}\n{error}", "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error al eliminar: {ex.Message}", "OK");
        }
        return false;
    }

    // Update product
    public async Task<bool> UpdateProductAsync(int idProduct, ProductModel model)
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
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
                var error = await response.Content.ReadAsStringAsync();
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo actualizar: {response.StatusCode}\n{error}", "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error al actualizar: {ex.Message}", "OK");
        }
        return false;
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

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
