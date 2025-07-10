using APP.Eds.Models.TypeOfCollection;
using System.ComponentModel;
using System.Text.Json;
using System.Text;
using System.Windows.Input;
using APP.Eds.Services.Config;
using APP.Eds.Helpers;
using System.Net.Http.Headers;
using APP.Eds.Models.Translations;


namespace APP.Eds.Services.TypeOfCollection
{
    public class TypeOfCollectionService : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private TypeOfCollectionRequest Request { get; set; }
        private TypeOfCollectionModel _typeOfCollection;
        private string? _authToken;
        public TypeOfCollectionModel TypeOfCollection
        {
                get => _typeOfCollection;
                set
                {
                _typeOfCollection = value;
                OnPropertyChanged(nameof(TypeOfCollection));
            }
        }

        private string _descriptionTitle;
        public string DescriptionTitle
        {
            get => _descriptionTitle;
            set
            {
                _descriptionTitle = value;
                OnPropertyChanged(nameof(DescriptionTitle));
            }
        }

        private string _descriptionPlaceHolder;
        public string DescriptionPlaceHolder
        {
            get => _descriptionPlaceHolder;
            set
            {
                _descriptionPlaceHolder = value;
                OnPropertyChanged(nameof(DescriptionPlaceHolder));
            }
        }

        private string _descriptionLabel;
        public string DescriptionLabel
        {
            get => _descriptionLabel;
            set
            {
                _descriptionLabel = value;
                OnPropertyChanged(nameof(DescriptionLabel));
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

        private string _sendData;
        public string SendData
        {
            get => _sendData;
            set
            {
                _sendData = value;
                OnPropertyChanged(nameof(SendData));
            }
        }

        private string _errorEmpty;
        public string ErrorEmpty
        {
            get => _errorEmpty;
            set
            {
                _errorEmpty = value;
                OnPropertyChanged(nameof(ErrorEmpty));
            }
        }

        private string _errorCharacteres;
        public string ErrorCharacteres
        {
            get => _errorCharacteres;
            set
            {
                _errorCharacteres = value;
                OnPropertyChanged(nameof(_errorCharacteres));
            }
        }

        public async Task<Dictionary<string, string>> GetTranslationsByLanguageAsync(string languageTag)
        {
            _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
            if (string.IsNullOrWhiteSpace(_authToken))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Authentication token is missing", "OK");
                return new Dictionary<string, string>();
            }

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/translations");
            var data = JsonSerializer.Deserialize<TranslationsResponse>(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return data.Translations.TryGetValue(languageTag, out var translations) ? translations
                : new Dictionary<string, string>();
        }

        public ICommand GetByIdTypeOfCollectionDataCommand { get; }
        public ICommand SaveTypeOfCollectionDataCommand { get; }

        public TypeOfCollectionService()
        {
            GetByIdTypeOfCollectionDataCommand = new Command<int>(async (typeOfCollectionId) => await GetByIdTypeOfCollectionDataAsync(typeOfCollectionId));
            SaveTypeOfCollectionDataCommand = new Command(async () => await SaveTypeOfCollectionDataAsync());
            _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
            LoadTranslationsAsync();
        }

        public async Task LoadTranslationsAsync()
        {
            var result = await GetTranslationsByLanguageAsync("es-CO");
            GlobalTranslations.SetTranslations(result ?? []);
            DescriptionTitle = GlobalTranslations.Get("DescriptionTitle");
            DescriptionLabel = GlobalTranslations.Get("DescriptionLabel");
            DescriptionPlaceHolder = GlobalTranslations.Get("DescriptionPlaceHolder");
            SendData = GlobalTranslations.Get("SendData");
            ErrorEmpty = GlobalTranslations.Get("ErrorEmpty");
            ErrorCharacteres = GlobalTranslations.Get("ErrorCharacteres");
        }

        public async Task GetByIdTypeOfCollectionDataAsync(int typeOfCollectionId)
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
                var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/type-of-collection{ typeOfCollectionId}");
                Console.WriteLine(response);

                TypeOfCollection = JsonSerializer.Deserialize<TypeOfCollectionModel>(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo cargar el dato: {ex.Message}", "OK");
            }
        }

        public async Task SaveTypeOfCollectionDataAsync()
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
                return;
            }
            try
            {
                TypeOfCollection = new TypeOfCollectionModel
                {
                    Description = Description
                };

                Request = new TypeOfCollectionRequest
                {
                    Request = TypeOfCollection
                };

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                var json = JsonSerializer.Serialize(Request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync($"{Configuration.BaseUrl}/api/v1/type-of-collection", content);

                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("Éxito", "Datos enviados correctamente", "OK");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await Application.Current.MainPage.DisplayAlert(GlobalTranslations.Get("Error"), 
                        $"{GlobalTranslations.Get("ErrorSendData")}, {response.StatusCode}\n{error}", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(GlobalTranslations.Get("Error"), 
                    $"{GlobalTranslations.Get("ErrorSendData")}, {ex.Message}", "OK");
            }
        }


        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
