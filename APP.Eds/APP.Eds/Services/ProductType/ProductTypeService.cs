using APP.Eds.Models.ProductType;
using System.ComponentModel;
using System.Text.Json;
using System.Text;
using System.Windows.Input;
using APP.Eds.Services.Config;
using APP.Eds.Helpers;
using System.Net.Http.Headers;

namespace APP.Eds.Services.ProductType
{
    public class ProductTypeService : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private ProductTypeRequest Request { get; set; }
        private ProductTypeModel _productType;
        private string? _authToken;
        public ProductTypeModel ProductType
        {
                get => _productType;
                set
                {
                _productType = value;
                OnPropertyChanged(nameof(ProductType));
            }
        }

       
        private string _description;
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }
        public ICommand GetByIdProductTypeDataCommand { get; }
        public ICommand SaveProductTypeDataCommand { get; }

        public ProductTypeService()
        {
            GetByIdProductTypeDataCommand = new Command<int>(async (productTypeId) => await GetByIdProductTypeDataAsync(productTypeId));
            SaveProductTypeDataCommand = new Command(async () => await SaveProductTypeDataAsync());
            _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
        }

        public async Task GetByIdProductTypeDataAsync(int productTypeId)
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
                var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/producttype{ productTypeId}");
                Console.WriteLine(response);

                ProductType = JsonSerializer.Deserialize<ProductTypeModel>(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo cargar el dato: {ex.Message}", "OK");
            }
        }

        public async Task SaveProductTypeDataAsync()
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
                return;
            }
            try
            {
                ProductType = new ProductTypeModel
                {
                    Description = Description
                };

                Request = new ProductTypeRequest
                {
                    Request = ProductType
                };

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                var json = JsonSerializer.Serialize(Request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync($"{Configuration.BaseUrl}/api/v1/producttype", content);

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


        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
