using APP.Eds.Helpers;
using APP.Eds.Models.EdsTank;
using APP.Eds.Models.Islander;
using APP.Eds.Services.Config;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows.Input;

namespace APP.Eds.Services.EdsTank;

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

    public EdsTankService()
    {

        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);

        GetAllEdsData();
        GetByIdEdsTankDataCommand = new Command<int>(async (edsTankId) => await GetByIdEdsTankDataAsync(edsTankId));
        SaveEdsTankDataCommand = new Command(async () => await SaveEdsTankDataAsync());

        GetAllTankData();
        GetByIdEdsTankDataCommand = new Command<int>(async (edsTankId) => await GetByIdEdsTankDataAsync(edsTankId));
        SaveEdsTankDataCommand = new Command(async () => await SaveEdsTankDataAsync());
        
    }
    //GuardaEds
    private async void GetAllEdsData()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return;
        }
        try
        {
            string url = $"{Configuration.BaseUrl}/api/v1/eds";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.GetStringAsync(url);
            var EdsList = JsonSerializer.Deserialize<EdsResponseModel>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            UpdateEdsList(EdsList.Data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando los datos: {ex.Message}");
        }
    }

    private void UpdateEdsList(IEnumerable<EdsModel> Data)
    {
        EdsList.Clear();
        foreach (var eds in Data)
        {
            EdsList.Add(eds);
        }
    }

    //GuardaTank

    private async void GetAllTankData()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return;
        }
        try
        {
            string url = $"{Configuration.BaseUrl}/api/v1/tank";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.GetStringAsync(url);
            var TankList = JsonSerializer.Deserialize<TankResponse>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            UpdateTankList(TankList.Data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando los datos: {ex.Message}");
        }
    }

    private void UpdateTankList(IEnumerable<TankModelResponse> Data)
    {
        TankList.Clear();
        foreach (var eds in Data)
        {
            TankList.Add(eds);
        }
    }
    public async Task GetByIdEdsTankDataAsync(int edsTankId)
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
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/eds-tank{ edsTankId}");
            Console.WriteLine(response);

            EdsTankModel = JsonSerializer.Deserialize<EdsTankModel>(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo cargar el dato: {ex.Message}", "OK");
        }
    }

    public async Task SaveEdsTankDataAsync()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return;
        }
        try
        {
            if (SelectEds is null || SelectTank is null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Por favor, seleccione Una Eds", "OK");
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


    public ICommand GetByIdEdsTankDataCommand { get; }
    public ICommand SaveEdsTankDataCommand { get; }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
