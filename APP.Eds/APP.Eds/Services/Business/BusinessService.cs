using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows.Input;
using APP.Eds.Components.PopUp;
using APP.Eds.Helpers;
using APP.Eds.Models.Business;
using APP.Eds.Models.Eds;
using APP.Eds.Services.Config;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls; 
using BusinessModel = APP.Eds.Models.Business.BusinessModel;

namespace APP.Eds.Services.Business;

public class BusinessService : INotifyPropertyChanged
{
    private string? _authToken;
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
            ValidateName();
            OnPropertyChanged(nameof(Name));
        }
    }

    private bool _showNameError;
    public bool ShowNameError
    {
        get => _showNameError;
        set
        {
            if (_showNameError != value)
            {
                _showNameError = value;
                OnPropertyChanged(nameof(ShowNameError));
            }
        }
    }

    public ICommand GetByIdBusinessDataCommand { get; }
    public ICommand SaveBusinessDataCommand { get; }
    public ICommand EditBusinessCommand { get; }
    public ICommand DeleteBusinessCommand { get; }

    public BusinessService()
    {
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
        GetByIdBusinessDataCommand = new Command<int>(async (businessId) => await GetByIdBusinessDataAsync(businessId));
        SaveBusinessDataCommand = new Command(async () => await SaveBusinessDataAsync());
        EditBusinessCommand = new Command<BusinessModel>(async (business) => await EditBusiness(business));
        DeleteBusinessCommand = new Command<BusinessModel>(async (business) => await DeleteBusiness(business));
    }
   public async Task GetBusinessList()
   {
       await GetBusinessesAsync();
   }

    public async Task GetBusinessesAsync(int pageNumber = 1, int pageSize = 100)
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return;
        }

        try
        {
            string url = $"{Configuration.BaseUrl}/api/v1/business?PageNumber={pageNumber}&PageSize={pageSize}";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.GetStringAsync(url);
            var businessList = JsonSerializer.Deserialize<BusinessResponseModel>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            BusinessList.Clear();
            IEnumerable<BusinessModel> data;
            if (businessList != null && businessList.Data != null)
            {
                data = businessList.Data.Select(b => new BusinessModel
                {
                    IdBusiness = b.IdBusiness,
                    Name = b.Name
                });
            }
            else
            {
                data = Array.Empty<BusinessModel>();
            }

            foreach (var b in data)
            {
                BusinessList.Add(b);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando negocios: {ex.Message}");
        }
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
        ValidateName();
        if (ShowNameError || string.IsNullOrWhiteSpace(Name))
        {
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
                await Application.Current.MainPage.ShowPopupAsync(new OkBusiness(this));
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
   private async Task EditBusiness(BusinessModel business)
   {
      await Application.Current.MainPage.DisplayAlert("Editar", $"Has seleccionado editar a {business.Name}", "OK");
   }

   private async Task DeleteBusiness(BusinessModel business)
   {
      if (string.IsNullOrEmpty(_authToken))
      {
          await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
          return;
      }

      bool confirmed = await Application.Current.MainPage.DisplayAlert("Confirmar", $"¿Estás seguro de que quieres eliminar a {business.Name}?", "Sí", "No");
      if (!confirmed)
          return;

      try
      {
          using var httpClient = new HttpClient();
          httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
          var response = await httpClient.DeleteAsync($"{Configuration.BaseUrl}/api/v1/business/{business.IdBusiness}");

          if (response.IsSuccessStatusCode)
          {
              await GetBusinessList();
          }
          else
          {
              var error = await response.Content.ReadAsStringAsync();
              await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo eliminar el negocio: {response.StatusCode}\n{error}", "OK");
          }
      }
      catch (Exception ex)
      {
          await Application.Current.MainPage.DisplayAlert("Error", $"Error al eliminar el negocio: {ex.Message}", "OK");
      }
   }

    private void ValidateName()
    {
        ShowNameError = string.IsNullOrWhiteSpace(Name) || !System.Text.RegularExpressions.Regex.IsMatch(Name, @"^[a-zA-Z\s]*$");
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}