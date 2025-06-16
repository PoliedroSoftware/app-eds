using APP.Eds.Helpers;
using APP.Eds.Models.Dispenser;
using APP.Eds.Models.Hose;
using APP.Eds.Models.Product;
using APP.Eds.Services.Config;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows.Input;

namespace APP.Eds.Services.Hose;

public class HoseService : INotifyPropertyChanged
{
    private string? _authToken;
    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<DispenserModelResponse> DispensersList { get; set; } = [];
    public ObservableCollection<ProductTypeModelResponse> ProductTypeList { get; set; } = [];
    public ObservableCollection<HoseResponse> HoseList { get; set; } = [];

    private HoseRequest Request { get; set; }
    private HoseModel _hose;

    public HoseModel Hose
    {
        get => _hose;
        set
        {
            _hose = value;
            OnPropertyChanged(nameof(Hose));
        }
    }

    private int _number;
    public int Number
    {
        get => _number;
        set
        {
            _number = value;
            OnPropertyChanged(nameof(Number));
        }
    }

    private DispenserModelResponse _selectedDispensers;
    public DispenserModelResponse SelectedDispensers
    {
        get => _selectedDispensers;
        set
        {
            _selectedDispensers = value;
            OnPropertyChanged(nameof(SelectedDispensers));
            if (_selectedDispensers != null)
            {
                IdDispensers = _selectedDispensers.IdDispensers;
            }
        }
    }

    private int _idDispensers;
    public int IdDispensers
    {
        get => _idDispensers;
        set
        {
            _idDispensers = value;
            OnPropertyChanged(nameof(IdDispensers));
        }
    }


    private int _idIdProductType;
    public int IdIdProductType
    {
        get => _idIdProductType;
        set
        {
            _idIdProductType = value;
            OnPropertyChanged(nameof(IdIdProductType));
        }
    }


    private double _accumulatedAmount;
    public double AccumulatedAmount
    {
        get => _accumulatedAmount;
        set
        {
            _accumulatedAmount = value;
            OnPropertyChanged(nameof(AccumulatedAmount));
        }
    }

    private double _accumulatedGallons;
    public double AccumulatedGallons
    {
        get => _accumulatedGallons;
        set
        {
            _accumulatedGallons = value;
            OnPropertyChanged(nameof(AccumulatedGallons));
        }
    }

    private ProductTypeModelResponse _selectedProductType;
    public ProductTypeModelResponse SelectProductType
    {
        get => _selectedProductType;
        set
        {
            _selectedProductType = value;
            OnPropertyChanged(nameof(SelectProductType));
            if (_selectedProductType != null)
            {
                IdIdProductType = _selectedProductType.IdProductType;
            }
        }
    }

    public ICommand GetByIdHoseDataCommand { get; }
    public ICommand SaveHoseDataCommand { get; }

    public HoseService()
    {
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
        GetAllDispensersData();
        GetAllProductTypeData();
        GetHoseAsync();
        GetByIdHoseDataCommand = new Command<int>(async (hoseId) => await GetByIdHoseDataAsync(hoseId));
        SaveHoseDataCommand = new Command(async () => await SaveHoseDataAsync());
        
    }

    private async void GetAllDispensersData()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return;
        }
        try
        {
            string url = $"{Configuration.BaseUrl}/api/v1/dispensers";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.GetStringAsync(url);
            var dispensersList = JsonSerializer.Deserialize<DispensersResponseModel>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            UpdateDispensersList(dispensersList?.Data ?? new List<DispenserModelResponse>());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando los datos: {ex.Message}");
        }
    }

    public async Task GetByIdHoseDataAsync(int hoseId)
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
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/hose/{hoseId}");
            Hose = JsonSerializer.Deserialize<HoseModel>(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo cargar el dato: {ex.Message}", "OK");
        }
    }

    public async Task SaveHoseDataAsync()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return;
        }
        try
        {
            if (SelectedDispensers is null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Por favor, seleccione un Dispensers", "OK");
                return;
            }

            if (SelectProductType is null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Por favor, seleccione un tipo de producto", "OK");
                return;
            }

            if (Number <= 0)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "El campo 'Number' debe ser mayor que 0", "OK");
                return;
            }

            if (AccumulatedAmount <= 0)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "El campo 'AccumulatedAmount' debe ser mayor que 0", "OK");
                return;
            }

            if (AccumulatedGallons <= 0)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "El campo 'AccumulatedGallons' debe ser mayor que 0", "OK");
                return;
            }

            Hose = new HoseModel
            {
                Number = Number,
                AccumulatedAmount = AccumulatedAmount,
                AccumulatedGallons = AccumulatedGallons,
                IdDispensers = SelectedDispensers.IdDispensers,
                IdProductType = SelectProductType.IdProductType
            };

            Request = new HoseRequest
            {
                Request = Hose
            };

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var json = JsonSerializer.Serialize(Request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{Configuration.BaseUrl}/api/v1/hose", content);

            if (response.IsSuccessStatusCode)
            {
                await Application.Current.MainPage.DisplayAlert("Éxito", "Datos enviados correctamente", "OK");
                await GetHoseAsync();
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

    private void UpdateDispensersList(IEnumerable<DispenserModelResponse> dispensersData)
    {
        DispensersList.Clear();
        foreach (var dispensers in dispensersData)
        {
            DispensersList.Add(dispensers);
        }
    }

    public async Task GetHoseAsync()
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
            var hoses = JsonSerializer.Deserialize<HoseApiResponse>(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            HoseList.Clear();
            foreach (var hose in hoses.Data)
            {
                HoseList.Add(hose);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private async void GetAllProductTypeData()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return;
        }
        try
        {
            string url = $"{Configuration.BaseUrl}/api/v1/producttype";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.GetStringAsync(url);
            var ProductTypeList = JsonSerializer.Deserialize<ProductTypeResponse>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            UpdateProducTypeList(ProductTypeList.Data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando los datos: {ex.Message}");
        }
    }

    private void UpdateProducTypeList(IEnumerable<ProductTypeModelResponse> Data)
    {
        ProductTypeList.Clear();
        foreach (var eds in Data)
        {
            ProductTypeList.Add(eds);
        }
    }



    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}


