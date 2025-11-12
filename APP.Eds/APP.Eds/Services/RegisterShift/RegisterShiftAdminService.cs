using APP.Eds.Helpers;
using APP.Eds.Models.Eds;
using APP.Eds.Services.Config;
using APP.Eds.Services.Court;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;

namespace APP.Eds.Services.RegisterShift;

public class RegisterShiftAdminService : INotifyPropertyChanged
{
    private static RegisterShiftAdminService? _instance;
    public static RegisterShiftAdminService Instance => _instance ??= new RegisterShiftAdminService();

    private string? _authToken;
    public event PropertyChangedEventHandler? PropertyChanged;

    private const string RegisterShiftEndpoint = "/api/v1/registershift";
    private readonly CourtService _courtService = new CourtService();

    public ObservableCollection<EdsResponse> UserEds { get; } = [];


    private EdsResponse _selectedUserEds;
    public EdsResponse SelectedUserEds
    {
        get => _selectedUserEds;
        set
        {
            _selectedUserEds = value;
            OnPropertyChanged(nameof(SelectedUserEds));
            IdEds = _selectedUserEds?.IdEds;
        }
    }

    private string _edsName;
    public string EdsName
    {
        get => _edsName;
        private set { _edsName = value; OnPropertyChanged(nameof(EdsName)); }
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

    private string _islanderName;
    public string IslanderName
    {
        get => _islanderName;
        private set
        {
            _islanderName = value;
            OnPropertyChanged(nameof(IslanderName));
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
    public ICommand SaveRegisterShiftCommand { get; }
    public ICommand RefreshIslanderCommand { get; }

    public RegisterShiftAdminService()
    {
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);

        SaveRegisterShiftCommand = new Command(async () => await SaveRegisterShiftAsync());

        DateStart = DateTime.Now.Date;
        DateEnd = DateTime.Now.Date;
        StartTime = new TimeSpan(6, 0, 0);
        EndTime = new TimeSpan(18, 0, 0);

    }


    private void UpdateDateEndForOvernight()
    {
        if (EndTime < StartTime)
            DateEnd = DateStart.AddDays(1);
        else
            DateEnd = DateStart;
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

        //if (end <= start)
        //{
        //    await Application.Current.MainPage.DisplayAlert(
        //        "Error de validación",
        //        "La fecha/hora de fin debe ser posterior a la fecha/hora de inicio.",
        //        "OK");
        //    return;
        //}

        try
        {
            var payload = new
            {
                request = new
                {
                    idEds = IdEds.Value,
                    idBusiness = IdBusiness.Value,
                    idIslero = IdIslander.Value,
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

