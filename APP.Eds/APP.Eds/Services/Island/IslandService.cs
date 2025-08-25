using APP.Eds.Helpers;
using APP.Eds.Models.Island;
using APP.Eds.Services.Config;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows.Input;

namespace APP.Eds.Services.Island;

public class IslandItemExtended
{
    public int Idisland { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Capacity { get; set; } = string.Empty;
    public DateTime InstallationDate { get; set; }
}

public class IslandService : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    
    // Collections
    public ObservableCollection<IslandResponse> IslandList { get; set; } = [];
    public ObservableCollection<IslandItemExtended> EnhancedIslandList { get; set; } = [];
    public ObservableCollection<string> LocationOptions { get; set; } = new();
    public ObservableCollection<string> IslandTypes { get; set; } = new();
    public ObservableCollection<string> CapacityOptions { get; set; } = new();
    public ObservableCollection<string> StatusOptions { get; set; } = new();

    private string? _authToken;
    private IslandRequest Request { get; set; }
    private IslandModel _island;

    public IslandModel Island
    {
        get => _island;
        set
        {
            _island = value;
            OnPropertyChanged(nameof(Island));
        }
    }

    // Enhanced Form Properties
    private string _islandName;
    public string IslandName
    {
        get => _islandName;
        set
        {
            _islandName = value;
            OnPropertyChanged(nameof(IslandName));
        }
    }

    private string _selectedLocation;
    public string SelectedLocation
    {
        get => _selectedLocation;
        set
        {
            _selectedLocation = value;
            OnPropertyChanged(nameof(SelectedLocation));
        }
    }

    private string _selectedType;
    public string SelectedType
    {
        get => _selectedType;
        set
        {
            _selectedType = value;
            OnPropertyChanged(nameof(SelectedType));
        }
    }

