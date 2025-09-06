using System.ComponentModel;
using System.Text.Json;
using System.Text;
using System.Windows.Input;
using System.Collections.ObjectModel;
using APP.Eds.Models.Compartiment;
using APP.Eds.Services.Config;
using APP.Eds.Helpers;
using System.Net.Http.Headers;
using APP.Eds.Models.Translations;
using System.Linq;
using APP.Eds.Components.PopUp;

namespace APP.Eds.Services.Compartiment
{
    public class CompartimentService : INotifyPropertyChanged
    {
        private string? _authToken;
        public event PropertyChangedEventHandler? PropertyChanged;
        public ObservableCollection<TankResponse> TankList { get; set; } = [];
        public ObservableCollection<CompartimentResponse> CompartimentList { get; set; } = [];
        private CompartimentRequest Request { get; set; }


        private CompartimentModel _compartiment;
        public CompartimentModel Compartiment
        {
            get => _compartiment;
            set
            {
                _compartiment = value;
                OnPropertyChanged(nameof(Compartiment));
            }
        }

        private int _number;
        public int Number
        {
            get => _number;
            set
            {
                _number = value;
                OnPropertyChanged(nameof(Number));
            }
        }

        private double _nominal;
        public double Nominal
        {
            get => _nominal;
            set
            {
                _nominal = value;
                OnPropertyChanged(nameof(Nominal));
            }
        }

        private double _operative;
        public double Operative
        {
            get => _operative;
            set
            {
                _operative = value;
                OnPropertyChanged(nameof(Operative));
            }
        }

        private double _stock;
        public double Stock
        {
            get => _stock;
            set
            {
                _stock = value;
                OnPropertyChanged(nameof(Stock));
            }
        }

        private double _height;
        public double Height
        {
            get => _height;
            set
            {
                _height = value;
                OnPropertyChanged(nameof(Height));
            }
        }

        private int _idTank;
        public int IdTank
        {
            get => _idTank;
            set
            {
                _idTank = value;
                OnPropertyChanged(nameof(IdTank));
            }
        }

        private TankResponse _selectedTank;
        public TankResponse SelectedTank
        {
            get => _selectedTank;
            set
            {
                _selectedTank = value;
                OnPropertyChanged(nameof(SelectedTank));
                if (_selectedTank != null)
                {
                    IdTank = _selectedTank.IdTank;
                }
            }
        }

        private string _NumberTranslation = string.Empty;
        public string NumberTranslation
        {
            get => _NumberTranslation;
            set
            {
                if (_NumberTranslation != value)
                {
                    _NumberTranslation = value;
                    OnPropertyChanged(nameof(NumberTranslation));
                }
            }
        }

        private string _CompartimentManagement = string.Empty;
        public string CompartimentManagement
        {
            get => _CompartimentManagement;
            set
            {
                if (_CompartimentManagement != value)
                {
                    _CompartimentManagement = value;
                    OnPropertyChanged(nameof(CompartimentManagement));
                }
            }
        }

