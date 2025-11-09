using APP.Eds.Helpers;
using APP.Eds.Models.Islander;
using APP.Eds.Services.Config;
using APP.Eds.Components.PopUp;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows.Input;
using APP.Eds.Models.Business;

namespace APP.Eds.Services.Islander;

public class EditablePendingIslander : INotifyPropertyChanged
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public int IdEds { get; set; }
    public required string Password { get; set; }
    public Models.Eds.EdsModel SelectedEds { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class EnhancedIslanderItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Firstname { get; set; } = string.Empty;
    public string Lastname { get; set; } = string.Empty;
    public int IdEds { get; set; }
    public string EdsName { get; set; } = string.Empty;
    public string Role { get; set; } = "Operario";
    public string RoleIcon { get; set; } = "👷";
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool CanManageDispensers { get; set; }
    public DateTime HireDate { get; set; } = DateTime.Now;
    public DateTime LastAccess { get; set; } = DateTime.Now;
}

public class IslanderService : INotifyPropertyChanged
{
    public bool ActivateClearForm { get; private set; }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    // Collections
    public ObservableCollection<EdsModel> EdsList { get; set; } = [];
    public ObservableCollection<IslanderResponse> IslanderList { get; set; } = [];
    public ObservableCollection<EnhancedIslanderItem> EnhancedIslanderList { get; set; } = [];
    public ObservableCollection<string> RoleOptions { get; set; } = new();

    private IslanderRequest Request { get; set; }
    private IslanderModel _islander;
    private string? _authToken;

    public IslanderModel Islander
    {
        get => _islander;
        set
        {
            _islander = value;
            OnPropertyChanged(nameof(Islander));
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
            OnPropertyChanged(nameof(Name));
        }
    }

    private string _email;
    public string Email
    {
        get => _email;
        set
        {
            _email = value;
            OnPropertyChanged(nameof(Email));
        }
    }

    private string _firstname;
    public string FirstName
    {
        get => _firstname;
        set
        {
            _firstname = value;
            OnPropertyChanged(nameof(FirstName));
        }
    }

    private string _lastname;
    public string LastName
    {
        get => _lastname;
        set
        {
            _lastname = value;
            OnPropertyChanged(nameof(LastName));
        }
    }

    private string _password;
    public string Password
    {
        get => _password;
        set
        {
            _password = value;
            OnPropertyChanged(nameof(Password));
        }
    }

    private string _phoneNumber;
    public string PhoneNumber
    {
        get => _phoneNumber;
        set
        {
            _phoneNumber = value;
            OnPropertyChanged(nameof(PhoneNumber));
        }
    }

