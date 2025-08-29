using APP.Eds.Models.ProductType;
using System.ComponentModel;
using System.Text.Json;
using System.Text;
using System.Windows.Input;
using APP.Eds.Models.DispenserType;
using APP.Eds.Services.Config;
using APP.Eds.Helpers;
using System.Net.Http.Headers;

namespace APP.Eds.Services.DispenserType
{
    public class DispenserTypeService : INotifyPropertyChanged
    {
        private string? _authToken;
        public event PropertyChangedEventHandler? PropertyChanged;
        private DispenserTypeRequest Request { get; set; }
        private DispenserTypeModel _dispenserType;
            public DispenserTypeModel DispenserType
        {
                get => _dispenserType;
                set
                {
                _dispenserType = value;
                OnPropertyChanged(nameof(DispenserType));
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
        public ICommand GetByIdDispenserTypeDataCommand { get; }
        public ICommand SaveDispenserTypeDataCommand { get; }
        public int Id { get; set; }

        public DispenserTypeService()
        {
            _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
            GetByIdDispenserTypeDataCommand = new Command<int>(async (dispenserTypeId) => await GetByIdDispenserTypeDataAsync(dispenserTypeId));
            SaveDispenserTypeDataCommand = new Command(async () => await SaveDispenserTypeDataAsync());
        }

        public async Task<List<DispenserTypeModel>> GetDispenserTypesAsync()
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
                return new List<DispenserTypeModel>();
            }
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/dispenser-type");
                return JsonSerializer.Deserialize<List<DispenserTypeModel>>(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo cargar la lista: {ex.Message}", "OK");
                return new List<DispenserTypeModel>();
            }
        }

        public async Task GetByIdDispenserTypeDataAsync(int DispenserTypeId)
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
                var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/dispenser-type/{DispenserTypeId}");
                Console.WriteLine(response);

                DispenserType = JsonSerializer.Deserialize<DispenserTypeModel>(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo cargar el dato: {ex.Message}", "OK");
            }
        }

        public async Task SaveDispenserTypeDataAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(_authToken))
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
                    return;
                }

                // Enhanced validation
                if (string.IsNullOrWhiteSpace(Description))
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Por favor, ingrese la descripción del tipo de dispensador", "OK");
                    return;
                }

                if (Description.Length < 3)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "La descripción debe tener al menos 3 caracteres", "OK");
                    return;
                }

                DispenserType = new DispenserTypeModel
                {
                    Description = Description
                };

                Request = new DispenserTypeRequest
                {
                    Request = DispenserType
                };

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                var json = JsonSerializer.Serialize(Request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response;

                if (Id == 0)
                {
                    response = await httpClient.PostAsync($"{Configuration.BaseUrl}/api/v1/dispenser-type", content);
                }
                else
                {
                    response = await httpClient.PutAsync($"{Configuration.BaseUrl}/api/v1/dispenser-type/{Id}", content);
                    Id = 0;
                }


                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("Éxito", 
                        $"Tipo de dispensador '{Description}' registrado correctamente", "OK");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await Application.Current.MainPage.DisplayAlert("Error", 
                        $"No se pudo registrar el tipo de dispensador: {response.StatusCode}\n{error}", "OK");
                }
            }
            catch (HttpRequestException httpEx)
            {
                System.Diagnostics.Debug.WriteLine($"HTTP error saving dispenser type: {httpEx.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", 
                    "Error de conexión. Verifique su conexión a internet e intente nuevamente.", "OK");
            }
            catch (JsonException jsonEx)
            {
                System.Diagnostics.Debug.WriteLine($"JSON error saving dispenser type: {jsonEx.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", 
                    "Error procesando la respuesta del servidor.", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"General error saving dispenser type: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", 
                    $"Error inesperado al registrar el tipo de dispensador: {ex.Message}", "OK");
            }
        }

        public async Task DeleteDispenserTypeAsync(int dispenserTypeId)
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
                var response = await httpClient.DeleteAsync($"{Configuration.BaseUrl}/api/v1/dispenser-type/{dispenserTypeId}");

                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("Éxito", "Tipo de dispensador eliminado correctamente", "OK");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo eliminar el tipo de dispensador: {response.StatusCode}\n{error}", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Error al eliminar el tipo de dispensador: {ex.Message}", "OK");
            }
        }


        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
