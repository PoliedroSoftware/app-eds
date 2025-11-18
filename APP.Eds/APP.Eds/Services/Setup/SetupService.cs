using APP.Eds.Helpers;
using APP.Eds.Models.Business;
using APP.Eds.Models.Compartiment;
using APP.Eds.Models.Dispensers;
using APP.Eds.Models.Eds;
using APP.Eds.Models.Hose;
using APP.Eds.Models.Island;
using APP.Eds.Models.Islander;
using APP.Eds.Models.Product;
using APP.Eds.Models.Provider;
using APP.Eds.Models.Setup;
using APP.Eds.Models.Tank;
using APP.Eds.Services.Compartiment;
using APP.Eds.Services.Config;
using APP.Eds.Services.Dispensers;
using APP.Eds.Services.Eds;
using APP.Eds.Services.Hose;
using APP.Eds.Services.Island;
using APP.Eds.Services.Islander;
using APP.Eds.Services.Product;
using APP.Eds.Services.Tank;
using APP.Eds.UsesCases.Product;
using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows.Input;
using BusinessModel = APP.Eds.Models.Business.BusinessModel;

namespace APP.Eds.Services.Setup;

public class SetupService : INotifyPropertyChanged
{
    // Error detection keywords
    private static readonly string[] DuplicateErrorKeywords = { "ya existe", "already exists", "duplicate", "duplicado", "unique", "constraint" };

    private string? _authToken;
    public event PropertyChangedEventHandler? PropertyChanged;
    public VerticalStackLayout ExpanderContainer;

    // Collections
    public ObservableCollection<EditablePendingEds> EdsList { get; set; } = new();
    public ObservableCollection<string> EdsListNames { get; set; } = new();
    public ObservableCollection<EditablePendingIsland> Islands { get; set; } = new();
    public ObservableCollection<string> IslandsListNames { get; set; } = new();
    public ObservableCollection<EditablePendingTank> Tanks { get; set; } = new();
    public ObservableCollection<string> TanksListNames { get; set; } = new();
    public ObservableCollection<EditablePendingCompartiment> Compartiments { get; set; } = new();
    public ObservableCollection<string> CompartimentsListNames { get; set; } = new();
    public ObservableCollection<EditablePendingProduct> Products { get; set; } = new();
    public ObservableCollection<string> ProductsListNames { get; set; } = new();
    public ObservableCollection<EditablePendingIslander> Islanders { get; set; } = new();
    public ObservableCollection<ProviderModel> Providers { get; set; } = new();
    public ObservableCollection<EditablePendingDispenser> Dispensers { get; set; } = new();
    public ObservableCollection<string> DispensersListNames { get; set; } = new();
    public ObservableCollection<EditablePendingHose> Hoses { get; set; } = new();

    // help collections
    public ObservableCollection<ProductOption> ProductOptions { get; set; } = []; 
    public ObservableCollection<ProductTypeModelResponse> ProductTypeList { get; set; } = [];
    public ObservableCollection<DisperserTypeResponse> DispenserTypeList { get; set; } = [];

    private SetupRequest Request { get; set; }

    private SetupModel _setup;

    public SetupModel Setup
    {
        get => _setup;
        set
        {
            _setup = value;
            OnPropertyChanged(nameof(Setup));
        }
    }

    // Enhanced Form Properties
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

    private string _description;
    public string Description
    {
        get => _description;
        set
        {
            _description = value;
            OnPropertyChanged(nameof(Description));
        }
    }

    private string _businessNotes;
    public string BusinessNotes
    {
        get => _businessNotes;
        set
        {
            _businessNotes = value;
            OnPropertyChanged(nameof(BusinessNotes));
        }
    }

    private string _selectedStatus;
    public string SelectedStatus
    {
        get => _selectedStatus;
        set
        {
            _selectedStatus = value;
            OnPropertyChanged(nameof(SelectedStatus));
        }
    }

    private bool _isActive = true;
    public bool IsActive
    {
        get => _isActive;
        set
        {
            _isActive = value;
            OnPropertyChanged(nameof(IsActive));
        }
    }

