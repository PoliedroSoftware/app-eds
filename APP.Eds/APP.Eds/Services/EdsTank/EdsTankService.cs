using APP.Eds.Helpers;
using APP.Eds.Models.EdsTank;
using APP.Eds.Models.Islander;
using APP.Eds.Services.Config;
using APP.Eds.Components.PopUp;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows.Input;

namespace APP.Eds.Services.EdsTank;

public class TankAssignmentInfo
{
    public string EdsName { get; set; }
    public string TankNumber { get; set; }
    public int EdsId { get; set; }
    public int TankId { get; set; }
}

public class EdsTankService : INotifyPropertyChanged
{
    private string? _authToken;
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public ObservableCollection<EdsModel> EdsList { get; set; } = [];
    public ObservableCollection<TankModelResponse> TankList { get; set; } = [];
    
    private EdsTankRequest Request { get; set; }
    private EdsTankModel _edsTank;
    
    public EdsTankModel EdsTankModel
    {
        get => _edsTank;
        set
        {
            _edsTank = value;
            OnPropertyChanged(nameof(EdsTankModel));
        }
    }

    private int _idIdEds;
    public int IdIdEds
    {
        get => _idIdEds;
        set
        {
            _idIdEds = value;
            OnPropertyChanged(nameof(IdIdEds));
        }
    }

    private EdsModel _selectedEds;
    public EdsModel SelectEds
    {
        get => _selectedEds;
        set
        {
            _selectedEds = value;
            OnPropertyChanged(nameof(SelectEds));
            if (_selectedEds != null)
            {
                IdIdEds = _selectedEds.IdEds;
            }
        }
    }

    private int _idIdTank;
    public int IdIdTank
    {
        get => _idIdTank;
        set
        {
            _idIdTank = value;
            OnPropertyChanged(nameof(IdIdTank));
        }
    }

    private TankModelResponse _selectedTank;
    public TankModelResponse SelectTank
    {
        get => _selectedTank;
        set
        {
            _selectedTank = value;
            OnPropertyChanged(nameof(SelectTank));
            if (_selectedTank != null)
            {
                IdIdTank = _selectedTank.IdTank;
            }
        }
    }

    public ICommand GetByIdEdsTankDataCommand { get; }
    public ICommand SaveEdsTankDataCommand { get; }

    public EdsTankService()
    {
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);

        GetByIdEdsTankDataCommand = new Command<int>(async (edsTankId) => await GetByIdEdsTankDataAsync(edsTankId));
        SaveEdsTankDataCommand = new Command(async () => await SaveEdsTankDataAsync());
        
