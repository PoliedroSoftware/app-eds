using APP.Eds.Models.ProductType;
using System.ComponentModel;
using System.Text.Json;
using System.Text;
using System.Windows.Input;
using APP.Eds.Models.Dispensers;
using System.Collections.ObjectModel;
using APP.Eds.Models.ShoppingProduct;
using APP.Eds.Services.Config;
using APP.Eds.Helpers;
using System.Net.Http.Headers;

namespace APP.Eds.Services.ShoppingProduct
{
    public class ShoppingProductService : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public ObservableCollection<ShoppingResponse> ShoppingList { get; set; } = [];
        public ObservableCollection<ProductResponse> ProductList { get; set; } = [];
        public ObservableCollection<CompartimentResponse> CompartimentList { get; set; } = [];
        public ObservableCollection<ShoppingProductResponse> ShoppingProductList { get; set; } = [];
        private ShoppingProductRequest Request { get; set; }
        private ShoppingProductModel _shoppingProduct;
        private string? _authToken;
        public ShoppingProductModel ShoppingProduct
        {
            get => _shoppingProduct;
            set
            {
                _shoppingProduct = value;
                OnPropertyChanged(nameof(ShoppingProduct));
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

        private double _quantity;
        public double Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
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

        // Selects
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

        private ProductResponse _selectedProduct;
        public ProductResponse SelectedProduct
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

        private CompartimentResponse _selectedCompartiment;
        public CompartimentResponse SelectedCompartiment
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


        public ICommand GetByIdShoppingProductDataCommand { get; }
        public ICommand SaveShoppingProductDataCommand { get; }

        //ejecutando el metodo
        public ShoppingProductService()
        {
            _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);

            GetAllShoppingData();
            GetByIdShoppingProductDataCommand = new Command<int>(async (ShoppingProductId) => await GetByIdShoppingProductDataAsync(ShoppingProductId));
            SaveShoppingProductDataCommand = new Command(async () => await SaveShoppingProductDataAsync());

            GetAllProductData();
            GetByIdShoppingProductDataCommand = new Command<int>(async (ShoppingProductId) => await GetByIdShoppingProductDataAsync(ShoppingProductId));
            SaveShoppingProductDataCommand = new Command(async () => await SaveShoppingProductDataAsync());

            GetAllCompartimentData();
            GetByIdShoppingProductDataCommand = new Command<int>(async (ShoppingProductId) => await GetByIdShoppingProductDataAsync(ShoppingProductId));
            SaveShoppingProductDataCommand = new Command(async () => await SaveShoppingProductDataAsync());

            

        }

        //Data
        private async void GetAllShoppingData()
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
                return;
            }
            try
            {
                string url = $"{Configuration.BaseUrl}/api/v1/shopping";
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                var response = await httpClient.GetStringAsync(url);
                var shoppingList = JsonSerializer.Deserialize<ShoppingApiResponse>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                UpdateShoppingList(shoppingList?.Data ?? new List<ShoppingResponse>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando los datos: {ex.Message}");
            }
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
                var productList = JsonSerializer.Deserialize<ProductApiResponse>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                UpdateProductList(productList?.Data ?? new List<ProductResponse>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando los datos: {ex.Message}");
            }
        }

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
                var compartimentList = JsonSerializer.Deserialize<CompartimentApiResponse>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                UpdateCompartimentList(compartimentList?.Data ?? new List<CompartimentResponse>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando los datos: {ex.Message}");
            }
        }

        //List
        private void UpdateShoppingList(IEnumerable<ShoppingResponse> Shopping)
        {
            ShoppingList.Clear();
            foreach (var shopping in Shopping)
            {
                ShoppingList.Add(shopping);
            }
        }

        private void UpdateProductList(IEnumerable<ProductResponse> Product)
        {
            ProductList.Clear();
            foreach (var product in Product)
            {
                ProductList.Add(product);
            }
        }

        private void UpdateCompartimentList(IEnumerable<CompartimentResponse> Island)
        {
            CompartimentList.Clear();
            foreach (var island in Island)
            {
                CompartimentList.Add(island);
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
                ShoppingProduct = JsonSerializer.Deserialize<ShoppingProductModel>(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo cargar el dato: {ex.Message}", "OK");
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
                var shoppingProduct = JsonSerializer.Deserialize<ShoppingProductApiResponse>(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                ShoppingProductList.Clear();
                foreach (var ShoppingProduct in shoppingProduct.Data)
                {
                    ShoppingProductList.Add(ShoppingProduct);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }


        //data
        public async Task SaveShoppingProductDataAsync()
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
                return;
            }
            try
            {
                if (SelectedShopping is null)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Por favor, seleccione una compra", "OK");
                    return;
                }

                if (SelectedProduct is null)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Por favor, seleccione un producto", "OK");
                    return;
                }

                if (Quantity <= 0)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "La cantidad debe ser mayor a 0", "OK");
                    return;
                }

                if (Price <= 0)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "El precio debe ser mayor a 0", "OK");
                    return;
                }

                if (SelectedCompartiment is null)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Por favor, seleccione un compartimento", "OK");
                    return;
                }
                ShoppingProduct = new ShoppingProductModel
                {
                    IdShopping = SelectedShopping.IdShopping,
                    IdProduct = SelectedProduct.IdProduct,
                    Quantity = Quantity,
                    Price = Price,
                    IdCompartment = SelectedCompartiment.IdCompartment,
                };

                Request = new ShoppingProductRequest
                {
                    Request = ShoppingProduct
                };


                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                var json = JsonSerializer.Serialize(Request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync($"{Configuration.BaseUrl}/api/v1/shopping-product", content);

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
