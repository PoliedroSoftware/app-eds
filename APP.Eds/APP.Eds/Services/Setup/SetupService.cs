using APP.Eds.Helpers;
using APP.Eds.Models.Business;
using APP.Eds.Models.Compartiment;
using APP.Eds.Models.Dispensers;
using APP.Eds.Models.Eds;
using APP.Eds.Models.Island;
using APP.Eds.Models.Islander;
using APP.Eds.Models.Product;
using APP.Eds.Models.Provider;
using APP.Eds.Models.Setup;
using APP.Eds.Models.Tank;
using APP.Eds.Services.Compartiment;
using APP.Eds.Services.Config;
using APP.Eds.Services.Island;
using APP.Eds.Services.Islander;
using APP.Eds.Services.Product;
using APP.Eds.Services.Tank;
using APP.Eds.UsesCases.Product;
using System.Collections.ObjectModel;
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

    // Collections
    public ObservableCollection<Models.Eds.EdsModel> EdsList { get; set; } = new();
    public ObservableCollection<EditablePendingIsland> Islands { get; set; } = new();
    public ObservableCollection<EditablePendingTank> Tanks { get; set; } = new();
    public ObservableCollection<EditablePendingCompartiment> Compartiments { get; set; } = new();
    public ObservableCollection<EditablePendingProduct> Products { get; set; } = new();
    public ObservableCollection<EditablePendingIslander> Islanders { get; set; } = new();
    public ObservableCollection<ProviderModel> Providers { get; set; } = new();

    // help collections
    public ObservableCollection<ProductOption> ProductOptions { get; set; } = [];

    private SetupRequest Request { get; set; }
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

    //REMOVE COMMANDS
    public ICommand RemoveEdsCommand { get; private set; }
    public ICommand RemoveIslandCommand { get; private set; }
    public ICommand RemoveTankCommand { get; private set; }
    public ICommand RemoveCompartimentCommand { get; private set; }
    public ICommand RemoveProductCommand { get; private set; }
    public ICommand RemoveIslanderCommand { get; private set; }
    public ICommand RemoveProviderCommand { get; private set; }

    public SetupService()
    {
        InitializeCommands();
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
        SaveDataCommand = new Command(async () => await SaveDataAsync());
        InitializeProductOptions();
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
        //removes
        RemoveEdsCommand = new Command<Models.Eds.EdsModel>(RemoveEds);
        RemoveIslandCommand = new Command<EditablePendingIsland>(RemoveIsland);
        RemoveTankCommand = new Command<EditablePendingTank>(RemoveTank);
        RemoveCompartimentCommand = new Command<EditablePendingCompartiment>(RemoveCompartiment);
        RemoveProductCommand = new Command<EditablePendingProduct>(RemoveProduct);
        RemoveIslanderCommand = new Command<EditablePendingIslander>(RemoveIslander);
        RemoveProviderCommand = new Command<ProviderModel>(RemoveProvider);
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
        }).ToList();
    }

    private List<TankModel> GetTanks()
    {
        return Tanks.Select(x => new TankModel()
        {
            Number = x.Number,
            Compartment = x.Compartment,
            Ability = x.Ability,
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
            NumberTank = x.SelectedTank?.Number,
            NameProduct = x.SelectedProduct?.Name,
        }).ToList();
    }

    private List<ProductModel> GetProducts()
    {
        return Products.Select(x => new ProductModel()
        {
            Name = x.Name,
            IdProductType = x.IdProductType,
            SellPrice = x.SellPrice,
            PurchasePrice = x.PurchasePrice,
            Stock = x.Stock,
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
            NameEDS = x.SelectedEds.Name
        }).ToList();
    }

    private List<ProviderModel> GetProviders()
    {
        return Providers.Select(x => new ProviderModel()
        {
            Name = x.Name
        }).ToList();
    }

    public async Task SaveDataAsync()
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
            Setup = new SetupModel
            {
                Bussiness = Business,
                EDS = GetEds(),
                Islands = GetIslands(),
                Tanks = GetTanks(),
                Compartiments = GetCompartiments(),
                // Dispensers = GetDispensers()
                //Hoses =GetHoses()
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
            var response = await httpClient.PostAsync($"{Configuration.BaseUrl}/api/v1/bootstrap/setup", content);

            if (response.IsSuccessStatusCode)
            {
                await Application.Current.MainPage.DisplayAlert("Exito", "Negocio registrado correctamente", "OK");

              

                // Clear form
                Name = string.Empty;
                IsActive = true;
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
        EdsList.Add(new Models.Eds.EdsModel
        {
            Name = "",
            Nit = "",
            Address = "",
            Sicom = ""
        });
    }

    private void AddIsland()
    {
        Islands.Add(new EditablePendingIsland
        {
            Number = Islands.Count + 1
        });
    }
    private void AddTank()
    {
        Tanks.Add(new EditablePendingTank
        {
        });
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
    }
    private void AddProvider()
    {
        Providers.Add(new ProviderModel
        {
        });
    }

    private void RemoveEds(Models.Eds.EdsModel eds)
    {
        if (EdsList.Contains(eds))
        {
            EdsList.Remove(eds);
        }
    }
    private void RemoveIsland(EditablePendingIsland island)
    {
        if (Islands.Contains(island))
        {
            Islands.Remove(island);
        }
    }
    private void RemoveTank(EditablePendingTank tank)
    {
        if (Tanks.Contains(tank))
        {
            Tanks.Remove(tank);
        }
    }
    private void RemoveCompartiment(EditablePendingCompartiment compartiment)
    {
        if (Compartiments.Contains(compartiment))
        {
            Compartiments.Remove(compartiment);
        }
    }
    private void RemoveProduct(EditablePendingProduct product)
    {
        if (Products.Contains(product))
        {
            Products.Remove(product);
        }
    }
    private void RemoveIslander(EditablePendingIslander islander)
    {
        if (Islanders.Contains(islander))
        {
            Islanders.Remove(islander);
        }
    }
    private void RemoveProvider(ProviderModel provider)
    {
        if (Providers.Contains(provider))
        {
            Providers.Remove(provider);
        }
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
            if (statusCode == System.Net.HttpStatusCode.BadRequest)
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
}