        private string _EnterNumber = string.Empty;
        public string EnterNumber
        {
            get => _EnterNumber;
            set
            {
                if (_EnterNumber != value)
                {
                    _EnterNumber = value;
                    OnPropertyChanged(nameof(EnterNumber));
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

        private string _EnterNominal = string.Empty;
        public string EnterNominal
        {
            get => _EnterNominal;
            set
            {
                if (_EnterNominal != value)
                {
                    _EnterNominal = value;
                    OnPropertyChanged(nameof(EnterNominal));
                }
            }
        }

        private string _EnterOperative = string.Empty;
        public string EnterOperative
        {
            get => _EnterOperative;
            set
            {
                if (_EnterOperative != value)
                {
                    _EnterOperative = value;
                    OnPropertyChanged(nameof(EnterOperative));
                }
            }
        }

        private string _EnterStock = string.Empty;
        public string EnterStock
        {
            get => _EnterStock;
            set
            {
                if (_EnterStock != value)
                {
                    _EnterStock = value;
                    OnPropertyChanged(nameof(EnterStock));
                }
            }
        }

        private string _EnterHeight = string.Empty;
        public string EnterHeight
        {
            get => _EnterHeight;
            set
            {
                if (_EnterHeight != value)
                {
                    _EnterHeight = value;
                    OnPropertyChanged(nameof(EnterHeight));
                }
            }
        }

        private string _SelectTank = string.Empty;
        public string SelectTank
        {
            get => _SelectTank;
            set
            {
                if (_SelectTank != value)
                {
                    _SelectTank = value;
                    OnPropertyChanged(nameof(SelectTank));
                }
            }
        }

        private string _ListDispensers = string.Empty;
        public string ListDispensers
        {
            get => _ListDispensers;
            set
            {
                if (_ListDispensers != value)
                {
                    _ListDispensers = value;
                    OnPropertyChanged(nameof(ListDispensers));
                }
            }
        }


        public ICommand GetByIdCompartimentDataCommand { get; }
        public ICommand SaveCompartimentDataCommand { get; }
        public string? NewDispenserNumber { get; internal set; }
        public string? NewDispenserNominal { get; internal set; }

        //ejecutando el metodo
        public CompartimentService()
        {
            _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
            // Se llama a GetAllTankData() en OnAppearing de CompartimentPostView.xaml.cs
            // GetCompartimentAsync(); // Se llama en OnAppearing de CompartimentPostView.xaml.cs
            GetByIdCompartimentDataCommand = new Command<int>(async (CompartimentId) => await GetByIdCompartimentDataAsync(CompartimentId));
            SaveCompartimentDataCommand = new Command(async () => await SaveCompartimentDataAsync());
            LoadTranslationsAsync();

        }

        public async Task LoadTranslationsAsync()
        {
            var result = await GetTranslationsByLanguageAsync("es-CO");
            GlobalTranslations.SetTranslations(result ?? []);
            NumberTranslation = GlobalTranslations.Get("Number");
            CompartimentManagement = GlobalTranslations.Get("CompartimentManagement");
            SendData = GlobalTranslations.Get("SendData");
            EnterNumber = GlobalTranslations.Get("EnterNumber");
            EnterNominal = GlobalTranslations.Get("EnterNominal");
            EnterOperative = GlobalTranslations.Get("EnterOperative");
            EnterStock = GlobalTranslations.Get("EnterStock");
            EnterHeight = GlobalTranslations.Get("EnterHeight");
             SelectTank = GlobalTranslations.Get("SelectTank");
            ListDispensers = GlobalTranslations.Get("ListDispensers");

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

        //Data
        public async Task GetAllTankData()
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                await CustomAlert.ShowErrorAsync("No se encontró el token de autenticación", "Error de Autenticación");
                return;
            }
            try
            {
                string url = $"{Configuration.BaseUrl}/api/v1/tank";
                Console.WriteLine($"Solicitando tanques desde: {url}"); // Debugging: Imprimir la URL completa
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                var httpResponse = await httpClient.GetAsync(url); // Usar GetAsync para verificar el StatusCode
                
                if (httpResponse.IsSuccessStatusCode)
                {
                    var responseContent = await httpResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"Respuesta exitosa de la API de tanques: {responseContent}"); // Debugging
                    var tankList = JsonSerializer.Deserialize<TankApiResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    UpdateTankList(tankList?.Data ?? new List<TankResponse>());
                    Console.WriteLine($"Número de tanques cargados: {TankList.Count}"); // Debugging
                }
                else
                {
                    var errorContent = await httpResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error en la API de tanques - StatusCode: {httpResponse.StatusCode}, Contenido: {errorContent}"); // Debugging
                    await CustomAlert.ShowErrorAsync($"Error al cargar los tanques: {httpResponse.StatusCode} - {errorContent}", "Error de Carga de Tanques");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción al cargar los datos del tanque: {ex.Message}"); // Debugging
                await CustomAlert.ShowErrorAsync($"Excepción al cargar los tanques: {ex.Message}", "Error de Carga de Tanques");
            }
        }

        //List
        private void UpdateTankList(IEnumerable<TankResponse> tank)
        {
            TankList.Clear();
            foreach (var item in tank)
            {
                TankList.Add(item);
            }
        }
        public async Task GetByIdCompartimentDataAsync(int CompartimentId)
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
                var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/compartiment/{CompartimentId}");
                Console.WriteLine(response);

                Compartiment = JsonSerializer.Deserialize<CompartimentModel>(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo cargar el dato: {ex.Message}", "OK");
            }
        }
        public async Task GetCompartimentAsync()
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
                var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/compartiment?PageNumber=1&PageSize=100");
                var compartiments = JsonSerializer.Deserialize<CompartimentApiResponse>(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                CompartimentList.Clear();
                foreach (var compartiment in compartiments.Data)
                {
                    CompartimentList.Add(compartiment);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        //data
        public async Task SaveCompartimentDataAsync()
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                await CustomAlert.ShowErrorAsync("No se encontró el token de autenticación", "Error de Autenticación");
                return;
            }
            try
            {
                
                await GetCompartimentAsync();

                if (Number <= 0)
                {
                    await CustomAlert.ShowErrorAsync("El número debe ser mayor o igual a 1", "Número Inválido");
                    return;
                }

                if (Nominal < 0)
                {
                    await CustomAlert.ShowErrorAsync("El valor nominal no puede ser negativo", "Capacidad Nominal Inválida");
                    return;
                }

                if (Operative < 0)
                {
                    await CustomAlert.ShowErrorAsync("El valor operativo no puede ser negativo", "Capacidad Operativa Inválida");
                    return;
                }

                if (Stock < 0)
                {
                    await CustomAlert.ShowErrorAsync("El stock no puede ser negativo", "Stock Inválido");
                    return;
                }

                if (Height < 0)
                {
                    await CustomAlert.ShowErrorAsync("La altura no puede ser negativa", "Altura Inválida");
                    return;
                }

                if (SelectedTank is null)
                {
                    await CustomAlert.ShowErrorAsync("Por favor, seleccione un tanque", "Tanque Requerido");
                    return;
                }

                // Validar si el compartimiento ya existe para el tanque seleccionado
                var existingCompartment = CompartimentList.FirstOrDefault(c =>
                    c.Number == Number && c.IdTank == SelectedTank.IdTank);
                if (existingCompartment != null)
                {
                    await CustomAlert.ShowErrorAsync("Este compartimiento ya fue registrado para el tanque.", "Compartimento Existente");
                    return;
                }

                // Validar si el tanque ya tiene todos sus compartimientos registrados
                var existingCompartmentsCount = CompartimentList.Count(c => c.IdTank == SelectedTank.IdTank);
                if (existingCompartmentsCount >= SelectedTank.Compartment)
                {
                    await CustomAlert.ShowErrorAsync($"El tanque ya tiene {SelectedTank.Compartment} compartimientos registrados, que es su capacidad máxima. No es posible agregar más.", "Capacidad Máxima Alcanzada");
                    return;
                }

                Compartiment = new CompartimentModel
                {
                    Number = Number,
                    Nominal = Nominal,
                    Operative = Operative,
                    Stock = Stock,
                    Height = Height,
                    IdTank = SelectedTank.IdTank
                };

                Request = new CompartimentRequest
                {
                    Request = Compartiment
                };


                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                var json = JsonSerializer.Serialize(Request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync($"{Configuration.BaseUrl}/api/v1/compartiment", content);

                if (response.IsSuccessStatusCode)
                {
                    await CustomAlert.ShowSuccessAsync("Datos enviados correctamente", "Éxito");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await CustomAlert.ShowErrorAsync($"No se pudo enviar el dato: {response.StatusCode}\n{error}", "Error de Envío");
                }
            }
            catch (Exception ex)
            {
                await CustomAlert.ShowErrorAsync($"Error al enviar los datos: {ex.Message}", "Error del Sistema");
            }
        }

        // Eliminar compartimento
        public async Task<bool> DeleteCompartimentAsync(int idCompartment)
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
                var response = await httpClient.DeleteAsync($"{Configuration.BaseUrl}/api/v1/compartiment/{idCompartment}");
                if (response.IsSuccessStatusCode)
                {
                    await GetCompartimentAsync();
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

        // Actualizar compartimento
        public async Task<bool> UpdateCompartimentAsync(int idCompartment, CompartimentModel model)
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
                return false;
            }
            try
            {
                var request = new CompartimentRequest { Request = model };
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PutAsync($"{Configuration.BaseUrl}/api/v1/compartiment/{idCompartment}", content);
                if (response.IsSuccessStatusCode)
                {
                    await GetCompartimentAsync();
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


        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        internal void AddNewDispenser()
        {
            throw new NotImplementedException();
        }
    }
}
