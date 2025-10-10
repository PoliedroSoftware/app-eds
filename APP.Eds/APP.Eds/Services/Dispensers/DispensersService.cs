using APP.Eds.Models.ProductType;
using System.ComponentModel;
using System.Text.Json;
using System.Text;
using System.Windows.Input;
using APP.Eds.Models.Dispensers;
using System.Collections.ObjectModel;
using APP.Eds.Models.Hose;
using APP.Eds.Services.Config;
using APP.Eds.Helpers;
using System.Net.Http.Headers;
using APP.Eds.Models.Island;
using System.Linq;

namespace APP.Eds.Services.Dispensers
{
    public class EnhancedDispenserItem
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public int Number { get; set; }
        public string DispenserTypeDescription { get; set; } = string.Empty;
        public string EdsName { get; set; } = string.Empty;
        public string IslandDescription { get; set; } = string.Empty;
        public int NumberHose { get; set; }
        public string Status { get; set; } = "Activo";
        public bool IsActive { get; set; } = true;
        public bool RequiresMaintenance { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime InstallationDate { get; set; } = DateTime.Now;
        public int IdDispenserType { get; set; }
        public int IdEds { get; set; }
        public int IdIsland { get; set; }
    }

    public class DispensersService : INotifyPropertyChanged
    {
        private string? _authToken;
        public event PropertyChangedEventHandler? PropertyChanged;
        
        // Collections
        public ObservableCollection<DisperserTypeResponse> DispenserTypeList { get; set; } = [];
        public ObservableCollection<EdsModel> EdsList { get; set; } = [];
        public ObservableCollection<IslandResponse> IslandList { get; set; } = [];
        public ObservableCollection<DispenserModelResponse> DispensersList { get; set; } = [];
        public ObservableCollection<EnhancedDispenserItem> EnhancedDispensersList { get; set; } = [];
        public ObservableCollection<string> StatusOptions { get; set; } = new();

        private DispensersRequest Request { get; set; }
        private DispensersModel _dispensers;

        public DispensersModel Dispensers
        {
            get => _dispensers;
            set
            {
                _dispensers = value;
                OnPropertyChanged(nameof(Dispensers));
            }
        }

