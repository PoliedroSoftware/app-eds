using APP.Eds.Models.Eds;
using APP.Eds.Models.RegisterShift;
using System.Collections.ObjectModel;
using APP.Eds.Helpers;
using APP.Eds.Services.Config;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows.Input;
using APP.Eds.Models.Dispensers;
using APP.Eds.Models.Islander;
using System.Linq.Expressions;

namespace APP.Eds.Services.RegisterShift;

public class RegisterShiftUserService : INotifyPropertyChanged
{
    private static RegisterShiftUserService? _instance;
    public static RegisterShiftUserService Instance => _instance ??= new RegisterShiftUserService();

    private string? _authToken;
    public event PropertyChangedEventHandler? PropertyChanged;

    private const string RegisterShiftEndpoint = "/api/v1/registershift";

    public ObservableCollection<EdsResponse> EdsList { get; } = [];
    public ObservableCollection<EdsResponse> BusinessList { get; } = [];
    public ObservableCollection<IslanderRequestModel> IslanderList { get; } = [];

    private EdsResponse _selectedEds;
    public EdsResponse SelectedEds
    {
        get => _selectedEds;
        set
        {
            _selectedEds = value;
            OnPropertyChanged(nameof(SelectedEds));
            IdEds = _selectedEds?.IdEds;
        }
    }

    private EdsResponse _selectedBusiness;
    public EdsResponse SelectedBusiness
    {
        get => _selectedBusiness;
        set
        {
            _selectedBusiness = value;
            OnPropertyChanged(nameof(SelectedBusiness));
            IdBusiness = _selectedBusiness?.IdBusiness;
        }
    }

    private IslanderRequestModel _selectedIslander;
    public IslanderRequestModel SelectedIslander
    {
        get => _selectedIslander;
        set
        {
            _selectedIslander = value;
            OnPropertyChanged(nameof(SelectedIslander));
            IdIslander = _selectedIslander?.IdIslander;
        }
    }

    private int? _idBusiness;
    public int? IdBusiness
    {
        get => _idBusiness;
        set
        {
            _idBusiness = value;
            OnPropertyChanged(nameof(IdBusiness));
        }
    }

    private int? _idIslander;
    public int? IdIslander
    {
        get => _idIslander;
        set
        {
            _idIslander = value;
            OnPropertyChanged(nameof(IdIslander));
        }
    }

    private int? _idEds;
    public int? IdEds
    {
        get => _idEds;
        set
        {
            _idEds = value;
            OnPropertyChanged(nameof(IdEds));
        }
    }

    private DateTime _dateStart = DateTime.Now.Date;
    public DateTime DateStart
    {
        get => _dateStart;
        set
        {
            if (_dateStart != value)
            {
                _dateStart = value;
                OnPropertyChanged(nameof(DateStart));
                UpdateDateEndForOvernight();
            }
        }
    }

    private DateTime _dateEnd = DateTime.Now.Date;
    public DateTime DateEnd
    {
        get => _dateEnd;
        set
        {
            if (_dateEnd != value)
            {
                _dateEnd = value;
                OnPropertyChanged(nameof(DateEnd));
            }
        }
    }

    private TimeSpan _startTime = new TimeSpan(6, 0, 0);
    public TimeSpan StartTime
    {
        get => _startTime;
        set
        {
            if (_startTime != value)
            {
                _startTime = value;
                OnPropertyChanged(nameof(StartTime));
                UpdateDateEndForOvernight();
            }
        }
    }

    private TimeSpan _endTime = new TimeSpan(18, 0, 0);
    public TimeSpan EndTime
    {
        get => _endTime;
        set
        {
            if (_endTime != value)
            {
                _endTime = value;
                OnPropertyChanged(nameof(EndTime));
                UpdateDateEndForOvernight();
            }
        }
    }

    private RegisterShiftUserModel _registerShiftUserModel;

    public ICommand SaveRegisterShiftCommand { get; }
    public ICommand RefreshEdsCommand { get; }
    public ICommand RefreshIslanderCommand { get; }

    public RegisterShiftUserService()
    {
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);

        SaveRegisterShiftCommand = new Command(async () => await SaveRegisterShiftAsync());
        RefreshEdsCommand = new Command(async () => await LoadEdsBusinessAsync());
        RefreshIslanderCommand = new Command(async () => await LoadIslanderAsync());

        DateStart = DateTime.Now.Date;
        DateEnd = DateTime.Now.Date;
        StartTime = new TimeSpan(6, 0, 0);
        EndTime = new TimeSpan(18, 0, 0);

        _ = LoadEdsBusinessAsync();
        _ = LoadIslanderAsync();
    }

    private void UpdateDateEndForOvernight()
    {
        if (EndTime < StartTime)
            DateEnd = DateStart.AddDays(1);
        else
            DateEnd = DateStart;
    }

    private async Task LoadEdsBusinessAsync()
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
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/eds?PageNumber=1&PageSize=100");
            var responseIslander = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/islander?PageNumber=1&PageSize=100");
            // The API typically returns { data: [...] }
            var edsResponse = JsonSerializer.Deserialize<EdsApiResponse>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            EdsList.Clear();
            BusinessList.Clear();
            foreach (var edsBusiness in edsResponse?.Data ?? new List<EdsResponse>())
                EdsList.Add(edsBusiness);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando EDS o business: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Error", $"Error al cargar EDS o Business: {ex.Message}", "OK");
            return;
        }
    }

    private async Task LoadIslanderAsync()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return;
        }

        try
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var responseIslander = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/islander?PageNumber=1&PageSize=100");
            var islanderResponse = JsonSerializer.Deserialize<IslanderApiResponse>(responseIslander, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            IslanderList.Clear();
            foreach (var islander in islanderResponse?.DataUser ?? new List<IslanderRequestModel>())
            {
                IslanderList.Add(islander);
            }       
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando islanders: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Error", $"Error al cargar islanders: {ex.Message}", "OK");
            return;
        }
    }

    public async Task SaveRegisterShiftAsync()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return;
        }

        if (!IdEds.HasValue || IdEds <= 0)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Por favor, seleccione un EDS válido", "OK");
            return;
        }
      

        var start = DateStart.Date + StartTime;
        var end = DateEnd.Date + EndTime;

        if (end <= start)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error de validación",
                "La fecha/hora de fin debe ser posterior a la fecha/hora de inicio.",
                "OK");
            return;
        }

        try
        {
            var payload = new
            {
                request = new
                {
                    idEds = IdEds.Value,
                    idBusiness = IdBusiness.Value,
                    idIslander = IdIslander.Value,
                    dateStartTime = DateStart.ToString("yyyy-MM-dd"),
                    startTime = StartTime.ToString(@"hh\:mm\:ss"),
                    dateEndTime = DateEnd.ToString("yyyy-MM-dd"),
                    endTime = EndTime.ToString(@"hh\:mm\:ss")
                }
            };

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{Configuration.BaseUrl}{RegisterShiftEndpoint}", content);

            if (response.IsSuccessStatusCode)
            {
                await Application.Current.MainPage.DisplayAlert("Éxito", "Turno registrado correctamente", "OK");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo registrar el turno: {response.StatusCode}\n{error}", "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error al registrar el turno: {ex.Message}", "OK");
        }
    }

    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}