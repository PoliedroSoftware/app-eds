using APP.Eds.Helpers;
using APP.Eds.Models.CompartimentCapacity;
using APP.Eds.Models.EdsTank;
using APP.Eds.Models.Islander;
using APP.Eds.Models.Product;
using APP.Eds.Models.ProductCompartiment;
using APP.Eds.Services.Config;
using APP.Eds.Components.PopUp;
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

    private byte? _default;
    public byte? Default
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
        _authToken = TokenHelper.LoadToken();
        GetAllCapacityData();
        GetByIdCompartimentCapacityDataCommand = new Command<int>(async (compartimentCapacityId) => await GetByIdCompartimentCapacityDataAsync(compartimentCapacityId));
        SaveCompartimentCapacityDataCommand = new Command(async () => await SaveCompartimentCapacityDataAsync());

        GetAllCompartimentData();
        GetByIdCompartimentCapacityDataCommand = new Command<int>(async (compartimentCapacityId) => await GetByIdCompartimentCapacityDataAsync(compartimentCapacityId));
        SaveCompartimentCapacityDataCommand = new Command(async () => await SaveCompartimentCapacityDataAsync());
    }
    //GuardaCapacity
    public async Task GetAllCapacityDataAsync()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await CustomAlert.ShowErrorAsync("No se encontró el token de autenticación", "Error de Autenticación");
            return;
        }

        try
        {
            string url = $"{Configuration.BaseUrl}/api/v1/capacity";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.GetStringAsync(url);
            var capacityResponse = JsonSerializer.Deserialize<CapacityResponse>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            UpdateCapacityList(capacityResponse?.Data ?? new List<CapacityModelResponse>());
        }
        catch (HttpRequestException httpEx)
        {
            System.Diagnostics.Debug.WriteLine($"HTTP error loading capacities: {httpEx.Message}");
            await CustomAlert.ShowErrorAsync("Error de conexión. Verifique su conexión a internet e intente nuevamente.", "Error de Conexión");
        }
        catch (JsonException jsonEx)
        {
            System.Diagnostics.Debug.WriteLine($"JSON error loading capacities: {jsonEx.Message}");
            await CustomAlert.ShowErrorAsync("Error al procesar los datos del servidor.", "Error de Datos");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"General error loading capacities: {ex.Message}");
            await CustomAlert.ShowErrorAsync($"Error cargando los tanques:\n\n{ex.Message}", "Error del Sistema");
        }
    }

    private async void GetAllCapacityData()
    {
        await GetAllCapacityDataAsync();
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
    public async Task GetAllCompartimentDataAsync()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await CustomAlert.ShowErrorAsync("No se encontró el token de autenticación", "Error de Autenticación");
            return;
        }
        try
        {
            string url = $"{Configuration.BaseUrl}/api/v1/compartiment";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.GetStringAsync(url);
            var compartimentResponse = JsonSerializer.Deserialize<compartimentResponse>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            UpdateCompartimentList(compartimentResponse?.Data ?? new List<CompartimentModelResponse>());
        }
        catch (HttpRequestException httpEx)
        {
            System.Diagnostics.Debug.WriteLine($"HTTP error loading compartments: {httpEx.Message}");
            await CustomAlert.ShowErrorAsync("Error de conexión. Verifique su conexión a internet e intente nuevamente.", "Error de Conexión");
        }
        catch (JsonException jsonEx)
        {
            System.Diagnostics.Debug.WriteLine($"JSON error loading compartments: {jsonEx.Message}");
            await CustomAlert.ShowErrorAsync("Error al procesar los datos del servidor.", "Error de Datos");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"General error loading compartments: {ex.Message}");
            await CustomAlert.ShowErrorAsync($"Error cargando los compartimentos:\n\n{ex.Message}", "Error del Sistema");
        }
    }

    private async void GetAllCompartimentData()
    {
        await GetAllCompartimentDataAsync();
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
            await CustomAlert.ShowErrorAsync("No se encontró el token de autenticación", "Error de Autenticación");
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
            await CustomAlert.ShowErrorAsync($"No se pudo cargar la configuración de capacidad:\n\n{ex.Message}", "Error de Carga");
        }
    }

    public async Task SaveCompartimentCapacityDataAsync()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await CustomAlert.ShowErrorAsync("No se encontró el token de autenticación", "Error de Autenticación");
            return;
        }
        try
        {
            if (SelectCapacity is null)
            {
                await CustomAlert.ShowErrorAsync("Debe seleccionar un tanque para asignar la capacidad", "Tanque Requerido");
                return;
            }

            if (SelectCompartiment is null)
            {
                await CustomAlert.ShowErrorAsync("Debe seleccionar un compartimento para configurar", "Compartimento Requerido");
                return;
            }

            if (!Default.HasValue || Default <= 0)
            {
                await CustomAlert.ShowErrorAsync("Debe ingresar un valor de capacidad válido (mayor que 0)", "Capacidad Inválida");
                return;
            }

            if (Default > 100000)
            {
                await CustomAlert.ShowErrorAsync("La capacidad no puede exceder 100,000 litros", "Capacidad Excesiva");
                return;
            }

            CompartimentCapacityModel = new CompartimentCapacityModel
            {
                IdCapacity = SelectCapacity.IdCapacity,
                IdCompartiment = SelectCompartiment.Number,
                Default = Default.Value
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
                string tankCode = SelectCapacity.Code ?? "N/A";
                int compartmentNumber = SelectCompartiment.Number;
                
                await CustomAlert.ShowSuccessAsync(
                    $"Se ha configurado exitosamente la capacidad de {Default.Value} L para el compartimento #{compartmentNumber} del tanque {tankCode}", 
                    "Capacidad Configurada");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await CustomAlert.ShowErrorAsync(
                    $"No se pudo guardar la configuración:\n\nCódigo: {response.StatusCode}\nDetalle: {error}", 
                    "Error del Servidor");
            }
        }
        catch (HttpRequestException httpEx)
        {
            await CustomAlert.ShowErrorAsync(
                "Error de conexión. Verifique su conexión a internet e intente nuevamente.", 
                "Error de Conexión");
        }
        catch (JsonException jsonEx)
        {
            await CustomAlert.ShowErrorAsync(
                "Error al procesar la respuesta del servidor.", 
                "Error de Datos");
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync(
                $"Error inesperado al enviar la configuración:\n\n{ex.Message}", 
                "Error del Sistema");
        }
    }


    public ICommand GetByIdCompartimentCapacityDataCommand { get; }
    public ICommand SaveCompartimentCapacityDataCommand { get; }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
