using APP.Eds.Helpers;
using APP.Eds.Models.Capacity;
using APP.Eds.Models.Compartiment;
using APP.Eds.Models.CompartimentCapacity;
using APP.Eds.Models.EdsTank;
using APP.Eds.Models.Islander;
using APP.Eds.Models.Product;
using APP.Eds.Models.ProductCompartiment;
using APP.Eds.Services.Config;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows.Input;

namespace APP.Eds.Services.CompartimentCapacity;

public class CompartimentCapacityService : INotifyPropertyChanged
{
    private string? _authToken;
    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<CapacityModelResponse> CapacityList { get; set; } = [];
    public ObservableCollection<CompartimentModelResponse> CompartimentList { get; set; } = [];
    private CompartimentCapacityRequest Request { get; set; }

    private CompartimentCapacityModel _compartimentCapacity;
    public CompartimentCapacityModel CompartimentCapacityModel

    {
        get => _compartimentCapacity;
        set
        {
            _compartimentCapacity = value;
            OnPropertyChanged(nameof(CompartimentCapacityModel));
        }
    }

    //SelectCapacity

    private int _idIdCapacity;
    public int IdIdCapacity
    {
        get => _idIdCapacity;
        set
        {
            if (_idIdCapacity != value)
            {
                _idIdCapacity = value;
                OnPropertyChanged(nameof(IdIdCapacity));

            }
        }
    }

    private CapacityModelResponse _selectedCapacity;
    public CapacityModelResponse SelectCapacity
    {
        get => _selectedCapacity;
        set
        {
            _selectedCapacity = value;
            OnPropertyChanged(nameof(SelectCapacity));
            if (_selectedCapacity != null)
            {
                IdIdCapacity = _selectedCapacity.IdCapacity;
            }
        }
    }

    //SelectCompartiment

    private int _idIdCompartiment;
    public int IdIdCompartiment
    {
        get => _idIdCompartiment;
        set
        {
            _idIdCompartiment = value;
            OnPropertyChanged(nameof(IdIdCompartiment));
        }
    }

    private CompartimentModelResponse _selectedCompartiment;
    public CompartimentModelResponse SelectCompartiment
    {
        get => _selectedCompartiment;
        set
        {
            _selectedCompartiment = value;
            OnPropertyChanged(nameof(SelectCompartiment));
            if (_selectedCompartiment != null)
            {
                IdIdCompartiment = _selectedCompartiment.IdCompartiment;
            }
        }
    }
    //AgregaDefault

    private byte _default;
    public byte Default
    {
        get => _default;
        set
        {
            _default = value;
            OnPropertyChanged(nameof(Default));
        }
    }

    public CompartimentCapacityService()
    {
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
        GetAllCapacityData();
        GetByIdCompartimentCapacityDataCommand = new Command<int>(async (compartimentCapacityId) => await GetByIdCompartimentCapacityDataAsync(compartimentCapacityId));
        SaveCompartimentCapacityDataCommand = new Command(async () => await SaveCompartimentCapacityDataAsync());

        GetAllCompartimentData();
        GetByIdCompartimentCapacityDataCommand = new Command<int>(async (compartimentCapacityId) => await GetByIdCompartimentCapacityDataAsync(compartimentCapacityId));
        SaveCompartimentCapacityDataCommand = new Command(async () => await SaveCompartimentCapacityDataAsync());
    }
    //GuardaCapacity
    private async void GetAllCapacityData()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return;
        }

        try
        {
            string url = $"{Configuration.BaseUrl}/api/v1/capacity";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.GetStringAsync(url);
            var CapacityList = JsonSerializer.Deserialize<CapacityResponse>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            UpdateCapacityList(CapacityList.Data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando los datos: {ex.Message}");
        }
    }

    private void UpdateCapacityList(IEnumerable<CapacityModelResponse> Data)
    {
        CapacityList.Clear();
        foreach (var eds in Data)
        {
            CapacityList.Add(eds);
        }
    }

    //GuardaCompartiment

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
            var CompartimentList = JsonSerializer.Deserialize<compartimentResponse>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            UpdateCompartimentList(CompartimentList.Data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando los datos: {ex.Message}");
        }
    }

    private void UpdateCompartimentList(IEnumerable<CompartimentModelResponse> Data)
    {
        CompartimentList.Clear();
        foreach (var eds in Data)
        {
            CompartimentList.Add(eds);
        }
    }
    public async Task GetByIdCompartimentCapacityDataAsync(int compartimentCapacityId)
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
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/compartiment-capacity{ compartimentCapacityId}");
            Console.WriteLine(response);

            CompartimentCapacityModel = JsonSerializer.Deserialize<CompartimentCapacityModel>(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo cargar el dato: {ex.Message}", "OK");
        }
    }

    public async Task SaveCompartimentCapacityDataAsync()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return;
        }
        try
        {
            if (SelectCapacity is null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Por favor, seleccione la Capacity del Tank", "OK");
                return;
            }

            if (SelectCompartiment is null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Por favor, seleccione un Compartiment", "OK");
                return;
            }

            if (Default <= 0)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Por favor, ingrese un valor en Default", "OK");
                return;
            }

            CompartimentCapacityModel = new CompartimentCapacityModel
            {
                IdCapacity = SelectCapacity.IdCapacity,
                IdCompartiment = SelectCompartiment.Number,
                Default = Default
            };

            Request = new CompartimentCapacityRequest
            {
                Request = CompartimentCapacityModel
            };

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var json = JsonSerializer.Serialize(Request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{Configuration.BaseUrl}/api/v1/compartiment-capacity", content);

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


    public ICommand GetByIdCompartimentCapacityDataCommand { get; }
    public ICommand SaveCompartimentCapacityDataCommand { get; }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
