using APP.Eds.Helpers;
using APP.Eds.Models.Category;
using APP.Eds.Models.Translations;
using APP.Eds.Services.Config;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows.Input;

namespace APP.Eds.Services.Category
{
    public class CategoryService : INotifyPropertyChanged
    {
        private string? _authToken;
        public event PropertyChangedEventHandler? PropertyChanged;
        private CategoryRequest Request { get; set; }
        private CategoryModel _category;
        public CategoryModel Category
        {
            get => _category;
            set
            {
                _category = value;
                OnPropertyChanged(nameof(Category));
            }
        }

        // Default category options
        public ObservableCollection<string> DefaultCategories { get; set; } = new ObservableCollection<string>
        {
            "Subsidiado",
            "Nacional",
            "Personalizado..." // Option to add custom category
        };

        private string _selectedDefaultCategory;
        public string SelectedDefaultCategory
        {
            get => _selectedDefaultCategory;
            set
            {
                _selectedDefaultCategory = value;
                OnPropertyChanged(nameof(SelectedDefaultCategory));
                OnPropertyChanged(nameof(IsCustomCategoryVisible));
                
                // If not "Personalizado", set as description
                if (value != null && value != "Personalizado...")
                {
                    Description = value;
                }
            }
        }

        // Property to control visibility of custom input
        public bool IsCustomCategoryVisible => SelectedDefaultCategory == "Personalizado...";

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
        public ICommand GetByIdCategoryDataCommand { get; }
        public ICommand SaveCategoryDataCommand { get; }


        private string _CategoryTranslation = string.Empty;
        public string CategoryTranslation
        {
            get => _CategoryTranslation;
            set
            {
                if (_CategoryTranslation != value)
                {
                    _CategoryTranslation = value;
                    OnPropertyChanged(nameof(CategoryTranslation));
                }
            }
        }

        private string _SendData = string.Empty;
        public string SendData
        {
            get => _SendData;
            set
            {
                if (_SendData != value)
                {
                    _SendData = value;
                    OnPropertyChanged(nameof(SendData));
                }
            }
        }

        private string _EnterCategory = string.Empty;

        public string EnterCategory
        {
            get => _EnterCategory;
            set
            {
                if (_EnterCategory != value)
                {
                    _EnterCategory = value;
                    OnPropertyChanged(nameof(EnterCategory));
                }
            }
        }    

        private string _CategoryManagement = string.Empty;

        public string CategoryManagement
        {
            get => _CategoryManagement;
            set
            {
                if (_CategoryManagement != value)
                {
                    _CategoryManagement = value;
                    OnPropertyChanged(nameof(CategoryManagement));
                }
            }
        }
        public  CategoryService()
        {
            _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
            GetByIdCategoryDataCommand = new Command<int>(async (CategoryId) => await GetByIdDispenserTypeDataAsync(CategoryId));
            SaveCategoryDataCommand = new Command(async () => await SaveCategoryDataAsync());
            LoadTranslationsAsync();
        }
        public async Task LoadTranslationsAsync()
        {
            var result = await GetTranslationsByLanguageAsync("es-CO");
            GlobalTranslations.SetTranslations(result ?? []);
            CategoryTranslation = GlobalTranslations.Get("Category");
            SendData = GlobalTranslations.Get("SendData");
            EnterCategory = GlobalTranslations.Get("EnterCategory");
            CategoryManagement = GlobalTranslations.Get("CategoryManagement");

        }
        public async Task<Dictionary<string, string>> GetTranslationsByLanguageAsync(string languageTag)
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
                return new Dictionary<string, string>();
            }
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/translations");
            var data = JsonSerializer.Deserialize<TranslationsResponse>(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return data.Translations.TryGetValue(languageTag, out var translations)
                ? translations
                : new Dictionary<string, string>();

        }
        public async Task GetByIdDispenserTypeDataAsync(int CategoryId)
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
                var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/category/{CategoryId}");
                Console.WriteLine(response);

                Category = JsonSerializer.Deserialize<CategoryModel>(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo cargar el dato: {ex.Message}", "OK");
            }
        }

        public async Task SaveCategoryDataAsync()
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
                return;
            }
            try
            {
                // Validate before making the request
                if (string.IsNullOrWhiteSpace(Description))
                {
                    await Application.Current.MainPage.DisplayAlert("Error de Validación", "Debe ingresar los datos en el campo categoría.", "Aceptar");
                    return; 
                }

                Category = new CategoryModel
                {
                    Description = Description
                };

                Request = new CategoryRequest
                {
                    Request = Category
                };

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                var json = JsonSerializer.Serialize(Request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync($"{Configuration.BaseUrl}/api/v1/category", content);

                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("Éxito", "Categoría guardada correctamente", "OK");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo guardar la categoría: {response.StatusCode}\n{error}", "OK");
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
