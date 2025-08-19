using APP.Eds.Models.ProductType;
using APP.Eds.Models.Product;
using System.ComponentModel;
using System.Text.Json;
using System.Text;
using System.Windows.Input;
using APP.Eds.Services.Config;
using APP.Eds.Helpers;
using System.Net.Http.Headers;
using System.Collections.ObjectModel;

namespace APP.Eds.Services.ProductType
{
    public class ProductTypeService : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private ProductTypeRequest Request { get; set; }
        private ProductTypeModel _productType;
        private string? _authToken;
        
        // Collection to hold all available product types
        public ObservableCollection<ProductTypeModelResponse> ProductTypeList { get; set; } = new();
        
        // Default product types as per requirements
        private readonly string[] DefaultProductTypes = { "Corriente", "Extra", "Acpm" };
        
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
        
        private ProductTypeModelResponse _selectedProductType;
        public ProductTypeModelResponse SelectedProductType
        {
            get => _selectedProductType;
            set
            {
                _selectedProductType = value;
                if (_selectedProductType != null)
                {
                    Description = _selectedProductType.Description;
                }
                OnPropertyChanged(nameof(SelectedProductType));
            }
        }
        
        private bool _isAddingNew;
        public bool IsAddingNew
        {
            get => _isAddingNew;
            set
            {
                _isAddingNew = value;
                OnPropertyChanged(nameof(IsAddingNew));
                OnPropertyChanged(nameof(IsNotAddingNew));
            }
        }
        
        public bool IsNotAddingNew => !_isAddingNew;
        
        public ICommand GetByIdProductTypeDataCommand { get; }
        public ICommand SaveProductTypeDataCommand { get; }
        public ICommand AddNewProductTypeCommand { get; }
        public ICommand CancelAddNewCommand { get; }

        public ProductTypeService()
        {
            GetByIdProductTypeDataCommand = new Command<int>(async (productTypeId) => await GetByIdProductTypeDataAsync(productTypeId));
            SaveProductTypeDataCommand = new Command(async () => await SaveProductTypeDataAsync());
            AddNewProductTypeCommand = new Command(() => SetAddNewMode(true));
            CancelAddNewCommand = new Command(() => SetAddNewMode(false));
            _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
            
            // Load existing product types and ensure defaults exist
            _ = Task.Run(async () => await InitializeProductTypesAsync());
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
            
            // If we're not adding new and have selected an existing type
            if (!IsAddingNew && SelectedProductType != null)
            {
                // If it's an offline type (negative ID), we need to create it first
                if (SelectedProductType.IdProductType < 0)
                {
                    await CreateProductTypeFromOfflineSelection();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Éxito", $"Tipo de producto '{SelectedProductType.Description}' seleccionado correctamente", "OK");
                }
                return;
            }
            
            // Validate that we have a description for new product types
            if (string.IsNullOrWhiteSpace(Description))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Por favor ingrese una descripción para el tipo de producto", "OK");
                return;
            }
            
            // Check for duplicates
            if (ProductTypeList.Any(pt => string.Equals(pt.Description.Trim(), Description.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Este tipo de producto ya existe. Por favor selecciónelo de la lista o ingrese un nombre diferente.", "OK");
                return;
            }
            
            try
            {
                ProductType = new ProductTypeModel
                {
                    Description = Description.Trim()
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
                    
                    // Refresh the product types list to include the new one
                    await GetAllProductTypesAsync();
                    
                    // Reset the form
                    SetAddNewMode(false);
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
        
        private async Task CreateProductTypeFromOfflineSelection()
        {
            try
            {
                var productType = new ProductTypeModel { Description = SelectedProductType.Description.Trim() };
                var request = new ProductTypeRequest { Request = productType };

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync($"{Configuration.BaseUrl}/api/v1/producttype", content);

                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("Éxito", $"Tipo de producto '{SelectedProductType.Description}' creado y seleccionado correctamente", "OK");
                    // Refresh the list to get the real ID from server
                    await GetAllProductTypesAsync();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo crear el tipo de producto: {response.StatusCode}\n{error}", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Error al crear el tipo de producto: {ex.Message}", "OK");
            }
        }

        private async Task InitializeProductTypesAsync()
        {
            try
            {
                await GetAllProductTypesAsync();
                await EnsureDefaultProductTypesExistAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during initialization: {ex.Message}");
                // If initialization fails, add default types to the local list for offline functionality
                AddOfflineDefaultTypes();
            }
        }
        
        private void AddOfflineDefaultTypes()
        {
            var offlineDefaults = DefaultProductTypes.Select((desc, index) => new ProductTypeModelResponse
            {
                IdProductType = -(index + 1), // Use negative IDs for offline entries
                Description = desc
            });
            
            foreach (var defaultType in offlineDefaults)
            {
                ProductTypeList.Add(defaultType);
            }
        }
        
        public async Task GetAllProductTypesAsync()
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
                var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/producttype");
                var productTypeResponse = JsonSerializer.Deserialize<ProductTypeResponse>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                UpdateProductTypeList(productTypeResponse.Data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading product types: {ex.Message}");
            }
        }
        
        private void UpdateProductTypeList(IEnumerable<ProductTypeModelResponse> data)
        {
            ProductTypeList.Clear();
            foreach (var productType in data)
            {
                ProductTypeList.Add(productType);
            }
        }
        
        private async Task EnsureDefaultProductTypesExistAsync()
        {
            try
            {
                foreach (var defaultType in DefaultProductTypes)
                {
                    if (!ProductTypeList.Any(pt => string.Equals(pt.Description.Trim(), defaultType.Trim(), StringComparison.OrdinalIgnoreCase)))
                    {
                        await CreateDefaultProductTypeAsync(defaultType);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error ensuring default product types: {ex.Message}");
            }
        }
        
        private async Task CreateDefaultProductTypeAsync(string description)
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                return;
            }
            
            try
            {
                var productType = new ProductTypeModel { Description = description };
                var request = new ProductTypeRequest { Request = productType };

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync($"{Configuration.BaseUrl}/api/v1/producttype", content);

                if (response.IsSuccessStatusCode)
                {
                    // Refresh the list to include the newly created default type
                    await GetAllProductTypesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating default product type '{description}': {ex.Message}");
            }
        }
        
        private void SetAddNewMode(bool isAddingNew)
        {
            IsAddingNew = isAddingNew;
            if (isAddingNew)
            {
                Description = string.Empty;
                SelectedProductType = null;
            }
            else
            {
                Description = string.Empty;
            }
        }


        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
