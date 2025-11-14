using APP.Eds.Helpers;
using APP.Eds.Models.Common;
using APP.Eds.Models.Compartiment;
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

public class EditablePendingHose : INotifyPropertyChanged
{
    public int Number { get; set; }
    public double AccumulatedAmount { get; set; }
    public double AccumulatedGallons { get; set; }
    public string SelectedCompartimentName { get; set; }
    public string SelectedDispenserName { get; set; }
    public ProductTypeModelResponse SelectProductType { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
public class HoseService : INotifyPropertyChanged
{
    private string? _authToken;
    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<DispenserModelResponse> DispensersList { get; set; } = [];
    public ObservableCollection<ProductTypeModelResponse> ProductTypeList { get; set; } = [];
    public ObservableCollection<CompartimentResponse> CompartimentsList { get; set; } = [];
    public ObservableCollection<HoseResponse> HoseList { get; set; } = [];
    public ObservableCollection<ProductModelResponse> Products { get; set; } = new();

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

    private int _idHose;
    public int IdHose
    {
        get => _idHose;
        set
        {
            _idHose = value;
            OnPropertyChanged(nameof(IdHose));
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

    private int _idIdCompartment;
    public int IdIdCompartment
    {
        get => _idIdCompartment;
        set
        {
            _idIdCompartment = value;
            OnPropertyChanged(nameof(IdIdCompartment));
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

    private CompartimentResponse _selectCompartiment;
    public CompartimentResponse SelectCompartiment
    {
        get => _selectCompartiment;
        set
        {
            _selectCompartiment = value;
            OnPropertyChanged(nameof(SelectCompartiment));
            if (_selectCompartiment != null)
            {
                IdIdCompartment = _selectCompartiment.IdCompartment;
            }
        }
    }
    
    public ICommand GetByIdHoseDataCommand { get; }
    public ICommand SaveHoseDataCommand { get; }
    public ICommand EditHoseDataCommand { get; private set; }

    public HoseService()
    {
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
        IdHose = 0;
        InitializeService();
        GetByIdHoseDataCommand = new Command<int>(async (hoseId) => await GetByIdHoseDataAsync(hoseId));
        SaveHoseDataCommand = new Command(async () => await SaveHoseDataAsync());
        EditHoseDataCommand = new Command<HoseResponse>(async (dispenser) => await EditDispenserAsync(dispenser));
    }

    private async Task InitializeService()
    {
        await GetHoseAsync();
        await GetAllDispensersData();
        await GetAllProductTypeData();
        await GetAllProductData();
        await GetAllCompartimentData();
    }

    private async Task GetAllDispensersData()
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

            if(IdHose > 0)
            {
                await UpdateAsync();
            }
            else
            {
                await CreateAsync();
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error al enviar los datos: {ex.Message}", "OK");
        }
    }

    private async Task UpdateAsync()
    {
        Hose = new HoseModel
        {
            IdHose = IdHose,
            Number = Number,
            AccumulatedAmount = AccumulatedAmount,
            AccumulatedGallons = AccumulatedGallons,
            IdDispensers = SelectedDispensers.IdDispensers,
            IdProductType = SelectProductType.IdProductType,
            IdCompartiment = SelectCompartiment.IdCompartment
        };

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
        var json = JsonSerializer.Serialize(Hose, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await httpClient.PutAsync($"{Configuration.BaseUrl}/api/v1/hose", content);

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
        IdHose = 0;
    }

    private async Task CreateAsync()
    {
        Hose = new HoseModel
        {
            Number = Number,
            AccumulatedAmount = AccumulatedAmount,
            AccumulatedGallons = AccumulatedGallons,
            IdDispensers = SelectedDispensers.IdDispensers,
            IdProductType = SelectProductType.IdProductType,
            IdCompartiment = SelectCompartiment.IdCompartment
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

    private void UpdateDispensersList(IEnumerable<DispenserModelResponse> dispensersData)
    {
        DispensersList.Clear();
        foreach (var dispensers in dispensersData)
        {
            DispensersList.Add(dispensers);
        }
        EnrichHoseListWithNames();
    }

    private void UpdateProducTypeList(IEnumerable<ProductTypeModelResponse> Data)
    {
        ProductTypeList.Clear();
        foreach (var eds in Data)
        {
            ProductTypeList.Add(eds);
        }
        EnrichHoseListWithNames();
    }

    private void UpdateCompartimentsList(IEnumerable<CompartimentResponse> Data)
    {
        CompartimentsList.Clear();
        foreach (var eds in Data)
            CompartimentsList.Add(eds);

        EnrichCompartimentsWithProductNames();
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
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/hose");
            var hoses = JsonSerializer.Deserialize<HoseApiResponse>(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            HoseList.Clear();
            foreach (var hose in hoses.Data)
            {
                HoseList.Add(hose);
            }
            EnrichHoseListWithNames();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private void EnrichHoseListWithNames()
    {
        foreach (var hose in HoseList)
        {
            hose.DispenserName = DispensersList.FirstOrDefault(d => d.IdDispensers == hose.IdDispensers)?.Code ?? string.Empty;
            hose.ProductTypeName = ProductTypeList.FirstOrDefault(p => p.IdProductType == hose.IdProductType)?.Description ?? string.Empty;
        }
    }

    private async Task GetAllProductTypeData()
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
    private async Task GetAllProductData()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return;
        }

        try
        {
            string url = $"{Configuration.BaseUrl}/api/v1/product";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            var httpResponse = await httpClient.GetAsync(url);
            if (!httpResponse.IsSuccessStatusCode)
            {
                var err = await httpResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Product GET {httpResponse.StatusCode}. Body: {err}");
                await Application.Current.MainPage.DisplayAlert("Error productos", $"Status: {httpResponse.StatusCode}", "OK");
                UpdateProductList(Array.Empty<ProductModelResponse>());
                return;
            }

            var response = await httpResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"🟦 Product raw (first 400): {response.Substring(0, Math.Min(400, response.Length))}");

            var products = ApiResponseUnwrapper.UnwrapDataList<ProductModelResponse>(response);
            Console.WriteLine($"✅ Products parsed: {products.Count}");

            foreach (var p in products.Take(5))
                Console.WriteLine($"   → {p.IdProduct}: {p.Name}");

            UpdateProductList(products);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando productos: {ex}");
            UpdateProductList(Array.Empty<ProductModelResponse>());
        }
    }
    private void UpdateProductList(IEnumerable<ProductModelResponse> data)
    {
        Products.Clear();
        foreach (var p in data)
            Products.Add(p);

        EnrichCompartimentsWithProductNames();
    }

    private async Task GetAllCompartimentData()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return;
        }

        try
        {
            string url = $"{Configuration.BaseUrl}/api/v1/compartiment";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            var httpResponse = await httpClient.GetAsync(url);
            if (!httpResponse.IsSuccessStatusCode)
            {
                var err = await httpResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Compartiment GET {httpResponse.StatusCode}. Body: {err}");
                await Application.Current.MainPage.DisplayAlert("Error compartimentos", $"Status: {httpResponse.StatusCode}", "OK");
                UpdateCompartimentsList(Array.Empty<CompartimentResponse>());
                return;
            }

            var response = await httpResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"🟪 Compartiment raw (first 400): {response.Substring(0, Math.Min(400, response.Length))}");

            var compartiments = ApiResponseUnwrapper.UnwrapDataList<CompartimentResponse>(response);
            Console.WriteLine($"✅ Compartiments parsed: {compartiments.Count}");

            foreach (var c in compartiments.Take(5))
                Console.WriteLine($"   → CompId={c.IdCompartment}, IdProduct={c.IdProduct}");

            UpdateCompartimentsList(compartiments);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando compartimentos: {ex}");
            UpdateCompartimentsList(Array.Empty<CompartimentResponse>());
        }
    }


    private void EnrichCompartimentsWithProductNames()
    {
        if (Products.Count == 0 || CompartimentsList.Count == 0) return;

        var dict = Products.ToDictionary(p => p.IdProduct, p => p.Name);

        for (int i = 0; i < CompartimentsList.Count; i++)
        {
            var c = CompartimentsList[i];
            c.ProductName = dict.TryGetValue(c.IdProduct, out var name) ? name : "-";

            CompartimentsList[i] = c;
        }
    }

    private async Task EditDispenserAsync(HoseResponse hose)
    {
        try
        {
            IdHose = hose.IdHose;
            Number = hose.Number;
            AccumulatedAmount = (double)hose.AccumulatedAmount;
            AccumulatedGallons = (double)hose.AccumulatedGallons;
            IdDispensers = hose.IdDispensers;

            // Find and select the corresponding items in the dropdowns
            SelectedDispensers = DispensersList.FirstOrDefault(x => x.IdDispensers == hose.IdDispensers);
            SelectProductType = ProductTypeList.FirstOrDefault(x => x.IdProductType == hose.IdProductType);
            SelectCompartiment = CompartimentsList.FirstOrDefault(x => x.IdCompartment == hose.IdCompartiment);

            await Application.Current.MainPage.DisplayAlert("Modo Edicion", $"Datos de la manguera '{hose.DispenserName}' cargados para edicion", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error editando dispensador: {ex.Message}", "OK");
        }
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}


