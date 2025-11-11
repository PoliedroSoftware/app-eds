using APP.Eds.Helpers;
using APP.Eds.Models.Dispensers;
using APP.Eds.Models.Eds;
using APP.Eds.Models.Islander;
using APP.Eds.Models.RegisterShift;
using APP.Eds.Services.Config;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Input;


namespace APP.Eds.Services.RegisterShift;

public class RegisterShiftUserService : INotifyPropertyChanged
{
    private static RegisterShiftUserService? _instance;
    public static RegisterShiftUserService Instance => _instance ??= new RegisterShiftUserService();

    private string? _authToken;
    public event PropertyChangedEventHandler? PropertyChanged;

    private const string RegisterShiftEndpoint = "/api/v1/registershift";

    public ObservableCollection<EdsResponse> ListEds { get; } = [];

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

    private bool _showEdsPicker;
    public bool ShowEdsPicker
    {
        get => _showEdsPicker;
        private set { _showEdsPicker = value; OnPropertyChanged(nameof(ShowEdsPicker)); }
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

    private string _edsName;
    public string EdsName
    {
        get => _edsName;
        private set
        {
            _edsName = value;
            OnPropertyChanged(nameof(EdsName));
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

    public RegisterShiftUserService()
    {
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);

        SaveRegisterShiftCommand = new Command(async () => await SaveRegisterShiftAsync());
        RefreshEdsCommand = new Command(async () => await LoadEdsAsync());

        DateStart = DateTime.Now.Date;
        DateEnd = DateTime.Now.Date;
        StartTime = new TimeSpan(6, 0, 0);
        EndTime = new TimeSpan(18, 0, 0);

        _ = LoadEdsAsync();
    }

    private void UpdateDateEndForOvernight()
    {
        if (EndTime < StartTime)
            DateEnd = DateStart.AddDays(1);
        else
            DateEnd = DateStart;
    }
    public async Task LoadEdsAsync()
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
            // The API typically returns { data: [...] }
            var edsResponse = JsonSerializer.Deserialize<EdsApiResponse>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            ListEds.Clear();
            foreach (var eds in edsResponse?.Data ?? new List<EdsResponse>())
                ListEds.Add(eds);

            //if (UserEds.Count <= 1)
            //{
            //    ShowEdsPicker = true;

            //    if (UserEds.Count == 1)
            //    {
            //        SelectedEds = UserEds[0];
            //        EdsName = UserEds[0].Name ?? "EDS asignado";
            //    }

            //    else
            //    {
            //        EdsName = "Sin EDS Asignado";
            //        IdEds = null;
            //    }
            //}
            //else
            //{
            //    ShowEdsPicker= true;
            //    EdsName = string.Empty;
            //    IdEds = null;
            //}

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando EDS o business: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Error", $"Error al cargar EDS o Business: {ex.Message}", "OK");
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
        if (!(IdEds is > 0))
        {
            if (ShowEdsPicker)
                await Application.Current.MainPage.DisplayAlert("EDS requerido", "Seleccione el EDS donde registrará el turno.", "OK");
            else
                await Application.Current.MainPage.DisplayAlert("EDS no disponible", "No se encontró un EDS asignado.", "OK");
            return;
        }

        try
        {
            var payload = new
            {
                request = new
                {
                    idEds = IdEds.Value,
                    // Business e Islander quedan opcionales; si son null se omiten del JSON
                    dateStartTime = DateStart.ToString("yyyy-MM-dd"),
                    startTime = StartTime.ToString(@"hh\:mm\:ss"),
                    dateEndTime = DateEnd.ToString("yyyy-MM-dd"),
                    endTime = EndTime.ToString(@"hh\:mm\:ss")
                }
            };
            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{Configuration.BaseUrl}{RegisterShiftEndpoint}", content);

            if (response.IsSuccessStatusCode)
                await Application.Current.MainPage.DisplayAlert("Éxito", "Turno registrado correctamente", "OK");
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