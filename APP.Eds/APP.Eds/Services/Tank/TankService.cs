using APP.Eds.Helpers;
using APP.Eds.Models.Common; 
using APP.Eds.Models.Tank;
using APP.Eds.Services.Config;
using APP.Eds.Services.Translations;
using Microsoft.Maui.ApplicationModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using System.Windows.Input;

namespace APP.Eds.Services.Tank;

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
            Tank = JsonConvert.DeserializeObject<TankModel>(response);
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

                if (!Compartment.HasValue || Compartment <= 0)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "El campo 'Compartment' debe ser mayor que 0", "OK");
                    return;
                }

                if (!Ability.HasValue || Ability <= 0)
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
            var json = JsonConvert.SerializeObject(Request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{Configuration.BaseUrl}/api/v1/tank", content);

            if (response.IsSuccessStatusCode)
            {
                await Application.Current.MainPage.DisplayAlert("Éxito", "Datos enviados correctamente", "OK");
                await GetTankAsync();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Error al enviar datos del tanque: {response.StatusCode}\n{errorContent}");
                
                string errorMessage = "Error interno del servidor al guardar los tanques. Por favor, contacte a soporte técnico.";
                try 
                {
                    var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
                    if (errorResponse != null && !string.IsNullOrEmpty(errorResponse.Detail))
                    {
                        errorMessage = errorResponse.Detail;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al deserializar respuesta de error: {ex.Message}");
                }
                
                await Application.Current.MainPage.DisplayAlert("Error", errorMessage, "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error al enviar los datos: {ex.Message}", "OK");
        }
    }

    public async Task GetTankAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                throw new Exception("No se encontró el token de autenticación");
            }

            using var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var requestUri = $"{Configuration.BaseUrl}/api/v1/tank";
            System.Diagnostics.Debug.WriteLine($"Realizando petición GET a: {requestUri}");

            var httpResponse = await httpClient.GetAsync(requestUri);
            var responseContent = await httpResponse.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"Respuesta recibida: {responseContent}");

            if (!httpResponse.IsSuccessStatusCode)
            {
                var errorDetail = "";
                try
                {
                    var error = JsonConvert.DeserializeObject<ErrorResponse>(responseContent);
                    errorDetail = $"{error.Type}: {error.Detail}";
                }
                catch
                {
                    errorDetail = responseContent;
                }

                System.Diagnostics.Debug.WriteLine($"Error del servidor ({httpResponse.StatusCode}): {errorDetail}");
                string displayMessage = "Error interno del servidor al cargar los tanques. Por favor, contacte a soporte técnico.";
                try
                {
                    var error = JsonConvert.DeserializeObject<ErrorResponse>(responseContent);
                    if (error != null && !string.IsNullOrEmpty(error.Detail))
                    {
                        displayMessage = error.Detail;
                    }
                    else if (error != null && !string.IsNullOrEmpty(error.Title))
                    {
                        displayMessage = error.Title;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al deserializar ErrorResponse en GetTankAsync: {ex.Message}");
                }

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error de Carga de Tanques",
                        displayMessage,
                        "OK"
                    );
                });
                return;
            }

            var tanks = JsonConvert.DeserializeObject<TankApiResponse>(responseContent);

            if (tanks == null || tanks.Data == null)
            {
                throw new Exception("La respuesta del servidor no contiene datos válidos");
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                TankList.Clear();
                foreach (var tank in tanks.Data)
                {
                    TankList.Add(tank);
                }
            });
        }
        catch (HttpRequestException hex)
        {
            var message = hex.InnerException?.Message ?? hex.Message;
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error de Conexión",
                    $"No se pudo conectar al servidor. Verifique su conexión a internet.\n\nDetalles: {message}",
                    "OK"
                );
            });
        }
        catch (TaskCanceledException)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Tiempo de Espera Agotado",
                    "La conexión al servidor ha tardado demasiado. Por favor, inténtelo de nuevo.",
                    "OK"
                );
            });
        }
        catch (Exception ex)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error de Carga de Tanques",
                    "No se pudieron cargar los tanques. Por favor, intente de nuevo más tarde o contacte a soporte técnico.",
                    "OK"
                );
            });
            System.Diagnostics.Debug.WriteLine($"Error en GetTankAsync: {ex}");
        }
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public async Task InitializeAsync()
    {
        await GetTankAsync();
    }
}
