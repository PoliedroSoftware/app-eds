using APP.Eds.Models.Business;
using System.ComponentModel;
using System.Text.Json;
using System.Text;
using System.Windows.Input;
using APP.Eds.Services.Config;
using System.Collections.ObjectModel;
using APP.Eds.Models.Eds;
using BusinessModel = APP.Eds.Models.Business.BusinessModel;
using APP.Eds.Helpers;
using System.Net.Http.Headers;
using Microsoft.Maui.Controls;
using APP.Eds.Services.Translations;

namespace APP.Eds.Services.Business;

public class BusinessService : INotifyPropertyChanged

{
    private string? _authToken;
    public readonly TranslationsService _translations= new TranslationsService();

    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<BusinessModel> BusinessList { get; set; } = [];
    private BusinessRequest Request { get; set; }
    private BusinessModel _business;
    public BusinessModel Business
    {
        get => _business;
        set
        {
            _business = value;
            OnPropertyChanged(nameof(Business));
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

    //public string TranslatedNameKey => GlobalTranslations.Get("NameKey");
    //public string TranslatedEnterBusinessPlaceholderKey => GlobalTranslations.Get("EnterBusinessPlaceholderKey");
    //public string TranslatedSendDataButtonKey => GlobalTranslations.Get("SendDataButtonKey");
    private string _translatedNamekey;
    public string TranslatedNameKey
    {
        get => _translatedNamekey;
        set
        {
            _translatedNamekey = value; 
            OnPropertyChanged(nameof(TranslatedNameKey));
        }
    }
    private string _translatedSendData;
    public string TranslatedSendData
    {
        get => _translatedSendData;
        set
        {
            _translatedSendData = value;
            OnPropertyChanged(nameof(TranslatedSendData));
        }
    }
    private string _translatedBusinessPlaceholder;
    public string TranslatedBusinessPlaceholder
    {
        get => _translatedBusinessPlaceholder;
        set
        {
            _translatedBusinessPlaceholder = value;
            OnPropertyChanged(nameof(TranslatedBusinessPlaceholder));
        }
    }

    private string _TranslatedBusinessmanagement;
    public string TranslatedBusinessmanagement
    {
        get => _TranslatedBusinessmanagement;
        set
        {
            _TranslatedBusinessmanagement = value;
            OnPropertyChanged(nameof(TranslatedBusinessmanagement));
        }
    }





    public ICommand GetByIdBusinessDataCommand { get; }
    public ICommand SaveBusinessDataCommand { get; }
 
    public BusinessService()
    {
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
        GetByIdBusinessDataCommand = new Command<int>(async (businessId) => await GetByIdBusinessDataAsync(businessId));
        SaveBusinessDataCommand = new Command(async () => await SaveBusinessDataAsync());
        LoadTranslationsAsync();
    }
    public async Task LoadTranslationsAsync()
    {
        var result = await _translations.GetTranslationsByLanguageAsync("es-CO");
        GlobalTranslations.SetTranslations(result ?? []);
        //SendData = GlobalTranslations.Get("SendData"); 134.122.125.60:8085
        TranslatedNameKey = GlobalTranslations.Get("BusinessPicker");
        TranslatedSendData = GlobalTranslations.Get("SendData");
        TranslatedBusinessPlaceholder = GlobalTranslations.Get("TranslatedBusinessPlaceholder");
        TranslatedBusinessmanagement = GlobalTranslations.Get("Businessmanagement");

    }

    public async Task GetByIdBusinessDataAsync(int businessId)
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
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/business/{businessId}");
            Console.WriteLine(response);

            Business = JsonSerializer.Deserialize<BusinessModel>(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo cargar el dato: {ex.Message}", "OK");
        }
    }

    public async Task SaveBusinessDataAsync()
    {
        
        if (string.IsNullOrWhiteSpace(Name))
        {

            await Application.Current.MainPage.DisplayAlert("Error de Validación", "Debe ingresar los datos en el campo de nombre.", "Aceptar");
            return; 
        }

        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return;
        }
        try
        {
            Business = new BusinessModel
            {
                Name = Name
            };

            Request = new BusinessRequest
            {
                Request = Business
            };

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var json = JsonSerializer.Serialize(Request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{Configuration.BaseUrl}/api/v1/business", content);

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

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}