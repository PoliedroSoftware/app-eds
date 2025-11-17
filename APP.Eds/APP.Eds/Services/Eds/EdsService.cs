using APP.Eds.Models.Eds;
using System.ComponentModel;
using System.Text.Json;
using System.Text;
using System.Windows.Input;
using System.Collections.ObjectModel;
using APP.Eds.Services.Config;
using APP.Eds.Helpers;
using System.Net.Http.Headers;
using APP.Eds.Services.Translations;

namespace APP.Eds.Services.Eds;

public class EditablePendingEds : INotifyPropertyChanged
{
    public string Nit { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Sicom { get; set; } = string.Empty;
    public int IdBusiness { get; set; }
    private string _name;

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged("EdsList");
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class EdsService : INotifyPropertyChanged
{
    private string? _authToken;
    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<BusinessModel> BusinessList { get; set; } = [];
    public ObservableCollection<EdsResponse> EdsList { get; set; } = [];
    private EdsRequest Request { get; set; }
    private EdsModel _eds;
    private TranslationsService _traslationService = new TranslationsService();


    public EdsModel Eds
    {
        get => _eds;
        set
        {
            _eds = value;
            OnPropertyChanged(nameof(Eds));
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

    private string _nit;
    public string Nit
    {
        get => _nit;
        set
        {
            _nit = value;
            OnPropertyChanged(nameof(Nit));
        }
    }

    private string _address;
    public string Address
    {
        get => _address;
        set
        {
            _address = value;
            OnPropertyChanged(nameof(Address));
        }
    }

    private string _sicom;
    public string Sicom
    {
        get => _sicom;
        set
        {
            _sicom = value;
            OnPropertyChanged(nameof(Sicom));
        }
    }

    private string _namePlaceHolder;
    public string NamePlaceHolder
    {
        get => _namePlaceHolder;
        set
        {
            _namePlaceHolder = value;
            OnPropertyChanged(nameof(NamePlaceHolder));
        }
    }

    private string _edsTitle;
    public string EdsTitle
    {
        get => _edsTitle;
        set
        {
            _edsTitle = value;
            OnPropertyChanged(nameof(EdsTitle));
        }
    }

    private string _nitPlaceHolder;
    public string NitPlaceHolder
    {
        get => _nitPlaceHolder;
        set
        {
            _nitPlaceHolder = value;
            OnPropertyChanged(nameof(NitPlaceHolder));
        }
    }

    private string _adreessPlaceHolder;
    public string AdreessPlaceHolder
    {
        get => _adreessPlaceHolder;
        set
        {
            _adreessPlaceHolder = value;
            OnPropertyChanged(nameof(AdreessPlaceHolder));
        }
    }

    private string _sicomPlaceHolder;
    public string SicomPlaceHolder
    {
        get => _sicomPlaceHolder;
        set
        {
            _sicomPlaceHolder = value;
            OnPropertyChanged(nameof(SicomPlaceHolder));
        }
    }

    private string _selectBusiness;
    public string SelectBusiness
    {
        get => _selectBusiness;
        set
        {
            _selectBusiness = value;
            OnPropertyChanged(nameof(SelectBusiness));
        }
    }

    private string _businessPicker;
    public string BusinessPicker
    {
        get => _businessPicker;
        set
        {
            _businessPicker = value;
            OnPropertyChanged(nameof(BusinessPicker));
        }
    }

    private string _edsListTranslation;
    public string EdsListTranslation
    {
        get => _edsListTranslation;
        set
        {
            _edsListTranslation = value;
            OnPropertyChanged(nameof(EdsListTranslation));
        }
    }

    private string _errorEmpty;
    public string ErrorEmpty
    {
        get => _errorEmpty;
        set
        {
            _errorEmpty = value;
            OnPropertyChanged(nameof(ErrorEmpty));
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


    private BusinessModel _selectedBusiness;
    public BusinessModel SelectedBusiness
    {
        get => _selectedBusiness;
        set
        {
            _selectedBusiness = value;
            OnPropertyChanged(nameof(SelectedBusiness));
            if (_selectedBusiness != null)
            {
                IdBusiness = _selectedBusiness.IdBusiness;
            }
        }
    }

    private int _idBusiness;
    public int IdBusiness
    {
        get => _idBusiness;
        set
        {
            _idBusiness = value;
            OnPropertyChanged(nameof(IdBusiness));
        }
    }


    public ICommand GetByIdEdsDataCommand { get; }
    public ICommand SaveEdsDataCommand { get; }

    public EdsService()
    {
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);

        GetAllBusinessData();
        GetEdssAsync();
        GetByIdEdsDataCommand = new Command<int>(async (edsId) => await GetByIdEdsDataAsync(edsId));
        SaveEdsDataCommand = new Command(async () => await SaveEdsDataAsync());
        LoadTraslationsAsync();
    }

    private async void GetAllBusinessData()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return;
        }
        try
        {
            string url = $"{Configuration.BaseUrl}/api/v1/business?PageNumber=1&PageSize=100";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.GetStringAsync(url);
            var businessList = JsonSerializer.Deserialize<BusinessResponseModel>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            UpdateBusinessList(businessList?.Data ?? new List<BusinessModel>());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando los datos: {ex.Message}");
        }
    }

    public async Task GetByIdEdsDataAsync(int edsId)
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
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/eds/{edsId}");
            Console.WriteLine(response);

            Eds = JsonSerializer.Deserialize<EdsModel>(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo cargar el dato: {ex.Message}", "OK");
        }
    }

    public async Task SaveEdsDataAsync()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return;
        }
        try
        {
            if (SelectedBusiness is null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Por favor, seleccione un Negocio", "OK");
                return;
            }

            Eds = new EdsModel
            {
                Name = Name,
                Nit = Nit,
                Address = Address,
                Sicom = Sicom,
                IdBusiness = SelectedBusiness.IdBusiness
            };

            Request = new EdsRequest
            {
                Request = Eds
            };

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var json = JsonSerializer.Serialize(Request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{Configuration.BaseUrl}/api/v1/eds", content);

            if (response.IsSuccessStatusCode)
            {
                await Application.Current.MainPage.DisplayAlert("Éxito", "Datos enviados correctamente", "OK");
                await GetEdssAsync();
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

    private void UpdateBusinessList(IEnumerable<BusinessModel> businessData)
    {
        BusinessList.Clear();
        foreach (var business in businessData)
        {
            BusinessList.Add(business);
        }
    }

    public async Task GetEdssAsync()
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
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/eds");
            var edss = JsonSerializer.Deserialize<EdsApiResponse>(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            EdsList.Clear();
            foreach (var eds in edss.Data)
            {
                EdsList.Add(eds);
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

    public async Task LoadTraslationsAsync()
    {
        var result = await _traslationService.GetTranslationsByLanguageAsync("es-CO");
        GlobalTranslations.SetTranslations(result ?? []);
        EdsTitle = GlobalTranslations.Get("EdsTitle");
        NamePlaceHolder = GlobalTranslations.Get("NamePlaceHolder");
        NitPlaceHolder = GlobalTranslations.Get("NitPlaceHolder");
        AdreessPlaceHolder = GlobalTranslations.Get("AdreessPlaceHolder");
        SicomPlaceHolder = GlobalTranslations.Get("SicomPLaceHolder");
        SelectBusiness = GlobalTranslations.Get("SelectBussines");
        BusinessPicker = GlobalTranslations.Get("BusinessPicker");
        EdsListTranslation = GlobalTranslations.Get("EdsListTranslation");
        ErrorEmpty = GlobalTranslations.Get("ErrorEmpty");
        SendData = GlobalTranslations.Get("SendData");
    }
}
