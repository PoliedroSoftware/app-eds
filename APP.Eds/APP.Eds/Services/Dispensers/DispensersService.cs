using APP.Eds.Models.ProductType;
using System.ComponentModel;
using System.Text.Json;
using System.Text;
using System.Windows.Input;
using APP.Eds.Models.Dispensers;
using System.Collections.ObjectModel;
using APP.Eds.Models.Dispenser;
using APP.Eds.Models.Hose;
using APP.Eds.Services.Config;
using APP.Eds.Helpers;
using System.Net.Http.Headers;
using APP.Eds.Models.Island;

namespace APP.Eds.Services.Dispensers
{
    public class DispensersService : INotifyPropertyChanged
    {
        private string? _authToken;
        public event PropertyChangedEventHandler? PropertyChanged;
        public ObservableCollection<DisperserTypeResponse> DispenserTypeList { get; set; } = [];
        public ObservableCollection<EdsModel> EdsList { get; set; } = [];
        public ObservableCollection<IslandResponse> IslandList { get; set; } = [];
        public ObservableCollection<DispenserModelResponse> DispensersList { get; set; } = [];
        private DispensersRequest Request { get; set; }
        private DispensersModel _dispensers;
        public DispensersModel Dispensers
        {
            get => _dispensers;
            set
            {
                _dispensers = value;
                OnPropertyChanged(nameof(Dispensers));
            }
        }


        private string _code;
        public string Code
        {
            get => _code;
            set
            {
                _code = value;
                OnPropertyChanged(nameof(Code));
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

        private int _dispenserTypeId;
        public int DispenserTypeId
        {
            get => _dispenserTypeId;
            set
            {
                _dispenserTypeId = value;
                OnPropertyChanged(nameof(DispenserTypeId));
            }
        }

        private int _edsId;
        public int EdsId
        {
            get => _edsId;
            set
            {
                _edsId = value;
                OnPropertyChanged(nameof(EdsId));
            }
        }

        private int _idIsland;
        public int IdIsland
        {
            get => _idIsland;
            set
            {
                _idIsland = value;
                OnPropertyChanged(nameof(IdIsland));
            }
        }

        private int _hoseNumber;
        public int HoseNumber
        {
            get => _hoseNumber;
            set
            {
                _hoseNumber = value;
                OnPropertyChanged(nameof(HoseNumber));
            }
        }

        //selects 
        private DisperserTypeResponse _selectedDispenserType;
        public DisperserTypeResponse SelectedDispenserType
        {
            get => _selectedDispenserType;
            set
            {
                _selectedDispenserType = value;
                OnPropertyChanged(nameof(SelectedDispenserType));
                if (_selectedDispenserType != null)
                {
                    DispenserTypeId = _selectedDispenserType.IdType;
                }
            }
        }

        private EdsModel _selectedEds;
        public EdsModel SelectedEds
        {
            get => _selectedEds;
            set
            {
                _selectedEds = value;
                OnPropertyChanged(nameof(SelectedEds));
                if (_selectedEds != null)
                {
                    EdsId = _selectedEds.IdEds;
                }
            }
        }

        private IslandResponse _selectedIsland;
        public IslandResponse SelectedIsland
        {
            get => _selectedIsland;
            set
            {
                _selectedIsland = value;
                OnPropertyChanged(nameof(SelectedIsland));
                if (_selectedIsland != null)
                {
                    Description = _selectedIsland.Description;
                }
            }
        }

        public ICommand GetByIdDispensersDataCommand { get; }
        public ICommand SaveDispensersDataCommand { get; }

        //ejecutando el metodo
        public DispensersService()
        {
            _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
            GetAllDispenserTypeData();
            GetAllIslandData();
            GetAllEdsData();
            GetByIdDispensersDataCommand = new Command<int>(async (DispensersId) => await GetByIdDispensersDataAsync(DispensersId));
            SaveDispensersDataCommand = new Command(async () => await SaveDispensersDataAsync());
            
        }

        //Data
        private async void GetAllDispenserTypeData()
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
                return;
            }
            try
            {
                string url = $"{Configuration.BaseUrl}/api/v1/dispenser-type";
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                var response = await httpClient.GetStringAsync(url);
                var dispenserTypeList = JsonSerializer.Deserialize<DispenserTypeResponseApi>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                UpdateDispenserTypeList(dispenserTypeList?.Data ?? new List<DisperserTypeResponse>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando los datos: {ex.Message}");
            }
        }

        private async void GetAllEdsData()
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
                return;
            }
            try
            {
                string url = $"{Configuration.BaseUrl}/api/v1/eds";
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                var response = await httpClient.GetStringAsync(url);
                var edsList = JsonSerializer.Deserialize<EdsResponseModel>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                UpdateEdsList(edsList?.Data ?? new List<EdsModel>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando los datos: {ex.Message}");
            }
        }

