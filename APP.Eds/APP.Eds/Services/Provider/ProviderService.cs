using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows.Input;
using APP.Eds.Helpers;
using APP.Eds.Models.Provider;
using APP.Eds.Services.Config;

namespace APP.Eds.Services.Provider;

public class ProviderService : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private ProviderRequest Request { get; set; }
    private ProviderModel _provider;
    public ObservableCollection<ProviderResponse> ProviderList { get; set; } = [];
    private string? _authToken;
    public ProviderModel Provider
    {
        get => _provider;
        set
        {
            _provider = value;
            OnPropertyChanged(nameof(Provider));
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
    public ICommand GetByIdProviderDataCommand { get; }
    public ICommand SaveProviderDataCommand { get; }

    public ProviderService()
    {
        GetByIdProviderDataCommand = new Command<int>(async (providerId) => await GetByIdProviderDataAsync(providerId));
        SaveProviderDataCommand = new Command(async () => await SaveProviderDataAsync());
        _authToken = TokenHelper.LoadToken();
        GetProvidersAsync();
    }

    public async Task GetByIdProviderDataAsync(int providerId)
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
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/provider/{providerId}");
            Console.WriteLine(response);

            Provider = JsonSerializer.Deserialize<ProviderModel>(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo cargar el dato: {ex.Message}", "OK");
        }
    }

    public async Task SaveProviderDataAsync()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return;
        }
        try
        {
            Provider = new ProviderModel
            {
                Name = Name
            };

            Request = new ProviderRequest
            {
                Request = Provider
            };

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var json = JsonSerializer.Serialize(Request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{Configuration.BaseUrl}/api/v1/provider", content);

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

    public async Task GetProvidersAsync()
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
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/provider?PageNumber=1&PageSize=100");
            Console.WriteLine(response);
            var providers = JsonSerializer.Deserialize<ProviderApiResponse>(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            ProviderList.Clear();
            foreach (var provider in providers.Data)
            {
                ProviderList.Add(provider);
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