    // Statistics Properties
    private int _totalBusinesses;
    public int TotalBusinesses
    {
        get => _totalBusinesses;
        set
        {
            _totalBusinesses = value;
            OnPropertyChanged(nameof(TotalBusinesses));
        }
    }

    private int _activeBusinesses;
    public int ActiveBusinesses
    {
        get => _activeBusinesses;
        set
        {
            _activeBusinesses = value;
            OnPropertyChanged(nameof(ActiveBusinesses));
        }
    }

    private int _totalEdsCount;
    public int TotalEdsCount
    {
        get => _totalEdsCount;
        set
        {
            _totalEdsCount = value;
            OnPropertyChanged(nameof(TotalEdsCount));
        }
    }

    private int _newBusinessesToday;
    public int NewBusinessesToday
    {
        get => _newBusinessesToday;
        set
        {
            _newBusinessesToday = value;
            OnPropertyChanged(nameof(NewBusinessesToday));
        }
    }

    // Filter Properties
    private Color _filterAllColor = Color.FromArgb("#3B82F6");
    public Color FilterAllColor
    {
        get => _filterAllColor;
        set
        {
            _filterAllColor = value;
            OnPropertyChanged(nameof(FilterAllColor));
        }
    }

    private Color _filterActiveColor = Color.FromArgb("#9E9E9E");
    public Color FilterActiveColor
    {
        get => _filterActiveColor;
        set
        {
            _filterActiveColor = value;
            OnPropertyChanged(nameof(FilterActiveColor));
        }
    }

    private Color _filterInactiveColor = Color.FromArgb("#9E9E9E");
    public Color FilterInactiveColor
    {
        get => _filterInactiveColor;
        set
        {
            _filterInactiveColor = value;
            OnPropertyChanged(nameof(FilterInactiveColor));
        }
    }

    // Commands
    public ICommand SaveDataCommand { get; }

    // Add commands 
    public ICommand AddEdsCommand { get; private set; }
    public ICommand AddIslandCommand { get; private set; }
    public ICommand AddTankCommand { get; private set; }
    public ICommand AddCompartimentCommand { get; private set; }
    public ICommand AddProductCommand { get; private set; }
    public ICommand AddIslanderCommand { get; private set; }
    public ICommand AddProviderCommand { get; private set; }
    public ICommand AddDispenserCommand { get; private set; }
    public ICommand AddHoseCommand { get; private set; }

    //REMOVE COMMANDS
    public ICommand RemoveEdsCommand { get; private set; }
    public ICommand RemoveIslandCommand { get; private set; }
    public ICommand RemoveTankCommand { get; private set; }
    public ICommand RemoveCompartimentCommand { get; private set; }
    public ICommand RemoveProductCommand { get; private set; }
    public ICommand RemoveIslanderCommand { get; private set; }
    public ICommand RemoveProviderCommand { get; private set; }
    public ICommand RemoveDispenserCommand { get; private set; }
    public ICommand RemoveHoseCommand { get; private set; }

    public SetupService(VerticalStackLayout expanderContainer)
    {
        ExpanderContainer = expanderContainer;
        InitializeCommands();
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
        SaveDataCommand = new Command(async () => await SaveDataAsync());
        InitializeProductOptions();
        GetAllProductTypeData();
        GetAllDispenserTypeData();
        LoadTraslationsAsync();
    }

    private void InitializeCommands()
    {
        //adds
        AddEdsCommand = new Command(() => AddEds());
        AddIslandCommand = new Command(() => AddIsland());
        AddTankCommand = new Command(() => AddTank());
        AddCompartimentCommand = new Command(() => AddCompartiment());
        AddProductCommand = new Command(() => AddProduct());
        AddIslanderCommand = new Command(() => AddIslander());
        AddProviderCommand = new Command(() => AddProvider());
        AddDispenserCommand = new Command(() => AddDispenser());
        AddHoseCommand = new Command(() => AddHose());
        //removes
        RemoveEdsCommand = new Command<EditablePendingEds>(RemoveEds);
        RemoveIslandCommand = new Command<EditablePendingIsland>(RemoveIsland);
        RemoveTankCommand = new Command<EditablePendingTank>(RemoveTank);
        RemoveCompartimentCommand = new Command<EditablePendingCompartiment>(RemoveCompartiment);
        RemoveProductCommand = new Command<EditablePendingProduct>(RemoveProduct);
        RemoveIslanderCommand = new Command<EditablePendingIslander>(RemoveIslander);
        RemoveProviderCommand = new Command<ProviderModel>(RemoveProvider);
        RemoveDispenserCommand = new Command<EditablePendingDispenser>(RemoveDispenser);
        RemoveHoseCommand = new Command<EditablePendingHose>(RemoveHose);
    }

