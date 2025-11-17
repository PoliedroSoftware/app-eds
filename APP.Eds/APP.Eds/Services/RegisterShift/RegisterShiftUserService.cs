using APP.Eds.Components.PopUp;
using APP.Eds.Helpers;
using APP.Eds.Models.Eds;
using APP.Eds.Models.Islander;
using APP.Eds.Models.RegisterShift;
using APP.Eds.Services.Config;
using APP.Eds.Services.Islander;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Input;
using static System.Net.WebRequestMethods;


namespace APP.Eds.Services.RegisterShift;

public class RegisterShiftUserService : INotifyPropertyChanged
{
    private static RegisterShiftUserService? _instance;
    public string UserRole { get; set; } = string.Empty;

    public static RegisterShiftUserService Instance => _instance ??= new RegisterShiftUserService();

    private string? _authToken;
    public event PropertyChangedEventHandler? PropertyChanged;

    private const string RegisterShiftEndpoint = "/api/v1/registershift";
    private readonly IslanderService _islanderService = new IslanderService();

    //public ObservableCollection<IslanderResponse> ListIslander { get; } = [];
    public ObservableCollection<EdsResponse> UserEds { get; } = [];
    public ObservableCollection<EnhancedIslanderItem> ListIslander { get; } = [];

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

    private RegisterShiftUserModel _registerShiftUserModel;

    public ICommand SaveRegisterShiftCommand { get; }
    public ICommand RefreshIslanderCommand { get; }

    public RegisterShiftUserService()
    {
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);

        SaveRegisterShiftCommand = new Command(async () => await SaveRegisterShiftAsync());
        RefreshIslanderCommand = new Command(async () => await LoadIslanderAsync());

        DateStart = DateTime.Now.Date;
        DateEnd = DateTime.Now.Date;
        StartTime = new TimeSpan(6, 0, 0);
        EndTime = new TimeSpan(18, 0, 0);

        _ = LoadIslanderAsync();

        UserRole = Preferences.Get("userRole", string.Empty);
    }

    private static string? GetUserLogged()
    {
        var name = Preferences.Get("Usernamelogin", string.Empty);
        return string.IsNullOrWhiteSpace(name) ? null : name.Trim();
    }

    private void UpdateDateEndForOvernight()
    {
        if (EndTime < StartTime)
            DateEnd = DateStart.AddDays(1);
        else
            DateEnd = DateStart;
    }
    public async Task LoadIslanderAsync()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return;
        }

        try
        {
            await _islanderService.GetIslandersAsync();

            var nameUser = GetUserLogged();
            if (string.IsNullOrEmpty(nameUser))
            {
                UserEds.Clear();
                ShowEdsPicker = false;
                EdsName = "Usuario sin Nombre";
                IdEds = null;
                return;
            }

            var getAllIslander = _islanderService.IslanderList;
            if (getAllIslander == null || getAllIslander.Count == 0 && UserRole == "User")
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se encontraron datos de Islander.", "OK");
                return;
            }
            
            var userIslander = getAllIslander
                .Where(i => !string.IsNullOrWhiteSpace(i.Name) &&
                            string.Equals(i.Name.Trim(), nameUser.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();

            if (!userIslander.Any() && UserRole == "User")
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se encontró EDS para el usuario '{nameUser}'.", "OK");
            }

            var edsIds = userIslander
                .Select(i => i.IdEds)
                .Where(id => id.HasValue && id.Value > 0)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            var userEdsList = _islanderService.EdsList
                .Where(e => edsIds.Contains(e.IdEds))
                .Select(e => new EdsResponse { IdEds = e.IdEds, Name = (e.Name ?? "").Trim() });

            var withNames = userIslander
                .Where(i => i.IdEds.HasValue && i.IdEds > 0 && !string.IsNullOrWhiteSpace(i.EdsName))
                .Select(i => new EdsResponse { IdEds = i.IdEds!.Value, Name = i.EdsName!.Trim() });

            var merged = userEdsList
                    .Concat(withNames)
                    .Where(x => x.IdEds > 0 && !string.IsNullOrWhiteSpace(x.Name))
                    .GroupBy(x => x.IdEds)
                    .Select(g => g.First())
                    .OrderBy(x => x.Name)
                    .ToList();

            UserEds.Clear();
            foreach (var eds in merged)
                UserEds.Add(eds);
                
            // 5) Decidir si mostrar el picker
            //if (UserEds.Count == 0)
            //{
            //    ShowEdsPicker = false;
            //    EdsName = "Sin EDS asignado";
            //    IdEds = null;
            //    SelectedUserEds = null;
            //}
            //else if (UserEds.Count == 1)
            //{
            //    ShowEdsPicker = false;
            //    SelectedUserEds = UserEds[0];               // esto setea IdEds
            //    EdsName = UserEds[0].Name ?? "EDS asignado";
            //}
            //else
            //{
            //    ShowEdsPicker = true;
            //    EdsName = string.Empty;
            //    IdEds = null;
            //    SelectedUserEds = null;
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
                await CustomAlert.ShowSuccessAsync("Turno registrado correctamente", "Éxito");
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