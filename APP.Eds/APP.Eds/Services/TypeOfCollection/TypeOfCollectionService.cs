using APP.Eds.Models.TypeOfCollection;
using System.ComponentModel;
using System.Text.Json;
using System.Text;
using System.Windows.Input;
using APP.Eds.Services.Config;
using APP.Eds.Helpers;
using System.Net.Http.Headers;

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
        public ICommand GetByIdTypeOfCollectionDataCommand { get; }
        public ICommand SaveTypeOfCollectionDataCommand { get; }

        public TypeOfCollectionService()
        {
            GetByIdTypeOfCollectionDataCommand = new Command<int>(async (typeOfCollectionId) => await GetByIdTypeOfCollectionDataAsync(typeOfCollectionId));
            SaveTypeOfCollectionDataCommand = new Command(async () => await SaveTypeOfCollectionDataAsync());
            _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
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
