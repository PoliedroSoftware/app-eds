using APP.Eds.Components.PopUp;
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

    private const string RegisterShiftEndpoint = "/api/v1/register-shift";
    private readonly CourtService _courtService = new CourtService();

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
        _authToken = TokenHelper.LoadToken();

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

    public void ResetAdminEds()
    {
        IdBusiness = null;
        IdEds = null;
        IdIslander = null;

        OnPropertyChanged(nameof(IdBusiness));
        OnPropertyChanged(nameof(IdEds));
        OnPropertyChanged(nameof(IdIslander));
    }

    /// <summary>
    /// Reinicia todos los campos del servicio de registro de turnos de administrador.
    /// Este método debe llamarse al cerrar sesión para limpiar completamente
    /// todos los datos del usuario anterior.
    /// </summary>
    public static void ResetInstanceFields()
    {
        if (_instance != null)
        {
            // Limpiar IDs
            _instance.IdEds = null;
            _instance.IdBusiness = null;
            _instance.IdIslander = null;
            
            // Limpiar nombres
            _instance.IslanderName = string.Empty;
            
            // Reiniciar fechas y horas a valores por defecto
            _instance.DateStart = DateTime.Now.Date;
            _instance.DateEnd = DateTime.Now.Date;
            _instance.StartTime = new TimeSpan(6, 0, 0);
            _instance.EndTime = new TimeSpan(14, 0, 0);
            
            // Notificar cambios para actualizar UI (si está visible)
            _instance.OnPropertyChanged(nameof(IdEds));
            _instance.OnPropertyChanged(nameof(IdBusiness));
            _instance.OnPropertyChanged(nameof(IdIslander));
            _instance.OnPropertyChanged(nameof(IslanderName));
            _instance.OnPropertyChanged(nameof(DateStart));
            _instance.OnPropertyChanged(nameof(DateEnd));
            _instance.OnPropertyChanged(nameof(StartTime));
            _instance.OnPropertyChanged(nameof(EndTime));
            
            System.Diagnostics.Debug.WriteLine("RegisterShiftAdminService: All instance fields reset successfully");
        }
    }

    /// <summary>
    /// Destruye la instancia singleton del servicio.
    /// </summary>
    public static void DestroyInstance()
    {
        _instance = null;
    }

    public async Task SaveRegisterShiftAsync(bool showAlert = true)
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return;
        }

        var start = DateStart.Date + StartTime;
        var end = DateEnd.Date + EndTime;

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
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{Configuration.BaseUrl}{RegisterShiftEndpoint}", content);

            if (response.IsSuccessStatusCode)
            {
                // ✅ Solo mostrar alerta si showAlert es true (cuando el checkbox está marcado)
                if (showAlert)
                {
                    await CustomAlert.ShowSuccessAsync("Turno registrado correctamente", "Éxito");
                }
                ResetAdminEds();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await CustomAlert.ShowErrorAsync($"No se pudo registrar el turno: {response.StatusCode}\n{error}", "Error");
                return;
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

