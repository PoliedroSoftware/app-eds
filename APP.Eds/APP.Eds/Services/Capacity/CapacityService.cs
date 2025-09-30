using APP.Eds.Models.Capacity;
using System.ComponentModel;
using System.Text.Json;
using System.Text;
using System.Windows.Input;
using APP.Eds.Services.Config;
using APP.Eds.Helpers;
using System.Net.Http.Headers;
using APP.Eds.Models.Translations;

namespace APP.Eds.Services.Capacity;

public class CapacityService : INotifyPropertyChanged
{
    private string? _authToken;
    public event PropertyChangedEventHandler? PropertyChanged;
    private CapacityRequest Request { get; set; }
    private CapacityModel _capacity;
    public ICommand GetByIdCapacityDataCommand { get; }
    public ICommand SaveCapacityDataCommand { get; }
    public CapacityModel Capacity
    {
        get => _capacity;
        set
        {
            _capacity = value;
            OnPropertyChanged(nameof(Capacity));
        }
    }

    private string _gestionCapacityLabel;
    public string GestionCapacityLabel
    {
        get => _gestionCapacityLabel;
        set
        {
            _gestionCapacityLabel = value;
            OnPropertyChanged(nameof(GestionCapacityLabel));
        }
    }
    private string _codeLabel;
    public string CodeLabel
    {
        get => _codeLabel;
        set
        {
            _codeLabel = value;
            OnPropertyChanged(nameof(CodeLabel));
        }
    }
    private string _CodePlaceholder;
    public string CodePlaceholder
    {
        get => _CodePlaceholder;
        set
        {
            _CodePlaceholder = value;
            OnPropertyChanged(nameof(CodePlaceholder));
        }
    }
    private string? _code;
    public string? Code
    {
        get => _code;
        set
        {
            _code = value;
            OnPropertyChanged(nameof(Code));
        }
    }

    private string _heightLabel;
    public string HeightLabel
    {
        get => _heightLabel;
        set
        {
            _heightLabel = value;
            OnPropertyChanged(nameof(HeightLabel));
        }
    }
    private string _heightPlaceholder;
    public string HeightPlaceholder
    {
        get => _heightPlaceholder;
        set
        {
            _heightPlaceholder = value;
            OnPropertyChanged(nameof(HeightPlaceholder));
        }
    }
    private double? _height;
    public double? Height
    {
        get => _height;
        set
        {
            _height = value;
            OnPropertyChanged(nameof(Height));
        }
    }

    private string _gallonLabel;
    public string GallonLabel
    {
        get => _gallonLabel;
        set
        {
            _gallonLabel = value;
            OnPropertyChanged(nameof(GallonLabel));
        }
    }
    private string _gallonPlaceholder;
    public string GallonPlaceholder
    {
        get => _gallonPlaceholder;
        set
        {
            _gallonPlaceholder = value;
            OnPropertyChanged(nameof(GallonPlaceholder));
        }
    }


    private bool _isUpdating = false;

    private double? _gallon;
    public double? Gallon
    {
        get => _gallon;
        set
        {
            if (_gallon == value) return;

            _gallon = value;
            OnPropertyChanged(nameof(Gallon));

            if (_isUpdating) return;      
            _isUpdating = true;
            try
            {
                if (value.HasValue && value.Value > 0)
                {
                    var liters = value.Value * 3.78541;
                    _liters = Math.Round(liters, 5);   
                }
                else
                {
                    _liters = null;
                }
                OnPropertyChanged(nameof(Liters));
            }
            finally { _isUpdating = false; }
        }
    }

    private double? _liters;
    public double? Liters
    {
        get => _liters;
        set
        {
            if (_liters == value) return;

            _liters = value;
            OnPropertyChanged(nameof(Liters));

            if (_isUpdating) return;

            _isUpdating = true;
            try
            {
                if (value.HasValue && value > 0)
                {
                    var calculatedGallons = value.Value / 3.78541;
                    _gallon = Math.Round(calculatedGallons, 5);   
                }
                else
                {
                    _gallon = null;                            
                }
                OnPropertyChanged(nameof(Gallon));             
            }
            finally
            {
                _isUpdating = false;
            }
        }
    }


    private string _litersLabel;
    public string LitersLabel
    {
        get => _litersLabel;
        set
        {
            _litersLabel = value;
            OnPropertyChanged(nameof(LitersLabel));
        }
    }
    private string _litersPlaceholder;
    public string LitersPlaceholder
    {
        get => _litersPlaceholder;
        set
        {
            _litersPlaceholder = value;
            OnPropertyChanged(nameof(LitersPlaceholder));
        }
    }