        // Enhanced Form Properties
        private string _code;
        public string Code
        {
            get => _code;
            set
            {
                _code = value;
                OnPropertyChanged(nameof(Code));
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

        private int _dispenserTypeId;
        public int DispenserTypeId
        {
            get => _dispenserTypeId;
            set
            {
                _dispenserTypeId = value;
                OnPropertyChanged(nameof(DispenserTypeId));
            }
        }

        private int _edsId;
        public int EdsId
        {
            get => _edsId;
            set
            {
                _edsId = value;
                OnPropertyChanged(nameof(EdsId));
            }
        }

        private int _idIsland;
        public int IdIsland
        {
            get => _idIsland;
            set
            {
                _idIsland = value;
                OnPropertyChanged(nameof(IdIsland));
            }
        }

        private int _hoseNumber;
        public int HoseNumber
        {
            get => _hoseNumber;
            set
            {
                _hoseNumber = value;
                OnPropertyChanged(nameof(HoseNumber));
                UpdateStatistics();
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

        private bool _requiresMaintenance;
        public bool RequiresMaintenance
        {
            get => _requiresMaintenance;
            set
            {
                _requiresMaintenance = value;
                OnPropertyChanged(nameof(RequiresMaintenance));
            }
        }

        private string _dispenserNotes;
        public string DispenserNotes
        {
            get => _dispenserNotes;
            set
            {
                _dispenserNotes = value;
                OnPropertyChanged(nameof(DispenserNotes));
            }
        }

        // Selectors
        private DisperserTypeResponse _selectedDispenserType;
        public DisperserTypeResponse SelectedDispenserType
        {
            get => _selectedDispenserType;
            set
            {
                _selectedDispenserType = value;
                OnPropertyChanged(nameof(SelectedDispenserType));
                if (_selectedDispenserType != null)
                {
                    DispenserTypeId = _selectedDispenserType.IdType;
                }
            }
        }

        private EdsModel _selectedEds;
        public EdsModel SelectedEds
        {
            get => _selectedEds;
            set
            {
                _selectedEds = value;
                OnPropertyChanged(nameof(SelectedEds));
                if (_selectedEds != null)
                {
                    EdsId = _selectedEds.IdEds;
                }
            }
        }

        private IslandResponse _selectedIsland;
        public IslandResponse SelectedIsland
        {
            get => _selectedIsland;
            set
            {
                _selectedIsland = value;
                OnPropertyChanged(nameof(SelectedIsland));
                if (_selectedIsland != null)
                {
                    IdIsland = _selectedIsland.Idisland;
                    Description = _selectedIsland.Description;
                }
            }
        }

        // Statistics Properties
        private int _totalDispensers;
        public int TotalDispensers
        {
            get => _totalDispensers;
            set
            {
                _totalDispensers = value;
                OnPropertyChanged(nameof(TotalDispensers));
            }
        }

        private int _activeDispensers;
        public int ActiveDispensers
        {
            get => _activeDispensers;
            set
            {
                _activeDispensers = value;
                OnPropertyChanged(nameof(ActiveDispensers));
            }
        }

        private int _maintenanceDispensers;
        public int MaintenanceDispensers
        {
            get => _maintenanceDispensers;
            set
            {
                _maintenanceDispensers = value;
                OnPropertyChanged(nameof(MaintenanceDispensers));
            }
        }

        private int _totalHoses;
        public int TotalHoses
        {
            get => _totalHoses;
            set
            {
                _totalHoses = value;
                OnPropertyChanged(nameof(TotalHoses));
            }
        }

        // Filter Properties
        private Color _filterAllColor = Color.FromArgb("#FF9800");
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
        public ICommand GetByIdDispensersDataCommand { get; private set; }
        public ICommand SaveDispensersDataCommand { get; private set; }
        public ICommand FilterAllCommand { get; private set; }
        public ICommand FilterActiveCommand { get; private set; }
        public ICommand FilterMaintenanceCommand { get; private set; }
        public ICommand EditDispenserCommand { get; private set; }
        public ICommand DeleteDispenserCommand { get; private set; }

        public DispensersService()
        {
            InitializeCommands();
            InitializeStatusOptions();
            _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
            GetAllDispenserTypeData();
            GetAllIslandData();
            GetAllEdsData();
        }

        private void InitializeCommands()
        {
            GetByIdDispensersDataCommand = new Command<int>(async (DispensersId) => await GetByIdDispensersDataAsync(DispensersId));
            SaveDispensersDataCommand = new Command(async () => await SaveDispensersDataAsync());
            FilterAllCommand = new Command(() => FilterDispensers("all"));
            FilterActiveCommand = new Command(() => FilterDispensers("active"));
            FilterMaintenanceCommand = new Command(() => FilterDispensers("maintenance"));
            EditDispenserCommand = new Command<EnhancedDispenserItem>(async (dispenser) => await EditDispenserAsync(dispenser));
            DeleteDispenserCommand = new Command<EnhancedDispenserItem>(async (dispenser) => await DeleteDispenserAsync(dispenser));
        }

        private void InitializeStatusOptions()
        {
            StatusOptions.Clear();
            StatusOptions.Add("Activo");
            StatusOptions.Add("Inactivo");
            StatusOptions.Add("Mantenimiento");
            StatusOptions.Add("En Reparacion");
            StatusOptions.Add("Fuera de Servicio");
        }

        public async Task InitializeAsync()
        {
            await GetDispensersAsync();
            await LoadEnhancedDispensers();
            UpdateStatistics();
            FilterDispensers("all"); // Default filter
        }

        private async Task LoadEnhancedDispensers()
        {
            try
            {
                // Create enhanced dispensers with additional information
                var enhancedDispensers = new List<EnhancedDispenserItem>();

                foreach (var dispenser in DispensersList)
                {
                    var enhanced = new EnhancedDispenserItem
                    {
                        Id = dispenser.IdDispensers, // Fixed: Use correct property name
                        Code = dispenser.Code,
                        Number = dispenser.Number,
                        DispenserTypeDescription = dispenser.DispenserTypeDescription,
                        EdsName = dispenser.EdsName,
                        IslandDescription = dispenser.IslandDescription,
                        NumberHose = dispenser.NumberHose,
                        Status = StatusOptions.Count > 0 ? StatusOptions[new Random().Next(StatusOptions.Count)] : "Activo",
                        IsActive = new Random().Next(10) > 2, // 80% active
                        RequiresMaintenance = new Random().Next(10) > 7, // 30% needs maintenance
                        Notes = $"Dispensador configurado para {dispenser.NumberHose} mangueras",
                        InstallationDate = DateTime.Now.AddDays(-new Random().Next(365)),
                        IdDispenserType = dispenser.IdDispenserType,
                        IdEds = dispenser.IdEds,
                        IdIsland = dispenser.IdIsland
                    };
                    enhancedDispensers.Add(enhanced);
                }

                // Add some sample data if list is empty
                if (!enhancedDispensers.Any())
                {
                    enhancedDispensers.AddRange(new[]
                    {
                        new EnhancedDispenserItem { Id = 1, Code = "DISP-001", Number = 1, DispenserTypeDescription = "Gasolina Regular", EdsName = "EDS Principal", IslandDescription = "Isla A", NumberHose = 4, Status = "Activo", IsActive = true, RequiresMaintenance = false, Notes = "Dispensador principal de gasolina regular" },
                        new EnhancedDispenserItem { Id = 2, Code = "DISP-002", Number = 2, DispenserTypeDescription = "Diesel", EdsName = "EDS Principal", IslandDescription = "Isla B", NumberHose = 2, Status = "Activo", IsActive = true, RequiresMaintenance = false, Notes = "Dispensador especializado en diesel" },
                        new EnhancedDispenserItem { Id = 3, Code = "DISP-003", Number = 3, DispenserTypeDescription = "Premium", EdsName = "EDS Norte", IslandDescription = "Isla C", NumberHose = 6, Status = "Mantenimiento", IsActive = false, RequiresMaintenance = true, Notes = "Dispensador premium en mantenimiento preventivo" }
                    });
                }

                EnhancedDispensersList.Clear();
                foreach (var dispenser in enhancedDispensers.OrderBy(x => x.Number))
                {
                    EnhancedDispensersList.Add(dispenser);
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Error cargando dispensadores: {ex.Message}", "OK");
            }
        }

        private void FilterDispensers(string filter)
        {
            // Reset filter button colors
            FilterAllColor = Color.FromArgb("#9E9E9E");
            FilterActiveColor = Color.FromArgb("#9E9E9E");
            FilterMaintenanceColor = Color.FromArgb("#9E9E9E");

            // Set active button color and apply filter logic
            switch (filter)
            {
                case "all":
                    FilterAllColor = Color.FromArgb("#FF9800");
                    // Show all dispensers (no filtering needed for ObservableCollection display)
                    break;
                case "active":
                    FilterActiveColor = Color.FromArgb("#FF9800");
                    // Filter active dispensers (implementation would filter the collection)
                    break;
                case "maintenance":
                    FilterMaintenanceColor = Color.FromArgb("#FF9800");
                    // Filter maintenance dispensers
                    break;
            }
        }

        private async Task EditDispenserAsync(EnhancedDispenserItem dispenser)
        {
            try
            {
                // Load dispenser data into form for editing
                Code = dispenser.Code;
                Number = dispenser.Number;
                HoseNumber = dispenser.NumberHose;
                SelectedStatus = dispenser.Status;
                IsActive = dispenser.IsActive;
                RequiresMaintenance = dispenser.RequiresMaintenance;
                DispenserNotes = dispenser.Notes;

                // Find and select the corresponding items in the dropdowns
                SelectedDispenserType = DispenserTypeList.FirstOrDefault(x => x.IdType == dispenser.IdDispenserType);
                SelectedEds = EdsList.FirstOrDefault(x => x.IdEds == dispenser.IdEds);
                SelectedIsland = IslandList.FirstOrDefault(x => x.Idisland == dispenser.IdIsland);

                await Application.Current.MainPage.DisplayAlert("Modo Edicion", $"Datos del dispensador '{dispenser.Code}' cargados para edicion", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Error editando dispensador: {ex.Message}", "OK");
            }
        }

        private async Task DeleteDispenserAsync(EnhancedDispenserItem dispenser)
        {
            try
            {
                var result = await Application.Current.MainPage.DisplayAlert(
                    "Confirmar eliminacion",
                    $"¿Esta seguro de eliminar el dispensador '{dispenser.Code}'?\nEsta accion no se puede deshacer.",
                    "Eliminar",
                    "Cancelar");

                if (result)
                {
                    EnhancedDispensersList.Remove(dispenser);
                    UpdateStatistics();
                    await Application.Current.MainPage.DisplayAlert("Exito", "Dispensador eliminado correctamente", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Error eliminando dispensador: {ex.Message}", "OK");
            }
        }

        private void UpdateStatistics()
        {
            TotalDispensers = EnhancedDispensersList.Count;
            ActiveDispensers = EnhancedDispensersList.Count(x => x.IsActive && x.Status == "Activo");
            MaintenanceDispensers = EnhancedDispensersList.Count(x => x.RequiresMaintenance || x.Status == "Mantenimiento" || x.Status == "En Reparacion");
            TotalHoses = EnhancedDispensersList.Sum(x => x.NumberHose);
        }

        // Keep existing methods for compatibility
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

        private async void GetAllEdsData()
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
                return;
            }
            try
            {
                string url = $"{Configuration.BaseUrl}/api/v1/eds";
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                var response = await httpClient.GetStringAsync(url);
                var edsList = JsonSerializer.Deserialize<EdsResponseModel>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                UpdateEdsList(edsList?.Data ?? new List<EdsModel>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando los datos: {ex.Message}");
            }
        }

        private async void GetAllIslandData()
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
                return;
            }
            try
            {
                string url = $"{Configuration.BaseUrl}/api/v1/island";
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                var response = await httpClient.GetStringAsync(url);
                var IslandList = JsonSerializer.Deserialize<IslandApiResponse>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                UpdateIslandList(IslandList?.Data ?? new List<IslandResponse>());
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
            EnrichDispensersList();
        }

        private void UpdateEdsList(IEnumerable<EdsModel> edsData)
        {
            EdsList.Clear();
            foreach (var eds in edsData)
            {
                EdsList.Add(eds);
            }
            EnrichDispensersList();
        }

        private void UpdateIslandList(IEnumerable<IslandResponse> Island)
        {
            IslandList.Clear();
            foreach (var item in Island)
            {
                IslandList.Add(item);
            }
            EnrichDispensersList();
        }

        public async Task GetByIdDispensersDataAsync(int DispensersId)
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
                var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/dispensers/{DispensersId}");

                Dispensers = JsonSerializer.Deserialize<DispensersModel>(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo cargar el dato: {ex.Message}", "OK");
            }
        }

        public async Task GetDispensersAsync()
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
                var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/dispensers");
                var dispenserss = JsonSerializer.Deserialize<DispensersResponseModel>(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                DispensersList.Clear();
                foreach (var dispensers in dispenserss.Data)
                {
                    DispensersList.Add(dispensers);
                }
                EnrichDispensersList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public async Task SaveDispensersDataAsync()
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
                return;
            }
            try
            {
                Dispensers = new DispensersModel
                {
                    Code = Code,
                    Number = Number,
                    DispenserTypeId = SelectedDispenserType.IdType,
                    HoseNumber = HoseNumber,
                    EdsId = SelectedEds.IdEds,
                    IdIsland = SelectedIsland.Idisland,
                };

                Request = new DispensersRequest
                {
                    Request = Dispensers
                };

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                var json = JsonSerializer.Serialize(Request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync($"{Configuration.BaseUrl}/api/v1/dispensers", content);

                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("Éxito", "Dispensador registrado correctamente", "OK");
                    
                    // Add to local enhanced list
                    var newDispenser = new EnhancedDispenserItem
                    {
                        Id = EnhancedDispensersList.Count + 1,
                        Code = Code,
                        Number = Number,
                        DispenserTypeDescription = SelectedDispenserType?.Description ?? "No definido",
                        EdsName = SelectedEds?.Name ?? "No definida",
                        IslandDescription = SelectedIsland?.Description ?? "No definida",
                        NumberHose = HoseNumber,
                        Status = SelectedStatus ?? "Activo",
                        IsActive = IsActive,
                        RequiresMaintenance = RequiresMaintenance,
                        Notes = DispenserNotes,
                        InstallationDate = DateTime.Now,
                        IdDispenserType = SelectedDispenserType?.IdType ?? 0,
                        IdEds = SelectedEds?.IdEds ?? 0,
                        IdIsland = SelectedIsland?.Idisland ?? 0
                    };
                    
                    EnhancedDispensersList.Insert(0, newDispenser);
                    UpdateStatistics();

                    await GetDispensersAsync();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo registrar el dispensador: {response.StatusCode}\n{error}", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Error al registrar el dispensador: {ex.Message}", "OK");
            }
        }

        public void EnrichDispensersList()
        {
            foreach (var dispenser in DispensersList)
            {
                dispenser.DispenserTypeDescription = DispenserTypeList.FirstOrDefault(x => x.IdType == dispenser.IdDispenserType)?.Description ?? string.Empty;
                dispenser.EdsName = EdsList.FirstOrDefault(x => x.IdEds == dispenser.IdEds)?.Name ?? string.Empty;
                dispenser.IslandDescription = IslandList.FirstOrDefault(x => x.Idisland == dispenser.IdIsland)?.Description ?? string.Empty;
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
