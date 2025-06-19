using APP.Eds.Helpers;
using APP.Eds.Models.Category;
using APP.Eds.Models.Translations;
using APP.Eds.Services.Config;
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


                if (string.IsNullOrWhiteSpace(Description))
                {
                    await Application.Current.MainPage.DisplayAlert("Error de Validación", "Debe ingresar los datos en el campo categoria.", "Aceptar");
                    return; 
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
