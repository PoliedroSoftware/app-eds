using APP.Eds.Models.Eds;
using APP.Eds.Models.RegisterShift;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using APP.Eds.Helpers;
using APP.Eds.Services.Config;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using APP.Eds.Models.Islander;

namespace APP.Eds.Services.RegisterShift;

public class RegisterShiftService : INotifyPropertyChanged
{
    private static RegisterShiftService? _instance;
    public static RegisterShiftService Instance => _instance ??= new RegisterShiftService();

    private string? _authToken;
    public event PropertyChangedEventHandler? PropertyChanged;

    private const string RegisterShiftEndpoint = "/api/v1/registershift";

    // EDS
    public ObservableCollection<EdsResponse> EdsList { get; } = [];
    private EdsResponse _selectedEds;
    public EdsResponse SelectedEds
    {
        get => _selectedEds;
        set
        {
            _selectedEds = value;
            OnPropertyChanged(nameof(SelectedEds));
            IdEds = _selectedEds?.IdEds;

            // Auto-select Business by EDS when available
            if (_selectedEds != null)
            {
                var byEdsBusiness = BusinessList?.FirstOrDefault(b => b.IdBusiness == _selectedEds.IdBusiness);
                if (byEdsBusiness != null)
                {
                    SelectedBusiness = byEdsBusiness;
                }
            }
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

    // Business
    public ObservableCollection<BusinessModel> BusinessList { get; } = [];
    private BusinessModel _selectedBusiness;
    public BusinessModel SelectedBusiness
    {
        get => _selectedBusiness;
        set
        {
            _selectedBusiness = value;
            OnPropertyChanged(nameof(SelectedBusiness));
            IdBusiness = _selectedBusiness?.IdBusiness;
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

    // Islander (Islero)
    public ObservableCollection<IslanderResponse> IslanderList { get; } = [];
    private IslanderResponse _selectedIslander;
    public IslanderResponse SelectedIslander
    {
        get => _selectedIslander;
        set
        {
            _selectedIslander = value;
            OnPropertyChanged(nameof(SelectedIslander));
            IdIslander = _selectedIslander?.IdIslander;
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

    // Time fields
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

    private RegisterShiftModel _registerShiftModel;

    public ICommand SaveRegisterShiftCommand { get; }
    public ICommand RefreshEdsCommand { get; }

    public RegisterShiftService()
    {
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);

        SaveRegisterShiftCommand = new Command(async () => await SaveRegisterShiftAsync());
        RefreshEdsCommand = new Command(async () => await LoadAllLookupsAsync());

        DateStart = DateTime.Now.Date;
        DateEnd = DateTime.Now.Date;
        StartTime = new TimeSpan(6, 0, 0);
        EndTime = new TimeSpan(18, 0, 0);

        _ = LoadAllLookupsAsync();
    }

    private void UpdateDateEndForOvernight()
    {
        if (EndTime < StartTime)
            DateEnd = DateStart.AddDays(1);
        else
            DateEnd = DateStart;
    }

    private async Task LoadAllLookupsAsync()
    {
        await LoadBusinessAsync();
        await LoadEdsAsync();
        await LoadIslandersAsync();

        // Defaults for normal User role
        var role = Preferences.Get("userRole", string.Empty);
        if (role == "User")
        {
            var edsPref = Preferences.Get("edsId", string.Empty);
            if (int.TryParse(edsPref, out var edsId))
            {
                var eds = EdsList.FirstOrDefault(e => e.IdEds == edsId);
                if (eds != null) SelectedEds = eds;
            }

            var businessPref = Preferences.Get("businessId", string.Empty);
            if (int.TryParse(businessPref, out var businessId))
            {
                var business = BusinessList.FirstOrDefault(b => b.IdBusiness == businessId);
                if (business != null) SelectedBusiness = business;
            }
            else if (SelectedEds != null)
            {
                var business = BusinessList.FirstOrDefault(b => b.IdBusiness == SelectedEds.IdBusiness);
                if (business != null) SelectedBusiness = business;
            }

            var islanderPref = Preferences.Get("islanderId", string.Empty);
            if (int.TryParse(islanderPref, out var islanderId))
            {
                var islander = IslanderList.FirstOrDefault(i => i.IdIslander == islanderId);
                if (islander != null) SelectedIslander = islander;
            }
        }
    }

    private async Task LoadBusinessAsync()
    {
        if (string.IsNullOrEmpty(_authToken)) return;
        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/business?PageNumber=1&PageSize=100");
            var businessList = JsonSerializer.Deserialize<BusinessResponseModel>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            BusinessList.Clear();
            foreach (var b in businessList?.Data ?? new List<BusinessModel>())
                BusinessList.Add(b);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando negocios: {ex.Message}");
        }
    }

    private async Task LoadEdsAsync()
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

            var edsResponse = JsonSerializer.Deserialize<EdsApiResponse>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            EdsList.Clear();
            foreach (var eds in edsResponse?.Data ?? new List<EdsResponse>())
                EdsList.Add(eds);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando EDS: {ex.Message}");
        }
    }

    private async Task LoadIslandersAsync()
    {
        if (string.IsNullOrEmpty(_authToken)) return;
        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/islander?PageNumber=1&PageSize=100");
            var islanderList = JsonSerializer.Deserialize<IslanderApiResponse>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            IslanderList.Clear();
            foreach (var i in islanderList?.Data ?? new List<IslanderResponse>())
                IslanderList.Add(i);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando isleros: {ex.Message}");
        }
    }

    public async Task SaveRegisterShiftAsync()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return;
        }

        // Determine role and fill defaults for User
        var role = Preferences.Get("userRole", string.Empty);
        if (role == "User")
        {
            if (!IdBusiness.HasValue || IdBusiness <= 0)
            {
                var businessPref = Preferences.Get("businessId", string.Empty);
                if (int.TryParse(businessPref, out var bid)) IdBusiness = bid; else if (SelectedEds != null) IdBusiness = SelectedEds.IdBusiness;
            }
            if (!IdIslander.HasValue || IdIslander <= 0)
            {
                var islanderPref = Preferences.Get("islanderId", string.Empty);
                if (int.TryParse(islanderPref, out var iid)) IdIslander = iid;
            }
            if (!IdEds.HasValue || IdEds <= 0)
            {
                var edsPref = Preferences.Get("edsId", string.Empty);
                if (int.TryParse(edsPref, out var eid)) IdEds = eid;
            }
        }
        else // Admin must select
        {
            if (!IdBusiness.HasValue || IdBusiness <= 0)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Seleccione un Negocio válido", "OK");
                return;
            }
            if (!IdIslander.HasValue || IdIslander <= 0)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Seleccione un Islero válido", "OK");
                return;
            }
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
                    idBusiness = IdBusiness,
                    idEds = IdEds,
                    idIslander = IdIslander,
                    dateStartTime = DateStart.ToString("yyyy-MM-dd"),
                    startTime = StartTime.ToString(@"HH\:mm\:ss"),
                    dateEndTime = DateEnd.ToString("yyyy-MM-dd"),
                    endTime = EndTime.ToString(@"HH\:mm\:ss")
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