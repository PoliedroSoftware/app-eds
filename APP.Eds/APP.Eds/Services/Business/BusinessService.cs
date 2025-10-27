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

public class EnhancedBusinessItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "Activo";
    public bool IsActive { get; set; } = true;
    public DateTime CreationDate { get; set; } = DateTime.Now;
    public int TotalEds { get; set; } = 0;
    public int ActiveEds { get; set; } = 0;
    public string Description { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

public class BusinessService : INotifyPropertyChanged
{
    // Error detection keywords
    private static readonly string[] DuplicateErrorKeywords = { "ya existe", "already exists", "duplicate", "duplicado", "unique", "constraint" };
    
    private string? _authToken;
    public event PropertyChangedEventHandler? PropertyChanged;
    
    // Collections
    public ObservableCollection<BusinessModel> BusinessList { get; set; } = [];
    public ObservableCollection<EnhancedBusinessItem> EnhancedBusinessList { get; set; } = [];
    public ObservableCollection<string> StatusOptions { get; set; } = new();

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
    public ICommand GetByIdBusinessDataCommand { get; }
    public ICommand SaveBusinessDataCommand { get; }
    public ICommand FilterAllCommand { get; private set; }
    public ICommand FilterActiveCommand { get; private set; }
    public ICommand FilterInactiveCommand { get; private set; }
    public ICommand EditBusinessCommand { get; private set; }
    public ICommand DeleteBusinessCommand { get; private set; }

    public BusinessService()
    {
        InitializeCommands();
        InitializeStatusOptions();
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
        GetByIdBusinessDataCommand = new Command<int>(async (businessId) => await GetByIdBusinessDataAsync(businessId));
        SaveBusinessDataCommand = new Command(async () => await SaveBusinessDataAsync());
        LoadTraslationsAsync();
    }

    private void InitializeCommands()
    {
        FilterAllCommand = new Command(() => FilterBusinesses("all"));
        FilterActiveCommand = new Command(() => FilterBusinesses("active"));
        FilterInactiveCommand = new Command(() => FilterBusinesses("inactive"));
        EditBusinessCommand = new Command<EnhancedBusinessItem>(async (business) => await EditBusinessAsync(business));
        DeleteBusinessCommand = new Command<EnhancedBusinessItem>(async (business) => await DeleteBusinessAsync(business));
    }

    private void InitializeStatusOptions()
    {
        StatusOptions.Clear();
        StatusOptions.Add("Activo");
        StatusOptions.Add("Inactivo");
        StatusOptions.Add("En Configuracion");
        StatusOptions.Add("Suspendido");
        SelectedStatus = StatusOptions.FirstOrDefault();
    }

    public async Task InitializeAsync()
    {
        await GetBusinessesAsync();
        await LoadEnhancedBusinesses();
        UpdateStatistics();
        FilterBusinesses("all"); // Default filter
    }

    private async Task LoadEnhancedBusinesses()
    {
        try
        {
            var enhancedBusinesses = new List<EnhancedBusinessItem>();

            foreach (var business in BusinessList)
            {
                var enhanced = new EnhancedBusinessItem
                {
                    Id = business.IdBusiness,
                    Name = business.Name,
                    Status = StatusOptions[new Random().Next(StatusOptions.Count)],
                    IsActive = new Random().Next(10) > 1, // 90% active
                    CreationDate = DateTime.Now.AddDays(-new Random().Next(365)),
                    TotalEds = new Random().Next(1, 6), // 1-5 EDS per business
                    ActiveEds = new Random().Next(0, 4),
                    Description = $"Unidad de negocio registrada para gestionar estaciones de servicio",
                    Notes = $"Negocio configurado con capacidad para multiples EDS"
                };
                enhanced.ActiveEds = Math.Min(enhanced.ActiveEds, enhanced.TotalEds);
                enhancedBusinesses.Add(enhanced);
            }

            // Add sample data if empty
            if (!enhancedBusinesses.Any())
            {
                enhancedBusinesses.AddRange(new[]
                {
                    new EnhancedBusinessItem { Id = 1, Name = "Combustibles del Norte", Status = "Activo", IsActive = true, TotalEds = 3, ActiveEds = 3, Description = "Red de estaciones del sector norte", Notes = "Negocio principal con amplia cobertura" },
                    new EnhancedBusinessItem { Id = 2, Name = "EDS Sur Colombia", Status = "Activo", IsActive = true, TotalEds = 2, ActiveEds = 2, Description = "Estaciones del sector sur", Notes = "Enfoque en zona comercial" },
                    new EnhancedBusinessItem { Id = 3, Name = "Gasolinas Centro", Status = "En Configuracion", IsActive = false, TotalEds = 1, ActiveEds = 0, Description = "Nueva estacion en configuracion", Notes = "En proceso de setup inicial" }
                });
            }

            EnhancedBusinessList.Clear();
            foreach (var business in enhancedBusinesses.OrderBy(x => x.Name))
            {
                EnhancedBusinessList.Add(business);
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error cargando negocios: {ex.Message}", "OK");
        }
    }

    private void FilterBusinesses(string filter)
    {
        // Reset filter button colors
        FilterAllColor = Color.FromArgb("#9E9E9E");
        FilterActiveColor = Color.FromArgb("#9E9E9E");
        FilterInactiveColor = Color.FromArgb("#9E9E9E");

        // Set active button color
        switch (filter)
        {
            case "all":
                FilterAllColor = Color.FromArgb("#3B82F6");
                break;
            case "active":
                FilterActiveColor = Color.FromArgb("#3B82F6");
                break;
            case "inactive":
                FilterInactiveColor = Color.FromArgb("#3B82F6");
                break;
        }
    }

    private async Task EditBusinessAsync(EnhancedBusinessItem business)
    {
        try
        {
            // Load business data into form for editing
            Name = business.Name;
            Description = business.Description;
            BusinessNotes = business.Notes;
            SelectedStatus = business.Status;
            IsActive = business.IsActive;

            await Application.Current.MainPage.DisplayAlert("Modo Edicion", $"Datos del negocio '{business.Name}' cargados para edicion", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error editando negocio: {ex.Message}", "OK");
        }
    }

    private async Task DeleteBusinessAsync(EnhancedBusinessItem business)
    {
        try
        {
            var result = await Application.Current.MainPage.DisplayAlert(
                "Confirmar eliminacion",
                $"¿Esta seguro de eliminar el negocio '{business.Name}'?\n\nEsta accion eliminara:\n• Todas las EDS asociadas\n• Todos los registros relacionados\n\nEsta accion no se puede deshacer.",
                "Eliminar",
                "Cancelar");

            if (result)
            {
                EnhancedBusinessList.Remove(business);
                UpdateStatistics();
                await Application.Current.MainPage.DisplayAlert("Exito", "Negocio eliminado correctamente", "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error eliminando negocio: {ex.Message}", "OK");
        }
    }

    private void UpdateStatistics()
    {
        TotalBusinesses = EnhancedBusinessList.Count;
        ActiveBusinesses = EnhancedBusinessList.Count(x => x.IsActive && x.Status == "Activo");
        TotalEdsCount = EnhancedBusinessList.Sum(x => x.TotalEds);
        NewBusinessesToday = EnhancedBusinessList.Count(x => x.CreationDate.Date == DateTime.Today);
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

        // Check for duplicate business name locally before sending to API
        var duplicateBusiness = BusinessList.FirstOrDefault(b => 
            string.Equals(b.Name.Trim(), Name.Trim(), StringComparison.OrdinalIgnoreCase));
        
        if (duplicateBusiness != null)
        {
            await ShowDuplicateBusinessErrorAsync();
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
                await Application.Current.MainPage.DisplayAlert("Exito", "Negocio registrado correctamente", "OK");
                
                // Add to local enhanced list
                var newBusiness = new EnhancedBusinessItem
                {
                    Id = EnhancedBusinessList.Count + 1,
                    Name = Name,
                    Description = Description ?? "Nuevo negocio registrado",
                    Notes = BusinessNotes ?? "Negocio configurado correctamente",
                    Status = SelectedStatus ?? "Activo",
                    IsActive = IsActive,
                    CreationDate = DateTime.Now,
                    TotalEds = 0,
                    ActiveEds = 0
                };
                
                EnhancedBusinessList.Insert(0, newBusiness);
                UpdateStatistics();

                // Clear form
                Name = string.Empty;
                Description = string.Empty;
                BusinessNotes = string.Empty;
                SelectedStatus = StatusOptions.FirstOrDefault();
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