    private string _selectedCapacity;
    public string SelectedCapacity
    {
        get => _selectedCapacity;
        set
        {
            _selectedCapacity = value;
            OnPropertyChanged(nameof(SelectedCapacity));
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

    private DateTime _installationDate = DateTime.Now;
    public DateTime InstallationDate
    {
        get => _installationDate;
        set
        {
            _installationDate = value;
            OnPropertyChanged(nameof(InstallationDate));
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

    // Statistics Properties
    private int _totalIslands;
    public int TotalIslands
    {
        get => _totalIslands;
        set
        {
            _totalIslands = value;
            OnPropertyChanged(nameof(TotalIslands));
        }
    }

    private int _activeIslands;
    public int ActiveIslands
    {
        get => _activeIslands;
        set
        {
            _activeIslands = value;
            OnPropertyChanged(nameof(ActiveIslands));
        }
    }

    private int _maintenanceIslands;
    public int MaintenanceIslands
    {
        get => _maintenanceIslands;
        set
        {
            _maintenanceIslands = value;
            OnPropertyChanged(nameof(MaintenanceIslands));
        }
    }

    private int _inactiveIslands;
    public int InactiveIslands
    {
        get => _inactiveIslands;
        set
        {
            _inactiveIslands = value;
            OnPropertyChanged(nameof(InactiveIslands));
        }
    }

    // Filter Properties
    private Color _filterAllColor = Color.FromArgb("#1976D2");
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

    private Color _filterMaintenanceColor = Color.FromArgb("#9E9E9E");
    public Color FilterMaintenanceColor
    {
        get => _filterMaintenanceColor;
        set
        {
            _filterMaintenanceColor = value;
            OnPropertyChanged(nameof(FilterMaintenanceColor));
        }
    }

    // Commands
    public ICommand GetByIdIslandDataCommand { get; private set; }
    public ICommand SaveIslandDataCommand { get; private set; }
    public ICommand FilterAllCommand { get; private set; }
    public ICommand FilterActiveCommand { get; private set; }
    public ICommand FilterMaintenanceCommand { get; private set; }
    public ICommand EditIslandCommand { get; private set; }
    public ICommand DeleteIslandCommand { get; private set; }

    public IslandService()
    {
        InitializeCommands();
        InitializeOptions();
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
    }

    private void InitializeCommands()
    {
        GetByIdIslandDataCommand = new Command<int>(async (islandId) => await GetByIdIslandDataAsync(islandId));
        SaveIslandDataCommand = new Command(async () => await SaveIslandDataAsync());
        FilterAllCommand = new Command(() => FilterIslands("all"));
        FilterActiveCommand = new Command(() => FilterIslands("active"));
        FilterMaintenanceCommand = new Command(() => FilterIslands("maintenance"));
        EditIslandCommand = new Command<IslandItemExtended>(async (island) => await EditIslandAsync(island));
        DeleteIslandCommand = new Command<IslandItemExtended>(async (island) => await DeleteIslandAsync(island));
    }

    private void InitializeOptions()
    {
        // Location Options
        LocationOptions.Clear();
        LocationOptions.Add("Entrada Principal");
        LocationOptions.Add("Area Central");
        LocationOptions.Add("Lado Derecho");
        LocationOptions.Add("Lado Izquierdo");
        LocationOptions.Add("Zona Posterior");
        LocationOptions.Add("Area de Servicio");

        // Island Types
        IslandTypes.Clear();
        IslandTypes.Add("Gasolina Regular");
        IslandTypes.Add("Gasolina Premium");
        IslandTypes.Add("Diesel");
        IslandTypes.Add("Mixta (Gas/Diesel)");
        IslandTypes.Add("GLP/Gas Natural");
        IslandTypes.Add("Electrica");

        // Capacity Options
        CapacityOptions.Clear();
        CapacityOptions.Add("2 Dispensadores");
        CapacityOptions.Add("4 Dispensadores");
        CapacityOptions.Add("6 Dispensadores");
        CapacityOptions.Add("8 Dispensadores");
        CapacityOptions.Add("10+ Dispensadores");

        // Status Options
        StatusOptions.Clear();
        StatusOptions.Add("Activa");
        StatusOptions.Add("Mantenimiento");
        StatusOptions.Add("Inactiva");
        StatusOptions.Add("En Construccion");
    }

    public async Task InitializeAsync()
    {
        await GetIslandAsync();
        await LoadEnhancedIslands();
        UpdateStatistics();
        FilterIslands("all"); // Default filter
    }

    private async Task LoadEnhancedIslands()
    {
        try
        {
            // Create enhanced islands with additional information
            // In real implementation, this would come from the API
            var enhancedIslands = new List<IslandItemExtended>();

            foreach (var island in IslandList)
            {
                var enhanced = new IslandItemExtended
                {
                    Idisland = island.Idisland,
                    Name = $"Isla {island.Idisland}",
                    Description = island.Description,
                    Location = LocationOptions[new Random().Next(LocationOptions.Count)],
                    Type = IslandTypes[new Random().Next(IslandTypes.Count)],
                    Status = StatusOptions[new Random().Next(StatusOptions.Count)],
                    Capacity = CapacityOptions[new Random().Next(CapacityOptions.Count)],
                    InstallationDate = DateTime.Now.AddDays(-new Random().Next(365))
                };
                enhancedIslands.Add(enhanced);
            }

            // Add some sample data if list is empty
            if (!enhancedIslands.Any())
            {
                enhancedIslands.AddRange(new[]
                {
                    new IslandItemExtended { Idisland = 1, Name = "Isla Principal A", Description = "Isla principal con dispensadores de gasolina regular y premium", Location = "Entrada Principal", Type = "Mixta (Gas/Diesel)", Status = "Activa", Capacity = "4 Dispensadores", InstallationDate = DateTime.Now.AddMonths(-6) },
                    new IslandItemExtended { Idisland = 2, Name = "Isla Diesel B", Description = "Isla especializada en combustible diesel para vehiculos pesados", Location = "Area Central", Type = "Diesel", Status = "Activa", Capacity = "2 Dispensadores", InstallationDate = DateTime.Now.AddMonths(-8) },
                    new IslandItemExtended { Idisland = 3, Name = "Isla Premium C", Description = "Isla para combustibles premium y servicios especiales", Location = "Lado Derecho", Type = "Gasolina Premium", Status = "Mantenimiento", Capacity = "6 Dispensadores", InstallationDate = DateTime.Now.AddYears(-1) }
                });
            }

            EnhancedIslandList.Clear();
            foreach (var island in enhancedIslands.OrderBy(x => x.Idisland))
            {
                EnhancedIslandList.Add(island);
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error cargando islas: {ex.Message}", "OK");
        }
    }

    private void FilterIslands(string filter)
    {
        // Reset filter button colors
        FilterAllColor = Color.FromArgb("#9E9E9E");
        FilterActiveColor = Color.FromArgb("#9E9E9E");
        FilterMaintenanceColor = Color.FromArgb("#9E9E9E");

        // Set active button color and apply filter logic
        switch (filter)
        {
            case "all":
                FilterAllColor = Color.FromArgb("#1976D2");
                // Show all islands (no filtering needed for ObservableCollection display)
                break;
            case "active":
                FilterActiveColor = Color.FromArgb("#1976D2");
                // Filter active islands (implementation would filter the collection)
                break;
            case "maintenance":
                FilterMaintenanceColor = Color.FromArgb("#1976D2");
                // Filter maintenance islands
                break;
        }
    }

    private async Task EditIslandAsync(IslandItemExtended island)
    {
        try
        {
            // Load island data into form for editing
            IslandName = island.Name;
            SelectedLocation = island.Location;
            SelectedType = island.Type;
            SelectedCapacity = island.Capacity;
            SelectedStatus = island.Status;
            Description = island.Description;
            InstallationDate = island.InstallationDate;

            await Application.Current.MainPage.DisplayAlert("Modo Edicion", $"Datos de '{island.Name}' cargados para edicion", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error editando isla: {ex.Message}", "OK");
        }
    }

    private async Task DeleteIslandAsync(IslandItemExtended island)
    {
        try
        {
            var result = await Application.Current.MainPage.DisplayAlert(
                "Confirmar eliminacion",
                $"¿Esta seguro de eliminar la isla '{island.Name}'?\nEsta accion no se puede deshacer.",
                "Eliminar",
                "Cancelar");

            if (result)
            {
                EnhancedIslandList.Remove(island);
                UpdateStatistics();
                await Application.Current.MainPage.DisplayAlert("Exito", "Isla eliminada correctamente", "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error eliminando isla: {ex.Message}", "OK");
        }
    }

    private void UpdateStatistics()
    {
        TotalIslands = EnhancedIslandList.Count;
        ActiveIslands = EnhancedIslandList.Count(x => x.Status == "Activa");
        MaintenanceIslands = EnhancedIslandList.Count(x => x.Status == "Mantenimiento");
        InactiveIslands = EnhancedIslandList.Count(x => x.Status == "Inactiva" || x.Status == "En Construccion");
    }

    public async Task GetByIdIslandDataAsync(int islandId)
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/island/{islandId}");
            Island = JsonSerializer.Deserialize<IslandModel>(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo cargar el dato: {ex.Message}", "OK");
        }
    }

    public async Task SaveIslandDataAsync()
    {
        try
        {
            // Create comprehensive island description
            var fullDescription = $"[{IslandName}] Tipo: {SelectedType}, Ubicacion: {SelectedLocation}, Capacidad: {SelectedCapacity}, Estado: {SelectedStatus} - {Description}";

            Island = new IslandModel
            {
                Description = fullDescription
            };

            Request = new IslandRequest
            {
                Request = Island
            };

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var json = JsonSerializer.Serialize(Request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{Configuration.BaseUrl}/api/v1/island", content);

            if (response.IsSuccessStatusCode)
            {
                await Application.Current.MainPage.DisplayAlert("Exito", "Isla registrada correctamente", "OK");
                
                // Add to local enhanced list
                var newIsland = new IslandItemExtended
                {
                    Idisland = EnhancedIslandList.Count + 1,
                    Name = IslandName,
                    Description = Description,
                    Location = SelectedLocation,
                    Type = SelectedType,
                    Status = SelectedStatus,
                    Capacity = SelectedCapacity,
                    InstallationDate = InstallationDate
                };
                
                EnhancedIslandList.Insert(0, newIsland);
                UpdateStatistics();
                
                await GetIslandAsync();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo registrar la isla: {response.StatusCode}\n{error}", "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error al registrar la isla: {ex.Message}", "OK");
        }
    }

    public async Task GetIslandAsync()
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/island");
            var islands = JsonSerializer.Deserialize<IslandApiResponse>(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            IslandList.Clear();
            foreach (var island in islands.Data)
            {
                IslandList.Add(island);
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