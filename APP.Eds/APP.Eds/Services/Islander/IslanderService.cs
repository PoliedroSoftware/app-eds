using APP.Eds.Helpers;
using APP.Eds.Models.Islander;
using APP.Eds.Services.Config;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows.Input;

namespace APP.Eds.Services.Islander;

public class IslanderService : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<EdsModel> EdsList { get; set; } = [];
    public ObservableCollection<IslanderResponse> IslanderList { get; set; } = [];

    private IslanderRequest Request { get; set; }
    private IslanderModel _islander;
    private string? _authToken;

    public IslanderModel Islander
    {
        get => _islander;
        set
        {
            _islander = value;
            OnPropertyChanged(nameof(Islander));
        }
    }

    private string _name;
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged(nameof(Name));
        }
    }

    private string _email;
    public string Email
    {
        get => _email;
        set
        {
            _email = value;
            OnPropertyChanged(nameof(Email));
        }
    }

    private string _firstname;
    public string FirstName
    {
        get => _firstname;
        set
        {
            _firstname = value;
            OnPropertyChanged(nameof(FirstName));
        }
    }

    private string _lastname;
    public string LastName
    {
        get => _lastname;
        set
        {
            _lastname = value;
            OnPropertyChanged(nameof(LastName));
        }
    }



    private string _password;
    public string Password
    {
        get => _password;
        set
        {
            _password = value;
            OnPropertyChanged(nameof(Password));
        }
    }


    private EdsModel _selectedEds;
    public EdsModel SelectedEds
    {
        get => _selectedEds;
        set
        {
            _selectedEds = value;
            OnPropertyChanged(nameof(SelectedEds));
            if (_selectedEds != null)
            {
                IdEds = _selectedEds.IdEds;
            }
        }
    }

    private int _idEds;
    public int IdEds
    {
        get => _idEds;
        set
        {
            _idEds = value;
            OnPropertyChanged(nameof(IdEds));
        }
    }

    public ICommand GetByIdIslanderDataCommand { get; }
    public ICommand SaveIslanderDataCommand { get; }

    public IslanderService()
    {
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
        
        GetAllEdsData();
        GetByIdIslanderDataCommand = new Command<int>(async (islanderId) => await GetByIdIslanderDataAsync(islanderId));
        SaveIslanderDataCommand = new Command(async () => await SaveIslanderDataAsync());
    }

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
            var edsList = JsonSerializer.Deserialize<EdsResponseModel>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            UpdateEdsList(edsList?.Data ?? new List<EdsModel>());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando los datos: {ex.Message}");
        }
    }

    public async Task GetByIdIslanderDataAsync(int islanderId)
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

            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/islander/{islanderId}");

            Islander = JsonSerializer.Deserialize<IslanderModel>(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo cargar el dato: {ex.Message}", "OK");
        }
    }

    public async Task SaveIslanderDataAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
                return;
            }

            if (SelectedEds is null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Por favor, seleccione un EDS", "OK");
                return;
            }

            Islander = new IslanderModel
            {
                Name = Name,
                Email = Email,
                FirstName = FirstName,
                LastName = LastName,
                IdEds = SelectedEds.IdEds,
                Password = Password,
            };

            Request = new IslanderRequest
            {
                Request = Islander
            };

            using var httpClient = new HttpClient();
            var json = JsonSerializer.Serialize(Request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            var response = await httpClient.PostAsync($"{Configuration.BaseUrl}/api/v1/islander", content);

            if (response.IsSuccessStatusCode)
            {
                await Application.Current.MainPage.DisplayAlert("Éxito", "Datos enviados correctamente", "OK");
                await GetIslandersAsync();
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

    private void UpdateEdsList(IEnumerable<EdsModel> edsData)
    {
        EdsList.Clear();
        foreach (var eds in edsData)
        {
            EdsList.Add(eds);
        }
    }

    public async Task GetIslandersAsync()
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

            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/islander");
            var islanders = JsonSerializer.Deserialize<IslanderApiResponse>(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            IslanderList.Clear();
            foreach (var islander in islanders.Data)
            {
                IslanderList.Add(islander);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