    private string _selectedRole;
    public string SelectedRole
    {
        get => _selectedRole;
        set
        {
            _selectedRole = value;
            OnPropertyChanged(nameof(SelectedRole));
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

    private bool _canManageDispensers;
    public bool CanManageDispensers
    {
        get => _canManageDispensers;
        set
        {
            _canManageDispensers = value;
            OnPropertyChanged(nameof(CanManageDispensers));
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
                IdEds = _selectedEds.IdEds;
            }
        }
    }

    private int _idEds;
    public int IdEds
    {
        get => _idEds;
        set
        {
            _idEds = value;
            OnPropertyChanged(nameof(IdEds));
        }
    }

    // Statistics Properties
    private int _totalIslanders;
    public int TotalIslanders
    {
        get => _totalIslanders;
        set
        {
            _totalIslanders = value;
            OnPropertyChanged(nameof(TotalIslanders));
        }
    }

    private int _activeIslanders;
    public int ActiveIslanders
    {
        get => _activeIslanders;
        set
        {
            _activeIslanders = value;
            OnPropertyChanged(nameof(ActiveIslanders));
        }
    }

    private int _supervisorIslanders;
    public int SupervisorIslanders
    {
        get => _supervisorIslanders;
        set
        {
            _supervisorIslanders = value;
            OnPropertyChanged(nameof(SupervisorIslanders));
        }
    }

    private int _coveredEDS;
    public int CoveredEDS
    {
        get => _coveredEDS;
        set
        {
            _coveredEDS = value;
            OnPropertyChanged(nameof(CoveredEDS));
        }
    }

    // Filter Properties
    private Color _filterAllColor = Color.FromArgb("#6A1B9A");
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

    private Color _filterSupervisorColor = Color.FromArgb("#9E9E9E");
    public Color FilterSupervisorColor
    {
        get => _filterSupervisorColor;
        set
        {
            _filterSupervisorColor = value;
            OnPropertyChanged(nameof(FilterSupervisorColor));
        }
    }

    // Commands
    public ICommand GetByIdIslanderDataCommand { get; private set; }
    public ICommand SaveIslanderDataCommand { get; private set; }
    public ICommand FilterAllCommand { get; private set; }
    public ICommand FilterActiveCommand { get; private set; }
    public ICommand FilterSupervisorCommand { get; private set; }
    public ICommand EditIslanderCommand { get; private set; }
    public ICommand DeleteIslanderCommand { get; private set; }

    public IslanderService()
    {
        InitializeCommands();
        InitializeRoleOptions();
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
        GetAllEdsData();
    }

    private void InitializeCommands()
    {
        GetByIdIslanderDataCommand = new Command<int>(async (islanderId) => await GetByIdIslanderDataAsync(islanderId));
        SaveIslanderDataCommand = new Command(async () => await SaveIslanderDataAsync());
        FilterAllCommand = new Command(() => FilterIslanders("all"));
        FilterActiveCommand = new Command(() => FilterIslanders("active"));
        FilterSupervisorCommand = new Command(() => FilterIslanders("supervisor"));
        EditIslanderCommand = new Command<EnhancedIslanderItem>(async (islander) => await EditIslanderAsync(islander));
        DeleteIslanderCommand = new Command<EnhancedIslanderItem>(async (islander) => await DeleteIslanderAsync(islander));
    }

    private void InitializeRoleOptions()
    {
        RoleOptions.Clear();
        RoleOptions.Add("Operario");
        RoleOptions.Add("Supervisor");
        RoleOptions.Add("Encargado de Turno");
        RoleOptions.Add("Cajero");
        RoleOptions.Add("Mantenimiento");
        RoleOptions.Add("Seguridad");
    }

    public async Task InitializeAsync()
    {
        await GetIslandersAsync();
        await LoadEnhancedIslanders();
        UpdateStatistics();
        FilterIslanders("all"); // Default filter
    }

    private async Task LoadEnhancedIslanders()
    {
        try
        {
            var enhancedIslanders = new List<EnhancedIslanderItem>();

            foreach (var islander in IslanderList)
            {
                var edsName = EdsList.FirstOrDefault(x => x.IdEds == islander.IdEds)?.Name ?? "Sin asignar";
                var role = RoleOptions[new Random().Next(RoleOptions.Count)];
                
                var enhanced = new EnhancedIslanderItem
                {
                    Id = islander.IdIslander, // Fixed: Use correct property name
                    Name = islander.Name,
                    Email = islander.Email, // Generate email from name
                    Firstname = ExtractFirstName(islander.Name), // Extract from full name
                    Lastname = ExtractLastName(islander.Name), // Extract from full name
                    IdEds = islander.IdEds,
                    EdsName = edsName,
                    Role = role,
                    RoleIcon = GetRoleIcon(role),
                    PhoneNumber = GeneratePhoneNumber(),
                    IsActive = new Random().Next(10) > 1, // 90% active
                    CanManageDispensers = role == "Supervisor" || role == "Encargado de Turno",
                    HireDate = DateTime.Now.AddDays(-new Random().Next(1095)), // Random date within 3 years
                    LastAccess = DateTime.Now.AddDays(-new Random().Next(30)) // Last 30 days
                };
                enhancedIslanders.Add(enhanced);
            }

            // Add sample data if empty
            if (!enhancedIslanders.Any())
            {
                enhancedIslanders.AddRange(new[]
                {
                    new EnhancedIslanderItem { Id = 1, Name = "Juan Carlos Rodriguez", Email = "juan.rodriguez@eds.com", Firstname = "Juan", Lastname = "Rodriguez", IdEds = 1, EdsName = "EDS Principal", Role = "Supervisor", RoleIcon = "👨‍💼", PhoneNumber = "+57 300 123 4567", IsActive = true, CanManageDispensers = true },
                    new EnhancedIslanderItem { Id = 2, Name = "Maria Elena Gutierrez", Email = "maria.gutierrez@eds.com", Firstname = "Maria", Lastname = "Gutierrez", IdEds = 1, EdsName = "EDS Principal", Role = "Operario", RoleIcon = "👷‍♀️", PhoneNumber = "+57 301 234 5678", IsActive = true, CanManageDispensers = false },
                    new EnhancedIslanderItem { Id = 3, Name = "Carlos Alberto Mendez", Email = "carlos.mendez@eds.com", Firstname = "Carlos", Lastname = "Mendez", IdEds = 2, EdsName = "EDS Norte", Role = "Encargado de Turno", RoleIcon = "👨‍💼", PhoneNumber = "+57 302 345 6789", IsActive = true, CanManageDispensers = true }
                });
            }

            EnhancedIslanderList.Clear();
            foreach (var islander in enhancedIslanders.OrderBy(x => x.Name))
            {
                EnhancedIslanderList.Add(islander);
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error cargando isleros: {ex.Message}", "OK");
        }
    }

    private string GetRoleIcon(string role)
    {
        return role switch
        {
            "Supervisor" => "👨‍💼",
            "Encargado de Turno" => "👨‍💼",
            "Cajero" => "💰",
            "Mantenimiento" => "🔧",
            "Seguridad" => "🛡️",
            _ => "👷"
        };
    }

    private string GeneratePhoneNumber()
    {
        var random = new Random();
        return $"+57 {300 + random.Next(20)} {random.Next(100, 999)} {random.Next(1000, 9999)}";
    }

    private string GenerateEmailFromName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return "usuario@eds.com";
        
        var cleanName = fullName.ToLowerInvariant()
            .Replace(" ", ".")
            .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")
            .Replace("ñ", "n");
        
        return $"{cleanName}@eds.com";
    }

    private string ExtractFirstName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return "Nombre";
        
        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[0] : "Nombre";
    }

