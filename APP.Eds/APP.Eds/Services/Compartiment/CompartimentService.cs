using System.ComponentModel;
using System.Text.Json;
using System.Text;
using System.Windows.Input;
using System.Collections.ObjectModel;
using APP.Eds.Models.Compartiment;
using APP.Eds.Services.Config;
using APP.Eds.Helpers;
using System.Net.Http.Headers;

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

        public ICommand GetByIdCompartimentDataCommand { get; }
        public ICommand SaveCompartimentDataCommand { get; }

        //ejecutando el metodo
        public CompartimentService()
        {
            _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
            GetAllTankData();
            GetByIdCompartimentDataCommand = new Command<int>(async (CompartimentId) => await GetByIdCompartimentDataAsync(CompartimentId));
            SaveCompartimentDataCommand = new Command(async () => await SaveCompartimentDataAsync());

        }

        //Data
        private async void GetAllTankData()
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
                return;
            }
            try
            {
                string url = $"{Configuration.BaseUrl}/api/v1/tank";
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                var response = await httpClient.GetStringAsync(url);
                var tankList = JsonSerializer.Deserialize<TankApiResponse>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                UpdateTankList(tankList?.Data ?? new List<TankResponse>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando los datos: {ex.Message}");
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
                var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/compartiment");
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
                await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
                return;
            }
            try
            {
                if (Number <= 0)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "El número debe ser mayor o igual a 1", "OK");
                    return;
                }

                if (Nominal < 0)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "El valor nominal no puede ser negativo", "OK");
                    return;
                }

                if (Operative < 0)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "El valor operativo no puede ser negativo", "OK");
                    return;
                }

                if (Stock < 0)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "El stock no puede ser negativo", "OK");
                    return;
                }

                if (Height < 0)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "La altura no puede ser negativa", "OK");
                    return;
                }

                if (SelectedTank is null)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Por favor, seleccione un tanque", "OK");
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