        private async void GetAllIslandData()
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
                return;
            }
            try
            {
                string url = $"{Configuration.BaseUrl}/api/v1/island";
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                var response = await httpClient.GetStringAsync(url);
                var IslandList = JsonSerializer.Deserialize<IslandApiResponse>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                UpdateIslandList(IslandList?.Data ?? new List<IslandResponse>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando los datos: {ex.Message}");
            }
        }

        //List
        private void UpdateDispenserTypeList(IEnumerable<DisperserTypeResponse> disperserTypes)
        {
            DispenserTypeList.Clear();
            foreach (var item in disperserTypes)
            {
                DispenserTypeList.Add(item);
            }
        }

        private void UpdateEdsList(IEnumerable<EdsModel> edsData)
        {
            EdsList.Clear();
            foreach (var eds in edsData)
            {
                EdsList.Add(eds);
            }
        }

        private void UpdateIslandList(IEnumerable<IslandResponse> Island)
        {
            IslandList.Clear();
            foreach (var item in Island)
            {
                IslandList.Add(item);
            }
        }
        public async Task GetByIdDispensersDataAsync(int DispensersId)
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
                var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/dispensers/{DispensersId}");
                Console.WriteLine(response);

                Dispensers = JsonSerializer.Deserialize<DispensersModel>(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo cargar el dato: {ex.Message}", "OK");
            }
        }
        public async Task GetDispensersAsync()
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
                var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/dispensers");
                var dispenserss = JsonSerializer.Deserialize<DispensersResponseModel>(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                DispensersList.Clear();
                foreach (var dispensers in dispenserss.Data)
                {
                    DispensersList.Add(dispensers);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        //data
        public async Task SaveDispensersDataAsync()
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
                return;
            }
            try
            {
                if (string.IsNullOrWhiteSpace(Code))
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "El campo 'Código' no puede estar vacío", "OK");
                    return;
                }

                if (Number <= 0)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "El número debe ser mayor o igual a 1", "OK");
                    return;
                }

                if (SelectedDispenserType is null)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Por favor, seleccione un tipo de dispensador", "OK");
                    return;
                }

                if (HoseNumber <= 0)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "El número de manguera debe ser mayor o igual a 0", "OK");
                    return;
                }

                if (SelectedEds is null)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Por favor, seleccione un EDS", "OK");
                    return;
                }

                if (SelectedIsland is null)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Por favor, seleccione un Islander", "OK");
                    return;
                }
                Dispensers = new DispensersModel
                {
                    Code = Code,
                    Number = Number,
                    DispenserTypeId = SelectedDispenserType.IdType,
                    HoseNumber = HoseNumber,
                    EdsId = SelectedEds.IdEds,
                    IdIsland = SelectedIsland.Idisland,
                    
                };

                Request = new DispensersRequest
                {
                    Request = Dispensers
                };


                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                var json = JsonSerializer.Serialize(Request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync($"{Configuration.BaseUrl}/api/v1/dispensers", content);

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
                await Application.Current.MainPage.DisplayAlert("Error", $"Error al enviar los datos, Status: {ex.Message}", "OK");
            }
        }


        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