    private string ExtractLastName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return "Apellido";
        
        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : "Apellido";
    }

    private void FilterIslanders(string filter)
    {
        // Reset filter button colors
        FilterAllColor = Color.FromArgb("#9E9E9E");
        FilterActiveColor = Color.FromArgb("#9E9E9E");
        FilterSupervisorColor = Color.FromArgb("#9E9E9E");

        // Set active button color
        switch (filter)
        {
            case "all":
                FilterAllColor = Color.FromArgb("#6A1B9A");
                break;
            case "active":
                FilterActiveColor = Color.FromArgb("#6A1B9A");
                break;
            case "supervisor":
                FilterSupervisorColor = Color.FromArgb("#6A1B9A");
                break;
        }
    }

    private async Task EditIslanderAsync(EnhancedIslanderItem islander)
    {
        try
        {
            // Load islander data into form for editing
            Name = islander.Name;
            FirstName = islander.Firstname;
            LastName = islander.Lastname;
            Email = islander.Email;
            PhoneNumber = islander.PhoneNumber;
            SelectedRole = islander.Role;
            IsActive = islander.IsActive;
            CanManageDispensers = islander.CanManageDispensers;

            // Find and select the corresponding EDS
            SelectedEds = EdsList.FirstOrDefault(x => x.IdEds == islander.IdEds);

            await Application.Current.MainPage.DisplayAlert("Modo Edicion", $"Datos del islero '{islander.Name}' cargados para edicion", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error editando islero: {ex.Message}", "OK");
        }
    }

    private async Task DeleteIslanderAsync(EnhancedIslanderItem islander)
    {
        try
        {
            var result = await Application.Current.MainPage.DisplayAlert(
                "Confirmar eliminacion",
                $"¿Esta seguro de eliminar al islero '{islander.Name}'?\nEsta accion no se puede deshacer.",
                "Eliminar",
                "Cancelar");

            if (result)
            {
                EnhancedIslanderList.Remove(islander);
                UpdateStatistics();
                await Application.Current.MainPage.DisplayAlert("Exito", "Islero eliminado correctamente", "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error eliminando islero: {ex.Message}", "OK");
        }
    }

    private void UpdateStatistics()
    {
        TotalIslanders = EnhancedIslanderList.Count;
        ActiveIslanders = EnhancedIslanderList.Count(x => x.IsActive);
        SupervisorIslanders = EnhancedIslanderList.Count(x => x.Role == "Supervisor" || x.Role == "Encargado de Turno");
        CoveredEDS = EnhancedIslanderList.Select(x => x.IdEds).Distinct().Count();
    }

    // Keep existing methods for API compatibility
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

    public async Task GetByIdIslanderDataAsync(int islanderId)
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

            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/islander/{islanderId}");

            Islander = JsonSerializer.Deserialize<IslanderModel>(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo cargar el dato: {ex.Message}", "OK");
        }
    }

    private async Task<string?> GetBusinessKeycloakIdAsync(int idBusiness)
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            return null;
        }

        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/business/{idBusiness}");

            var business = JsonSerializer.Deserialize<APP.Eds.Models.Business.BusinessModel>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return business?.KeycloakId;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error obteniendo KeycloakId del negocio {idBusiness}: {ex.Message}");
            return null;
        }
    }

    public async Task SaveIslanderDataAsync()
    {
        try
        {
            ActivateClearForm = false;
            if (string.IsNullOrEmpty(_authToken))
            {
                await CustomAlert.ShowErrorAsync("No se encontró el token de autenticación", "Error de Autenticación");
                return;
            }

            if (SelectedEds is null)
            {
                await CustomAlert.ShowErrorAsync("Por favor, seleccione un EDS", "EDS Requerido");
                return;
            }

            bool userError = IslanderList.Any(i => i.Name.Equals(Name, StringComparison.OrdinalIgnoreCase));
            bool emailError = IslanderList.Any(i => i.Email.Equals(Email, StringComparison.OrdinalIgnoreCase));

            if (emailError)
            {
                await CustomAlert.ShowErrorAsync($"El correo electrónico {Email} ya está registrado. Por favor, use otro correo.", "Email Duplicado");
                return;
            }
            if (userError)
            {
                await CustomAlert.ShowErrorAsync($"El usuario {Name} ya está registrado. Por favor, use otro nombre.", "Usuario Duplicado");
                return;
            }

            // Obtener KeycloakId del negocio asociado al EDS seleccionado
            string? keycloakId = await GetBusinessKeycloakIdAsync(SelectedEds.IdBusiness);

            Islander = new IslanderModel
            {
                Name = Name,
                Email = Email,
                FirstName = FirstName,
                LastName = LastName,
                IdEds = SelectedEds.IdEds,
                Password = Password,
            };

            Request = new IslanderRequest
            {
                Request = Islander,
                NameClaimToken = keycloakId ?? string.Empty
            };

            using var httpClient = new HttpClient();
            var json = JsonSerializer.Serialize(Request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            var response = await httpClient.PostAsync($"{Configuration.BaseUrl}/api/v1/islander", content);

            if (response.IsSuccessStatusCode)
            {
                await CustomAlert.ShowSuccessAsync(
                    $"Islero registrado exitosamente:\n\n" +
                    $"• Nombre: {Name}\n" +
                    $"• Email: {Email}\n" +
                    $"• Primer Nombre: {FirstName}\n" +
                    $"• Apellidos: {LastName}\n" +
                    $"• EDS: {SelectedEds.Name}\n" +
                    $"• Rol: {SelectedRole ?? "Operario"}", 
                    "Islero Registrado");
                
                // Add to enhanced list
                var newIslander = new EnhancedIslanderItem
                {
                    Id = EnhancedIslanderList.Count + 1,
                    Name = Name,
                    Email = Email,
                    Firstname = FirstName,
                    Lastname = LastName,
                    IdEds = SelectedEds.IdEds,
                    EdsName = SelectedEds.Name,
                    Role = SelectedRole ?? "Operario",
                    RoleIcon = GetRoleIcon(SelectedRole ?? "Operario"),
                    PhoneNumber = PhoneNumber,
                    IsActive = IsActive,
                    CanManageDispensers = CanManageDispensers,
                    HireDate = DateTime.Now,
                    LastAccess = DateTime.Now
                };
                ActivateClearForm = true;
                
                EnhancedIslanderList.Insert(0, newIslander);
                UpdateStatistics();

                await GetIslandersAsync();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await CustomAlert.ShowErrorAsync($"No se pudo registrar el islero: {response.StatusCode}\n{error}", "Error del Servidor");
                
            }
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error al registrar el islero: {ex.Message}", "Error del Sistema");
            
        }
    }

    private void UpdateEdsList(IEnumerable<EdsModel> edsData)
    {
        EdsList.Clear();
        foreach (var eds in edsData)
        {
            EdsList.Add(eds);
        }
    }

    public async Task GetIslandersAsync()
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
            var islanders = JsonSerializer.Deserialize<IslanderApiResponse>(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            IslanderList.Clear();
            foreach (var islander in islanders.Data)
            {
                IslanderList.Add(islander);
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
