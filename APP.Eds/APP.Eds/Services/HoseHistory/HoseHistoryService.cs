using APP.Eds.Helpers;
using APP.Eds.Models.Dispenser;
using APP.Eds.Models.Hose;
using APP.Eds.Models.HoseHistory;
using APP.Eds.Services.Config;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows.Input;

namespace APP.Eds.Services.HoseHistory;

public class HoseHistoryService : INotifyPropertyChanged
{
    private string? _authToken;
    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<DispenserModelResponse> DispensersList { get; set; } = [];
    public ObservableCollection<HoseResponse> HoseList { get; set; } = [];
    public ObservableCollection<HoseHistoryResponse> HoseHistoryList { get; set; } = [];

    private HoseHistoryRequest Request { get; set; }
    private HoseHistoryModel _hosehistory;

    public HoseHistoryModel HoseHistory
    {
        get => _hosehistory;
        set
        {
            _hosehistory = value;
            OnPropertyChanged(nameof(HoseHistory));
        }
    }

    private DateTime _date = DateTime.Now;
    public DateTime Date
    {
        get => _date;
        set
        {
            _date = value;
            OnPropertyChanged(nameof(Date));
        }
    }

    private DispenserModelResponse _selectedDispensers;
    public DispenserModelResponse SelectedDispensers
    {
        get => _selectedDispensers;
        set
        {
            _selectedDispensers = value;
            OnPropertyChanged(nameof(SelectedDispensers));
            if (_selectedDispensers != null)
            {
                IdDispensers = _selectedDispensers.IdDispensers;
            }
        }
    }

    private int _idDispensers;
    public int IdDispensers
    {
        get => _idDispensers;
        set
        {
            _idDispensers = value;
            OnPropertyChanged(nameof(IdDispensers));
        }
    }

    private int _idIdHose;
    public int IdIdHose
    {
        get => _idIdHose;
        set
        {
            _idIdHose = value;
            OnPropertyChanged(nameof(IdIdHose));
        }
    }

    private double _accumulatedAmount;
    public double AccumulatedAmount
    {
        get => _accumulatedAmount;
        set
        {
            _accumulatedAmount = value;
            OnPropertyChanged(nameof(AccumulatedAmount));
        }
    }

    private double _accumulatedGallons;
    public double AccumulatedGallons
    {
        get => _accumulatedGallons;
        set
        {
            _accumulatedGallons = value;
            OnPropertyChanged(nameof(AccumulatedGallons));
        }
    }

    private HoseResponse _selectedHose;
    public HoseResponse SelectHose
    {
        get => _selectedHose;
        set
        {
            _selectedHose = value;
            OnPropertyChanged(nameof(SelectHose));
            if (_selectedHose != null)
            {
                IdIdHose = _selectedHose.IdHose;
            }
        }
    }

    public ICommand GetByIdHoseHistoryDataCommand { get; }
    public ICommand SaveHoseHistoryDataCommand { get; }

    public HoseHistoryService()
    {
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
        GetAllDispensersData();
        GetHoseHistoryAsync();
        GetByIdHoseHistoryDataCommand = new Command<int>(async (hosehistoryId) => await GetByIdHoseHistoryDataAsync(hosehistoryId));
        GetAllHoseData();
        
    }

    private async void GetAllDispensersData()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return;
        }
        try
        {
            string url = $"{Configuration.BaseUrl}/api/v1/dispensers";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.GetStringAsync(url);
            var dispensersList = JsonSerializer.Deserialize<DispensersResponseModel>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            UpdateDispensersList(dispensersList?.Data ?? new List<DispenserModelResponse>());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando los datos: {ex.Message}");
        }
    }

    public async Task GetByIdHoseHistoryDataAsync(int hosehistoryId)
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
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/hose-history/{hosehistoryId}");
            HoseHistory = JsonSerializer.Deserialize<HoseHistoryModel>(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo cargar el dato: {ex.Message}", "OK");
        }
    }

    

    private void UpdateDispensersList(IEnumerable<DispenserModelResponse> dispensersData)
    {
        DispensersList.Clear();
        foreach (var dispensers in dispensersData)
        {
            DispensersList.Add(dispensers);
        }
    }

    public async Task GetHoseHistoryAsync()
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
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/hose-history");
            var hosehistorys = JsonSerializer.Deserialize<HoseHistoryApiResponse>(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            HoseList.Clear();
            var sortedList = hosehistorys.Data.OrderByDescending(h => h.Date).ToList();
            foreach (var hosehistory in sortedList)
            {
                HoseHistoryList.Add(hosehistory);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public async void GetAllHoseData()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return;
        }
        try
        {
            string url = $"{Configuration.BaseUrl}/api/v1/hose";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.GetStringAsync(url);
            var HoseList = JsonSerializer.Deserialize<HoseApiResponse>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            UpdateHoseList(HoseList.Data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando los datos: {ex.Message}");
        }
    }

    private void UpdateHoseList(IEnumerable<HoseResponse> Data)
    {
        HoseList.Clear();
        foreach (var eds in Data)
        {
            HoseList.Add(eds);
        }
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}