    private void InitializeProductOptions()
    {
        // Inicializar las opciones de producto principales
        ProductOptions.Clear();
        ProductOptions.Add(new ProductOption(1, "Gasolina", "⛽"));
        ProductOptions.Add(new ProductOption(2, "ACPM", "🚛"));
        ProductOptions.Add(new ProductOption(3, "Urea", "🧪"));

        OnPropertyChanged(nameof(ProductOptions));
    }

    private List<Models.Eds.EdsModel> GetEds()
    {
        return EdsList.Select(x => new Models.Eds.EdsModel()
        {
            Name = x.Name,
            Nit = x.Nit,
            Address = x.Address,
            Sicom = x.Sicom,
        }).ToList();
    }

    private List<IslandModel> GetIslands()
    {
        return Islands.Select(x => new IslandModel()
        {
            Description = x.Name,
            NameEDS = x.SelectedEdsName
        }).ToList();
    }

    private List<TankModel> GetTanks()
    {
        return Tanks.Select(x => new TankModel()
        {
            Number = x.Number,
            Compartment = x.Compartment,
            Ability = x.Ability,
            NameEDS = x.SelectedEdsName,
        }).ToList();
    }

    private List<CompartimentModel> GetCompartiments()
    {
        return Compartiments.Select(x => new CompartimentModel()
        {
            Number = x.Number,
            Nominal = x.Nominal,
            Operative = x.Operative,
            Height = x.Height,
            NumberTank = Tanks.ToList().FirstOrDefault(y=> y.DisplayText == x.SelectedTankName)?.Number,
            NameProduct = x.SelectedProductName,
        }).ToList();
    }

    private List<ProductModel> GetProducts()
    {
        return Products.Select(x => new ProductModel()
        {
            Name = x.Name,
            IdProductType = x?.IdProductType ?? 0,
            SellPrice = x.SellPrice,
            PurchasePrice = x.PurchasePrice,
            Stock = x.Stock,
            NameEDS = x.SelectedEdsName
        }).ToList();
    }

    private List<IslanderModel> GetIslanders()
    {
        return Islanders.Select(x => new IslanderModel()
        {
            Name = x.Name,
            Email = x.Email,
            FirstName = x.FirstName,
            LastName = x.LastName,
            Password = x.Password,
            NameClaimToken = "OPERARIO",
            NameEDS = x.SelectedEdsName
        }).ToList();
    }

    private List<ProviderModel> GetProviders()
    {
        return Providers.Select(x => new ProviderModel()
        {
            Name = x.Name
        }).ToList();
    }

    private List<DispensersModel> GetDispensers()
    {
        return Dispensers.Select(x => new DispensersModel()
        {
            Code = x.Code,
            Number = x.Number,
            DispenserTypeId = x.SelectedDispenserType?.IdType ?? 0,
            HoseNumber = x.HoseNumber,
            NumberIsland = Islands.ToList().FindIndex(y=> y.Name == x.SelectedIslandName),
            NameEDS = x.SelectedEdsName
        }).ToList();
    }

    private List<HoseModel> GetHoses()
    {
        return Hoses.Select(x => new HoseModel()
        {
            Number = x.Number,
            AccumulatedAmount = x.AccumulatedAmount,
            AccumulatedGallons = x.AccumulatedGallons,
            IdProductType = x.SelectProductType?.IdProductType ?? 0,
            CodeDispenser = Dispensers.ToList().FirstOrDefault(y => y.DisplayName == x.SelectedDispenserName)?.Code,
            NumberCompartiment = Compartiments.ToList().FirstOrDefault(y => y.DisplayCompartiment == x.SelectedCompartimentName)?.Number
        }).ToList();
    }
    