        // Cargar datos iniciales
        _ = Task.Run(async () => await RefreshDataAsync());
    }

    public async Task RefreshDataAsync()
    {
        try
        {
            await Task.WhenAll(
                GetAllEdsDataAsync(),
                GetAllTankDataAsync()
            );
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error al actualizar los datos:\n\n{ex.Message}", "Error de Actualización");
        }
    }

    private async Task GetAllEdsDataAsync()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await CustomAlert.ShowErrorAsync("No se encontró el token de autenticación", "Error de Autenticación");
            return;
        }
        
        try
        {
            string url = $"{Configuration.BaseUrl}/api/v1/eds";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.GetStringAsync(url);
            var edsResponse = JsonSerializer.Deserialize<EdsResponseModel>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                UpdateEdsList(edsResponse?.Data ?? []);
            });
        }
        catch (HttpRequestException)
        {
            await CustomAlert.ShowErrorAsync("Error de conexión. Verifique su conexión a internet.", "Error de Conexión");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando EDS: {ex.Message}");
        }
    }

    private async Task GetAllTankDataAsync()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await CustomAlert.ShowErrorAsync("No se encontró el token de autenticación", "Error de Autenticación");
            return;
        }
        
        try
        {
            string url = $"{Configuration.BaseUrl}/api/v1/tank";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                await CustomAlert.ShowErrorAsync($"Error del servidor ({response.StatusCode}): {errorMsg}", "Error de Carga de Tanques");
                return;
            }

            var json = await response.Content.ReadAsStringAsync();
            var tankResponse = JsonSerializer.Deserialize<TankResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                UpdateTankList(tankResponse?.Data ?? []);
            });
        }
        catch (HttpRequestException)
        {
            await CustomAlert.ShowErrorAsync("Error de conexión. Verifique su conexión a internet.", "Error de Conexión");
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error inesperado: {ex.Message}", "Error de Carga de Tanques");
        }
    }

    private void UpdateEdsList(IEnumerable<EdsModel> data)
    {
        EdsList.Clear();
        foreach (var eds in data.OrderBy(e => e.Name))
        {
            EdsList.Add(eds);
        }
    }

    private void UpdateTankList(IEnumerable<TankModelResponse> data)
    {
        TankList.Clear();
        foreach (var tank in data.OrderBy(t => t.Number))
        {
            TankList.Add(tank);
        }
    }

    public async Task<bool> CheckExistingAssignmentAsync()
    {
        if (SelectTank == null || string.IsNullOrEmpty(_authToken))
            return false;

        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/eds-tank/check/{SelectTank.IdTank}");
            
            return bool.Parse(response);
        }
        catch
        {
            // Si hay error, asumir que no existe para permitir la operación
            return false;
        }
    }

    public async Task<List<TankAssignmentInfo>> GetCurrentAssignmentsAsync()
    {
        if (string.IsNullOrEmpty(_authToken))
            return new List<TankAssignmentInfo>();

        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/eds-tank/assignments");
            
            var assignments = JsonSerializer.Deserialize<List<TankAssignmentInfo>>(response, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
            
            return assignments ?? new List<TankAssignmentInfo>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading assignments: {ex.Message}");
            return new List<TankAssignmentInfo>();
        }
    }

    public async Task GetByIdEdsTankDataAsync(int edsTankId)
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
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/eds-tank/{edsTankId}");

            EdsTankModel = JsonSerializer.Deserialize<EdsTankModel>(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"No se pudo cargar el dato:\n\n{ex.Message}", "Error de Carga");
        }
    }

    public async Task SaveEdsTankDataAsync()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await CustomAlert.ShowErrorAsync("No se encontró el token de autenticación", "Error de Autenticación");
            return;
        }

        try
        {
            if (SelectEds == null)
            {
                await CustomAlert.ShowWarningAsync("Debe seleccionar una estación de servicio (EDS)", "EDS Requerida");
                return;
            }

            if (SelectTank == null)
            {
                await CustomAlert.ShowWarningAsync("Debe seleccionar un tanque", "Tanque Requerido");
                return;
            }

            EdsTankModel = new EdsTankModel
            {
                IdEds = SelectEds.IdEds,
                IdTank = SelectTank.IdTank
            };

            Request = new EdsTankRequest
            {
                Request = EdsTankModel
            };

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var json = JsonSerializer.Serialize(Request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{Configuration.BaseUrl}/api/v1/eds-tank", content);

            if (response.IsSuccessStatusCode)
            {
                // Éxito - el mensaje se maneja en la vista
                Console.WriteLine("Tank assignment successful");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                string errorMessage = "No se pudo completar la asignación del tanque.";
                
                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    errorMessage = "El tanque ya está asignado a otra EDS. Use la opción de reasignación si desea cambiar la asignación.";
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    errorMessage = "Los datos proporcionados no son válidos. Verifique las selecciones e intente nuevamente.";
                }
                
                await CustomAlert.ShowErrorAsync($"{errorMessage}\n\nCódigo: {response.StatusCode}", "Error del Servidor");
            }
        }
        catch (HttpRequestException)
        {
            await CustomAlert.ShowErrorAsync("Error de conexión. Verifique su conexión a internet e intente nuevamente.", "Error de Conexión");
        }
        catch (TaskCanceledException)
        {
            await CustomAlert.ShowErrorAsync("La operación tardó demasiado tiempo. Intente nuevamente.", "Tiempo Agotado");
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error inesperado al asignar el tanque:\n\n{ex.Message}", "Error del Sistema");
        }
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
