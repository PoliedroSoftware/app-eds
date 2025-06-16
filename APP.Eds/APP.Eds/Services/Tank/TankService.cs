using APP.Eds.Helpers;
using APP.Eds.Models.Tank;
using APP.Eds.Services.Config;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows.Input;

namespace APP.Eds.Services.Tank;

public class TankService : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<TankResponse> TankList { get; set; } = [];

    private TankRequest Request { get; set; }
    private TankModel _tank;
    private string? _authToken;

    public TankModel Tank
    {
        get => _tank;
        set
        {
            _tank = value;
            OnPropertyChanged(nameof(Tank));
        }
    }

    private int _compartment;
    public int Compartment
    {
        get => _compartment;
        set
        {
            _compartment = value;
            OnPropertyChanged(nameof(Compartment));
        }
    }

    private string _number;
    public string Number
    {
        get => _number;
        set
        {
            _number = value;
            OnPropertyChanged(nameof(Number));
        }
    }

    private double _ability;
    public double Ability
    {
        get => _ability;
        set
        {
            _ability = value;
            OnPropertyChanged(nameof(Ability));
        }
    }

    private double _stock;
    public double Stock
    {
        get => _stock;
        set
        {
            _stock = value;
            OnPropertyChanged(nameof(Stock));
        }
    }

    public ICommand GetByIdTankDataCommand { get; }
    public ICommand SaveTankDataCommand { get; }

    public TankService()
    {
        GetByIdTankDataCommand = new Command<int>(async (tankId) => await GetByIdTankDataAsync(tankId));
        SaveTankDataCommand = new Command(async () => await SaveTankDataAsync());
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
    }

    public async Task GetByIdTankDataAsync(int tankId)
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
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/tank/{tankId}");
            Tank = JsonSerializer.Deserialize<TankModel>(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo cargar el dato: {ex.Message}", "OK");
        }
    }

    public async Task SaveTankDataAsync()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return;
        }
        try
        {
                if (string.IsNullOrWhiteSpace(Number))
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "El campo 'Number' no puede estar vacío.", "OK");
                    return;
                }

                if (Compartment <= 0)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "El campo 'Compartment' debe ser mayor que 0", "OK");
                    return;
                }

                if (Stock <= 0)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "El campo 'Stock' debe ser mayor que 0", "OK");
                    return;
                }

                if (Ability <= 0)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "El campo 'Ability' debe ser mayor que 0", "OK");
                    return;
                }

                Tank = new TankModel
                {
                Number = Number,
                Compartment = Compartment,
                Ability = Ability,
                Stock = Stock
            };

            Request = new TankRequest
            {
                Request = Tank
            };

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var json = JsonSerializer.Serialize(Request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{Configuration.BaseUrl}/api/v1/tank", content);

            if (response.IsSuccessStatusCode)
            {
                await Application.Current.MainPage.DisplayAlert("Éxito", "Datos enviados correctamente", "OK");
                await GetTankAsync();
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

    public async Task GetTankAsync()
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
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/tank");
            var tanks = JsonSerializer.Deserialize<TankApiResponse>(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            TankList.Clear();
            foreach (var tank in tanks.Data)
            {
                TankList.Add(tank);
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