    public async Task SaveDataAsync()
    {
        ValidateName();
        
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return;
        }
        CalculateNames();

        bool isInvalidForm = await IsInvalidateSetupAsync();

        if (isInvalidForm)
            return;

        try
        {
            Setup = new SetupModel
            {
                Bussiness = new BusinessModel()
                {
                    Name = Name
                },
                EDS = GetEds(),
                Islands = GetIslands(),
                Tanks = GetTanks(),
                Compartiments = GetCompartiments(),
                Dispensers = GetDispensers(),
                Hoses = GetHoses(),
                Products = GetProducts(),
                Islanders = GetIslanders(),
                Providers = GetProviders()
            };

            Request = new SetupRequest
            {
                Request = Setup
            };

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var json = JsonSerializer.Serialize(Request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var url = $"{Configuration.BaseUrl}/api/v1/bootstrap/setup";
            var response = await httpClient.PostAsync($"{Configuration.BaseUrl}/api/v1/bootstrap/setup", content);

            if (response.IsSuccessStatusCode)
            {
                await Application.Current.MainPage.DisplayAlert("Exito", "Negocio registrado correctamente", "OK");

                // Clear form
                Name = string.Empty;
                EdsList = new();
                Islands = new();
                Tanks = new();
                Compartiments = new();
                Products = new();
                Islanders = new();
                Providers = new();
                Dispensers = new();
                Hoses = new();
                IsActive = true;
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(EdsList));
                OnPropertyChanged(nameof(Tanks));
                OnPropertyChanged(nameof(Compartiments));
                OnPropertyChanged(nameof(Islands));
                OnPropertyChanged(nameof(Products));
                OnPropertyChanged(nameof(Islanders));
                OnPropertyChanged(nameof(Providers));
                OnPropertyChanged(nameof(Dispensers));
                OnPropertyChanged(nameof(Hoses));
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await HandleBusinessErrorAsync(response.StatusCode, error);
            }
        }
        catch (HttpRequestException httpEx)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error de Conexión",
                $"No se pudo conectar con el servidor.\n\nVerifica tu conexión a internet y que el servidor esté disponible.\n\nDetalles técnicos: {httpEx.Message}",
                "OK");
        }
        catch (TaskCanceledException)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Tiempo Agotado",
                "La operación tardó demasiado tiempo en responder.\n\nEl servidor puede estar sobrecargado. Intenta nuevamente en unos minutos.",
                "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error inesperado al enviar los datos: {ex.Message}", "OK");
        }
    }

    private void AddEds()
    {
        EdsList.Add(new EditablePendingEds
        {
            Name = "",
            Nit = "",
            Address = "",
            Sicom = ""
        });
        OnPropertyChanged(nameof(EdsList));
    }

    private void AddIsland()
    {
        Islands.Add(new EditablePendingIsland
        {
            Number = Islands.Count + 1
        });
        OnPropertyChanged(nameof(Islands));
        OnPropertyChanged(nameof(EdsList));
    }
    private void AddTank()
    {
        Tanks.Add(new EditablePendingTank
        {
        });
        OnPropertyChanged(nameof(Tanks));
    }
    private void AddCompartiment()
    {
        Compartiments.Add(new EditablePendingCompartiment
        {
        });
        OnPropertyChanged(nameof(Tanks));
        OnPropertyChanged(nameof(Products));
    }
    private void AddProduct()
    {
        Products.Add(new EditablePendingProduct
        {
        });
        OnPropertyChanged(nameof(EdsList));
        OnPropertyChanged(nameof(Products));
    }
    private void AddIslander()
    {
        Islanders.Add(new EditablePendingIslander
        {
            Email = string.Empty,
            FirstName = string.Empty,
            LastName = string.Empty,
            Name = string.Empty,
            Password = string.Empty,
        });
        OnPropertyChanged(nameof(Islanders));
    }
    private void AddProvider()
    {
        Providers.Add(new ProviderModel
        {
        });
        OnPropertyChanged(nameof(Providers));
    }

    private void AddDispenser()
    {
        Dispensers.Add(new EditablePendingDispenser
        {
        });
        OnPropertyChanged(nameof(EdsList));
        OnPropertyChanged(nameof(Islands));
        OnPropertyChanged(nameof(Dispensers));
    }

    private void AddHose()
    {
        Hoses.Add(new EditablePendingHose
        {
        });

        OnPropertyChanged(nameof(Dispensers));
        OnPropertyChanged(nameof(Compartiments));
        OnPropertyChanged(nameof(Hoses));
    }

    private void RemoveEds(EditablePendingEds eds)
    {
        if (EdsList.Contains(eds))
        {
            EdsList.Remove(eds);
        }
        OnPropertyChanged(nameof(EdsList));
    }

    private void RemoveIsland(EditablePendingIsland island)
    {
        if (Islands.Contains(island))
        {
            Islands.Remove(island);
        }
        OnPropertyChanged(nameof(Islands));
    }

    private void RemoveTank(EditablePendingTank tank)
    {
        if (Tanks.Contains(tank))
        {
            Tanks.Remove(tank);
        }
        OnPropertyChanged(nameof(Tanks));
    }

    private void RemoveCompartiment(EditablePendingCompartiment compartiment)
    {
        if (Compartiments.Contains(compartiment))
        {
            Compartiments.Remove(compartiment);
        }
        OnPropertyChanged(nameof(Compartiments));
    }

    private void RemoveProduct(EditablePendingProduct product)
    {
        if (Products.Contains(product))
        {
            Products.Remove(product);
        }
        OnPropertyChanged(nameof(Products));
    }

    private void RemoveIslander(EditablePendingIslander islander)
    {
        if (Islanders.Contains(islander))
        {
            Islanders.Remove(islander);
        }
        OnPropertyChanged(nameof(Islanders));
    }

    private void RemoveProvider(ProviderModel provider)
    {
        if (Providers.Contains(provider))
        {
            Providers.Remove(provider);
        }
        OnPropertyChanged(nameof(Providers));
    }

    private void RemoveDispenser(EditablePendingDispenser dispenser)
    {
        if (Dispensers.Contains(dispenser))
        {
            Dispensers.Remove(dispenser);
        }
        OnPropertyChanged(nameof(Dispensers));
    }

    private void RemoveHose(EditablePendingHose hose)
    {
        if (Hoses.Contains(hose))
        {
            Hoses.Remove(hose);
        }
        OnPropertyChanged(nameof(Hoses));
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

    private void UpdateProducTypeList(IEnumerable<ProductTypeModelResponse> Data)
    {
        ProductTypeList.Clear();
        foreach (var eds in Data)
        {
            ProductTypeList.Add(eds);
        }
    }

    private async void GetAllDispenserTypeData()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return;
        }
        try
        {
            string url = $"{Configuration.BaseUrl}/api/v1/dispenser-type";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.GetStringAsync(url);
            var dispenserTypeList = JsonSerializer.Deserialize<DispenserTypeResponseApi>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            UpdateDispenserTypeList(dispenserTypeList?.Data ?? new List<DisperserTypeResponse>());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando los datos: {ex.Message}");
        }
    }

    private void UpdateDispenserTypeList(IEnumerable<DisperserTypeResponse> disperserTypes)
    {
        DispenserTypeList.Clear();
        foreach (var item in disperserTypes)
        {
            DispenserTypeList.Add(item);
        }
    }

    public async Task<bool> IsInvalidateSetupAsync()
    {
        var invalidProperties = new List<string>();

        if (string.IsNullOrWhiteSpace(Name) || Name.Length < 3)
            invalidProperties.Add("El nombre no puede ser vacio y debe tener minimo 3 caracteres");
        
        if (EdsList == null || !EdsList.Any())
            invalidProperties.Add("Exista al menos una EDS");

        if (Tanks == null || !Tanks.Any())
            invalidProperties.Add("Exista al menos un Tanque");

        if (Compartiments == null || !Compartiments.Any())
            invalidProperties.Add("Exista al menos un Compartimiento");

        if (Products == null || !Products.Any())
            invalidProperties.Add("Exista al menos un Producto");

        if (Islanders == null || !Islanders.Any())
            invalidProperties.Add("Exista al menos un Islero");

        if (Islands == null || !Islands.Any())
            invalidProperties.Add("Exista al menos una Isla");

        if (Dispensers == null || !Dispensers.Any())
            invalidProperties.Add("Exista al menos un Dispensador");

        if (Hoses == null || !Hoses.Any())
            invalidProperties.Add("Exista al menos una Manguera");

        bool invalid = invalidProperties.Any();

        if(invalid)
            await Application.Current.MainPage.DisplayAlert(
                "Formulario Invalido",
                "Los datos enviados no son válidos.\n\n" +
                "Por favor, verifica que:\n" +
                string.Join("\n", invalidProperties),
                "OK");

        return invalid;
    }


    private async Task HandleBusinessErrorAsync(System.Net.HttpStatusCode statusCode, string errorResponse)
    {
        try
        {
            // Try to parse error response
            ErrorResponse? errorObj = null;
            try
            {
                errorObj = JsonSerializer.Deserialize<ErrorResponse>(errorResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                // If JSON parsing fails, continue with string-based error handling
            }

            // Handle specific error cases
            if (statusCode == System.Net.HttpStatusCode.Conflict)
            {
                // Generic error
                var errorMessage = errorObj?.Detail ?? "Error desconocido del servidor";
                // Generic validation error
                await Application.Current.MainPage.DisplayAlert(
                    "Error de Validación",
                    "Los datos enviados ya existen en el sistema.\n\n" +
                    "Por favor, verifica:\n" +
                    errorMessage,
                    "OK");
            }    
            else if (statusCode == System.Net.HttpStatusCode.BadRequest)
            {
                // Check for duplicate business name error
                if (IsDuplicateError(errorResponse))
                {
                    await ShowDuplicateBusinessErrorAsync();
                    return;
                }

                // Generic validation error
                await Application.Current.MainPage.DisplayAlert(
                    "Error de Validación",
                    "Los datos enviados no son válidos.\n\n" +
                    "Por favor, verifica que:\n" +
                    "• El nombre del negocio sea único\n" +
                    "• Todos los campos requeridos estén completos\n" +
                    "• Exista al menos una entidad de cada tipo\n" +
                    "• Los valores sean correctos",
                    "OK");
            }
            else if (statusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                // Check if it's a constraint violation (duplicate)
                if (IsDuplicateError(errorResponse))
                {
                    await ShowDuplicateBusinessErrorAsync();
                    return;
                }

                // Generic internal server error
                await Application.Current.MainPage.DisplayAlert(
                    "Error del Servidor",
                    "Error interno del servidor al procesar la solicitud.\n\n" +
                    "Intenta nuevamente en unos minutos. Si el problema persiste, contacta al administrador.",
                    "OK");
            }
            else if (statusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Sesión Expirada",
                    "Tu sesión ha expirado.\n\nPor favor, inicia sesión nuevamente.",
                    "OK");
            }
            else if (statusCode == System.Net.HttpStatusCode.Forbidden)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Acceso Denegado",
                    "No tienes permisos suficientes para registrar negocios.\n\nContacta al administrador.",
                    "OK");
            }
            else
            {
                // Generic error
                var errorMessage = errorObj?.Detail ?? "Error desconocido del servidor";
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"No se pudo registrar el negocio.\n\nCódigo de error: {statusCode}\n\n{errorMessage}",
                    "OK");
            }
        }
        catch
        {
            // Fallback error message
            await Application.Current.MainPage.DisplayAlert(
                "Error",
                $"No se pudo registrar el negocio.\n\nCódigo de error: {statusCode}\n\n" +
                "Si el problema persiste, contacta al administrador.",
                "OK");
        }
    }

    private bool IsDuplicateError(string errorResponse)
    {
        return DuplicateErrorKeywords.Any(keyword =>
            errorResponse.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private async Task ShowDuplicateBusinessErrorAsync()
    {
        await Application.Current.MainPage.DisplayAlert(
            "Negocio Duplicado",
            $"El nombre del negocio '{Name}' ya existe en la base de datos.\n\n" +
            "Por favor, ingrese un nombre diferente.",
            "OK");
    }

    private void ValidateName()
    {
        ShowNameError = string.IsNullOrWhiteSpace(Name) || Name.Length < 3;
    }

    private async void LoadTraslationsAsync()
    {
        // Implementation for loading translations if needed
        await Task.CompletedTask;
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public async void OnExpanderExpanded(object sender, EventArgs e)
    {
        if (sender is not Expander expandedExpander)
            return;

        if (!expandedExpander.IsExpanded)
            return;

        foreach (var child in ExpanderContainer.Children)
        {
            if (child is Expander expander && expander != expandedExpander)
            {
                expander.IsExpanded = false;
            }
        }

        OnPropertyChanged(nameof(EdsList));
        OnPropertyChanged(nameof(Tanks));
        OnPropertyChanged(nameof(Compartiments));
        OnPropertyChanged(nameof(Islands));
        OnPropertyChanged(nameof(Products));
        OnPropertyChanged(nameof(Islanders));
        OnPropertyChanged(nameof(Providers));
        OnPropertyChanged(nameof(Dispensers));
        OnPropertyChanged(nameof(Hoses));
        CalculateNames();
    }

    private void CalculateNames()
    {
        CalculateEdsNames();
        CalculateIslandsNames();
        CalculateTanksNames();
        CalculateCompartimentsNames();
        CalculateProductsNames();
        CalculateDispensersNames();
    }

    private void CalculateEdsNames()
    {
        var names = EdsList.ToList().Select(x => x.Name).ToArray();

        for (int i = 0; i < names.Length; i++)
        {
            if(EdsListNames.Count > i)
            {
                EdsListNames[i] = names[i];
            }
            else
            {
                EdsListNames.Add(names[i]);
            }
        }

        OnPropertyChanged(nameof(EdsListNames));
    }

    private void CalculateIslandsNames()
    {
        var names = Islands.ToList().Select(x => x.Name).ToArray();

        for (int i = 0; i < names.Length; i++)
        {
            if (IslandsListNames.Count > i)
            {
                IslandsListNames[i] = names[i];
            }
            else
            {
                IslandsListNames.Add(names[i]);
            }
        }

        OnPropertyChanged(nameof(IslandsListNames));
    }

    private void CalculateTanksNames()
    {
        var names = Tanks.ToList().Select(x => x.DisplayText).ToArray();

        for (int i = 0; i < names.Length; i++)
        {
            if (TanksListNames.Count > i)
            {
                TanksListNames[i] = names[i];
            }
            else
            {
                TanksListNames.Add(names[i]);
            }
        }

        OnPropertyChanged(nameof(TanksListNames));
    }

    private void CalculateCompartimentsNames()
    {
        var names = Compartiments.ToList().Select(x => x.DisplayCompartiment).ToArray();

        for (int i = 0; i < names.Length; i++)
        {
            if (CompartimentsListNames.Count > i)
            {
                CompartimentsListNames[i] = names[i];
            }
            else
            {
                CompartimentsListNames.Add(names[i]);
            }
        }

        OnPropertyChanged(nameof(CompartimentsListNames));
    }

    private void CalculateProductsNames()
    {
        var names = Products.ToList().Select(x => x.Name).ToArray();

        for (int i = 0; i < names.Length; i++)
        {
            if (ProductsListNames.Count > i)
            {
                ProductsListNames[i] = names[i];
            }
            else
            {
                ProductsListNames.Add(names[i]);
            }
        }

        OnPropertyChanged(nameof(ProductsListNames));
    }

    private void CalculateDispensersNames()
    {
        var names = Dispensers.ToList().Select(x => x.DisplayName).ToArray();

        for (int i = 0; i < names.Length; i++)
        {
            if (DispensersListNames.Count > i)
            {
                DispensersListNames[i] = names[i];
            }
            else
            {
                DispensersListNames.Add(names[i]);
            }
        }

        OnPropertyChanged(nameof(DispensersListNames));
    }
}