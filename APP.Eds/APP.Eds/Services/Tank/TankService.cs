using APP.Eds.Helpers;
using APP.Eds.Models.Tank;
using APP.Eds.Services.Config;
using APP.Eds.Services.Translations;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows.Input;

namespace APP.Eds.Services.Tank;

public class EditablePendingTank : INotifyPropertyChanged
{
    public int? Compartment { get; set; }
    public string? Number { get; set; }
    public double? Ability { get; set; }
    public double? Stock { get; set; }

    public string DisplayText => $"Tanque {Number} - Capacidad {Ability:N0} L";

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class TankService : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<TankResponse> TankList { get; set; } = [];

    private TankRequest Request { get; set; }
    private TankModel _tank;
    private readonly TranslationsService _translations = new TranslationsService();
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

    private int? _compartment;
    public int? Compartment
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

    private double? _ability;
    public double? Ability
    {
        get => _ability;
        set
        {
            _ability = value;
            OnPropertyChanged(nameof(Ability));
        }
    }

    private double? _stock;
    public double? Stock
    {
        get => _stock;
        set
        {
            _stock = value;
            OnPropertyChanged(nameof(Stock));
        }
    }

    // Translations
    private string _gestionTankTitle;
    public string GestionTankTitle
    {
        get => _gestionTankTitle;
        set
        {
            _gestionTankTitle = value;
            OnPropertyChanged(nameof(GestionTankTitle));
        }
    }

    private string _sendData;
    public string SendData
    {
        get => _sendData;
        set
        {
            _sendData = value;
            OnPropertyChanged(nameof(SendData));
        }
    }
    private string _tankNumberPlaceholder;
    public string TankNumberPlaceholder
    {
        get => _tankNumberPlaceholder;
        set
        {
            _tankNumberPlaceholder = value;
            OnPropertyChanged(nameof(TankNumberPlaceholder));
        }
    }
    private string _tankNumberLabel;
    public string TankNumberLabel
    {
        get => _tankNumberLabel;
        set
        {
            _tankNumberLabel = value;
            OnPropertyChanged(nameof(TankNumberLabel));
        }
    }
    private string _tankCompartmentLabel;
    public string TankCompartmentLabel
    {
        get => _tankCompartmentLabel;
        set
        {
            _tankCompartmentLabel = value;
            OnPropertyChanged(nameof(TankCompartmentLabel));
        }
    }
    private string _compartmentPlacheholder;
    public string CompartmentPlacheholder
    {
        get => _compartmentPlacheholder;
        set
        {
            _compartmentPlacheholder = value;
            OnPropertyChanged(nameof(CompartmentPlacheholder));
        }
    }
    private string _stockLabel;
    public string StockLabel
    {
        get => _stockLabel;
        set
        {
            _stockLabel = value;
            OnPropertyChanged(nameof(StockLabel));
}
    }
    private string _stockPlaceholder;
    public string StockPlaceholder
    {
        get => _stockPlaceholder;
        set
        {
            _stockPlaceholder = value;
            OnPropertyChanged(nameof(StockPlaceholder));
        }
    }
    private string _capacity;
    public string Capacity
    {
        get => _capacity;
        set
        {
            _capacity = value;
            OnPropertyChanged(nameof(Capacity));
        }
    }
    private string _capacityPlaceholder;
    public string CapacityPlaceholder
    {
        get => _capacityPlaceholder;
        set
        {
            _capacityPlaceholder = value;
            OnPropertyChanged(nameof(CapacityPlaceholder));
        }
    }
    //Error handling
    private string _error;
    public string Error
    {
        get => _error;
        set
        {
            _error = value;
            OnPropertyChanged(nameof(Error));
        }
    }
    private string _errorEnterNumber;
    public string ErrorEnterNumber
    {
        get => _errorEnterNumber;
        set
        {
            _errorEnterNumber = value;
            OnPropertyChanged(nameof(ErrorEnterNumber));
        }
    }
    private string _errorNegativeNumber;
    public string ErrorNegativeNumber
    {
        get => _errorNegativeNumber;
        set
        {
            _errorNegativeNumber = value;
            OnPropertyChanged(nameof(ErrorNegativeNumber));
        }
    }
    private string _errorStockNegative;
    public string ErrorStockNegative
    {
        get => _errorStockNegative;
        set
        {
            _errorStockNegative = value;
            OnPropertyChanged(nameof(ErrorStockNegative));
        }
    }
    private string _errorAbilityNegative;
    public string ErrorAbilityNegative
    {
        get => _errorAbilityNegative;
        set
        {
            _errorAbilityNegative = value;
            OnPropertyChanged(nameof(ErrorAbilityNegative));
        }
    }
    private string _tankListTitle;
    public string TankListTitle
    {
        get => _tankListTitle;
        set
        {
            _tankListTitle = value;
            OnPropertyChanged(nameof(TankListTitle));
        }
    }


    public ICommand GetByIdTankDataCommand { get; }
    public ICommand SaveTankDataCommand { get; }

    public TankService()
    {
        GetByIdTankDataCommand = new Command<int>(async (tankId) => await GetByIdTankDataAsync(tankId));
        SaveTankDataCommand = new Command(async () => await SaveTankDataAsync());
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
        GetTankAsync();
        LoadTranslationsAsync();
    }

    public async Task LoadTranslationsAsync()
    {
        var result = await _translations.GetTranslationsByLanguageAsync("es-CO");
        GlobalTranslations.SetTranslations(result ?? []);
        SendData = GlobalTranslations.Get("SendData");
        TankNumberPlaceholder = GlobalTranslations.Get("TankNumberPlaceholder");
        TankNumberLabel = GlobalTranslations.Get("TankNumberLabel");
        GestionTankTitle = GlobalTranslations.Get("GestionTankTitle");
        TankCompartmentLabel = GlobalTranslations.Get("TankCompartmentLabel");
        CompartmentPlacheholder = GlobalTranslations.Get("CompartmentPlacheholder");
        StockLabel = GlobalTranslations.Get("StockLabel");
        StockPlaceholder = GlobalTranslations.Get("StockPlaceholder");
        Capacity = GlobalTranslations.Get("Capacity");
        CapacityPlaceholder = GlobalTranslations.Get("CapacityPlaceholder");
        Error = GlobalTranslations.Get("Error");
        ErrorEnterNumber = GlobalTranslations.Get("ErrorEnterNumber");
        ErrorNegativeNumber = GlobalTranslations.Get("ErrorNegativeNumber");
        ErrorStockNegative = GlobalTranslations.Get("ErrorStockNegative");
        ErrorAbilityNegative = GlobalTranslations.Get("ErrorAbilityNegative");
        TankListTitle = GlobalTranslations.Get("TankListTitle");

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