    private string _SendData;
    public string SendData
    {
        get => _SendData;
        set
        {
            _SendData = value;
            OnPropertyChanged(nameof(SendData));
        }
    }
    // Error messages
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
    private string _errorAllFieldsRequired;
    public string ErrorAllFieldsRequired
    {
        get => _errorAllFieldsRequired;
        set
        {
            _errorAllFieldsRequired = value;
            OnPropertyChanged(nameof(ErrorAllFieldsRequired));
        }
    }
    private string _errorTokenNoFound;
    public string ErrorTokenNoFound
    {
        get => _errorTokenNoFound;
        set
        {
            _errorTokenNoFound = value;
            OnPropertyChanged(nameof(ErrorTokenNoFound));
        }
    }
    private string _errorDataLoad;
    public string ErrorDataLoad
    {
        get => _errorDataLoad;
        set
        {
            _errorDataLoad = value;
            OnPropertyChanged(nameof(ErrorDataLoad));
        }
    }
    private string _errorSendData;
    public string ErrorSendData
    {
        get => _errorSendData;
        set
        {
            _errorSendData = value;
            OnPropertyChanged(nameof(ErrorSendData));
        }
    }
    // Success messages
    private string _success;
    public string Success
    {
        get => _success;
        set
        {
            _success = value;
            OnPropertyChanged(nameof(Success));
        }
    }
    private string _successDataSend;
    public string SuccessDataSend
    {
        get => _successDataSend;
        set
        {
            _successDataSend = value;
            OnPropertyChanged(nameof(SuccessDataSend));
        }
    }

    public async Task<Dictionary<string, string>> GetTranslationsByLanguageAsync(string languageTag)
    {
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Authentication token is missing", "OK");
            return new Dictionary<string, string>();
        }
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
        var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/translations");
        var data = JsonSerializer.Deserialize<TranslationsResponse>(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return data.Translations.TryGetValue(languageTag, out var translations)
            ? translations
            : new Dictionary<string, string>();
    }
    public CapacityService()
    {
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
        GetByIdCapacityDataCommand = new Command<int>(async (capacityId) => await GetByIdCapacityDataAsync(capacityId));
        SaveCapacityDataCommand = new Command(async () => await SaveCapacityDataAsync());
        LoadTranslationsAsync();
    }
    public async Task LoadTranslationsAsync()
    {
        var result = await GetTranslationsByLanguageAsync("es-CO");
        GlobalTranslations.SetTranslations(result ?? []);
        GestionCapacityLabel = GlobalTranslations.Get("GestionCapacity");
        SendData = GlobalTranslations.Get("SendData");
        CodeLabel = GlobalTranslations.Get("CodeLabel");
        CodePlaceholder = GlobalTranslations.Get("CodePlaceholder");
        HeightPlaceholder = GlobalTranslations.Get("HeightPlaceholder");
        HeightLabel = GlobalTranslations.Get("HeightLabel");
        GallonPlaceholder = GlobalTranslations.Get("GallonPlaceholder");
        GallonLabel = GlobalTranslations.Get("GallonLabel");
        LitersPlaceholder = GlobalTranslations.Get("LitersPlaceholder");
        LitersLabel = GlobalTranslations.Get("LitersLabel");
        Error = GlobalTranslations.Get("Error");
        ErrorAllFieldsRequired = GlobalTranslations.Get("ErrorAllFieldsRequired");
        ErrorTokenNoFound = GlobalTranslations.Get("ErrorTokenNoFound");
        ErrorDataLoad = GlobalTranslations.Get("ErrorDataLoad");
        ErrorSendData = GlobalTranslations.Get("ErrorSendData");
        Success = GlobalTranslations.Get("Success");
        SuccessDataSend = GlobalTranslations.Get("SuccessDataSend");
    }
    public async Task GetByIdCapacityDataAsync(int capacityId)
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert(Error, ErrorTokenNoFound, "OK");
            return;
        }
        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/capacity/{capacityId}");
            Console.WriteLine(response);

            Capacity = JsonSerializer.Deserialize<CapacityModel>(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(Error, $"{ErrorDataLoad}, {ex.Message}", "OK");
        }
    }

    public async Task SaveCapacityDataAsync()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert(Error, ErrorTokenNoFound, "OK");
            return;
        }
        try
        {
            Capacity = new CapacityModel
            {
                Code = Code ?? string.Empty,
                Height = Height.GetValueOrDefault(),
                Gallon = Gallon.GetValueOrDefault(),
                Liters = (int?)Liters.GetValueOrDefault()
            };


            Request = new CapacityRequest
            {
                Request = Capacity
            };

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var json = JsonSerializer.Serialize(Request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{Configuration.BaseUrl}/api/v1/capacity", content);

            if (response.IsSuccessStatusCode)
            {
                await Application.Current.MainPage.DisplayAlert(Success, SuccessDataSend, "OK");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await Application.Current.MainPage.DisplayAlert(Error, $"{ErrorSendData}, {response.StatusCode}\n{error}", "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(Error, $"{ErrorSendData}, {ex.Message}", "OK");
        }
    }


    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
   
}
