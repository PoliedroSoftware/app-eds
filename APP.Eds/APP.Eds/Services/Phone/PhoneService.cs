using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Input;
using APP.Eds.Components.PopUp;
using APP.Eds.Helpers;
using APP.Eds.Models.Phone;
using APP.Eds.Services.Config;

namespace APP.Eds.Services.Phone;

public class PhoneService : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    
    private readonly HttpClient _httpClient;
    private readonly string? _authToken;

    public ObservableCollection<PhoneModel> PhonesList { get; set; } = new();
    public ObservableCollection<PhoneModel> DatabasePhonesList { get; set; } = new();

    // Propiedades del formulario
    private string _phoneNumber = string.Empty;
    public string PhoneNumber
    {
        get => _phoneNumber;
        set
        {
            _phoneNumber = value;
            OnPropertyChanged(nameof(PhoneNumber));
            OnPropertyChanged(nameof(FullPhoneNumber));
        }
    }

    private string _ownerName = string.Empty;
    public string OwnerName
    {
        get => _ownerName;
        set
        {
            _ownerName = value;
            OnPropertyChanged(nameof(OwnerName));
        }
    }

    public string FullPhoneNumber => string.IsNullOrEmpty(PhoneNumber) ? "+57" : $"+57{PhoneNumber}";

    // Estadísticas
    public int TotalPhones => PhonesList.Count;
    public int PhonesToday => PhonesList.Count(p => p.CreatedAt.Date == DateTime.Today);
    public int DatabasePhonesCount => DatabasePhonesList.Count;

    // Estado de carga
    private bool _isLoadingDatabasePhones;
    public bool IsLoadingDatabasePhones
    {
        get => _isLoadingDatabasePhones;
        set
        {
            _isLoadingDatabasePhones = value;
            OnPropertyChanged(nameof(IsLoadingDatabasePhones));
        }
    }

    // Comandos
    public ICommand SavePhoneCommand { get; private set; }
    public ICommand DeletePhoneCommand { get; private set; }
    public ICommand ClearFormCommand { get; private set; }
    public ICommand SendPhonesCommand { get; private set; }
    public ICommand LoadDatabasePhonesCommand { get; private set; }
    public ICommand RefreshDatabasePhonesCommand { get; private set; }

    public PhoneService()
    {
        _httpClient = new HttpClient();
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
        InitializeCommands();
        LoadDatabasePhonesAsync(); // Cargar automáticamente al iniciar
    }

    private void InitializeCommands()
    {
        SavePhoneCommand = new Command(async () => await SavePhoneAsync());
        DeletePhoneCommand = new Command<PhoneModel>(async (phone) => await DeletePhoneAsync(phone));
        ClearFormCommand = new Command(ClearForm);
        SendPhonesCommand = new Command(async () => await SendPhonesToServerAsync());
        LoadDatabasePhonesCommand = new Command(async () => await LoadDatabasePhonesAsync());
        RefreshDatabasePhonesCommand = new Command(async () => await RefreshDatabasePhonesAsync());
    }

    // Validación unificada
    private (bool IsValid, string ErrorMessage) ValidateForm()
    {
        // Validar número de teléfono
        if (string.IsNullOrWhiteSpace(PhoneNumber))
            return (false, "El número de teléfono es requerido");

        if (!Regex.IsMatch(PhoneNumber, @"^\d+$"))
            return (false, "El número solo debe contener dígitos");

        if (PhoneNumber.Length < 7 || PhoneNumber.Length > 12)
            return (false, "El número debe tener entre 7 y 12 dígitos");

        if (PhoneNumber.StartsWith("0"))
            return (false, "El número no debe empezar con 0");

        // Validar nombre del propietario
        if (string.IsNullOrWhiteSpace(OwnerName))
            return (false, "El nombre del propietario es requerido");

        if (OwnerName.Trim().Length < 2)
            return (false, "El nombre debe tener al menos 2 caracteres");

        if (!Regex.IsMatch(OwnerName, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s\-\.]+$"))
            return (false, "El nombre solo puede contener letras, espacios y guiones");

        return (true, string.Empty);
    }

    private async Task<bool> HandleDuplicatePhoneAsync(string phoneNumber, string ownerName)
    {
        var existingPhone = PhonesList.FirstOrDefault(p => p.Number == phoneNumber);
        if (existingPhone == null) return true;

        // Mismo propietario
        if (string.Equals(existingPhone.Name.Trim(), ownerName.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            await CustomAlert.ShowInfoAsync(
                $"NÚMERO YA REGISTRADO\n\n" +
                $"El número {FullPhoneNumber} ya está registrado con el mismo propietario.\n\n" +
                $"Número: {existingPhone.FullNumber}\n" +
                $"Propietario: {existingPhone.Name}\n" +
                $"Registrado: {existingPhone.CreatedAt:dd/MM/yyyy HH:mm}",
                "Información");
            return false;
        }

        // Diferente propietario - preguntar si reemplazar
        var shouldReplace = await CustomAlert.ShowConfirmAsync(
            $"NÚMERO DUPLICADO CON DIFERENTE PROPIETARIO\n\n" +
            $"Número actual: {existingPhone.Name}\n" +
            $"Nuevo propietario: {ownerName}\n\n" +
            $"¿Deseas reemplazar el registro existente?",
            "Reemplazar Registro",
            "Reemplazar",
            "Cancelar");

        if (shouldReplace)
        {
            PhonesList.Remove(existingPhone);
            OnPropertyChanged(nameof(TotalPhones));
            OnPropertyChanged(nameof(PhonesToday));
        }

        return shouldReplace;
    }

    public async Task SavePhoneAsync()
    {
        try
        {
            var (isValid, errorMessage) = ValidateForm();
            if (!isValid)
            {
                await CustomAlert.ShowErrorAsync(errorMessage, "Error de Validación");
                return;
            }

            var canProceed = await HandleDuplicatePhoneAsync(PhoneNumber, OwnerName.Trim());
            if (!canProceed) return;

            var newPhone = new PhoneModel
            {
                IdPhone = PhonesList.Count + 1,
                Number = PhoneNumber,
                Name = OwnerName.Trim(),
                CountryCode = "57",
                CreatedAt = DateTime.Now
            };

            PhonesList.Insert(0, newPhone);
            OnPropertyChanged(nameof(TotalPhones));
            OnPropertyChanged(nameof(PhonesToday));

            await CustomAlert.ShowSuccessAsync(
                $"TELÉFONO AGREGADO EXITOSAMENTE\n\n" +
                $"Número: {newPhone.FullNumber}\n" +
                $"Propietario: {newPhone.Name}\n" +
                $"Total en lista: {TotalPhones} teléfonos",
                "Agregado a la Lista");

            ClearForm();
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error al agregar el teléfono:\n\n{ex.Message}", "Error del Sistema");
        }
    }

    public async Task DeletePhoneAsync(PhoneModel phone)
    {
        try
        {
            var result = await CustomAlert.ShowConfirmAsync(
                $"¿Está seguro de eliminar el número {phone.FullNumber} de {phone.Name}?",
                "Confirmar Eliminación",
                "Eliminar",
                "Cancelar");

            if (result)
            {
                PhonesList.Remove(phone);
                OnPropertyChanged(nameof(TotalPhones));
                OnPropertyChanged(nameof(PhonesToday));
                
                await CustomAlert.ShowSuccessAsync(
                    $"Número {phone.FullNumber} de {phone.Name} eliminado de la lista",
                    "Eliminado");
            }
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error al eliminar el teléfono:\n\n{ex.Message}", "Error del Sistema");
        }
    }

    public async Task SendPhonesToServerAsync()
    {
        try
        {
            if (!PhonesList.Any())
            {
                await CustomAlert.ShowWarningAsync("No hay números de teléfono en la lista para enviar", "Lista Vacía");
                return;
            }

            if (string.IsNullOrEmpty(_authToken))
            {
                await CustomAlert.ShowErrorAsync(
                    "No se encontró el token de autenticación.\n\nPor favor, inicia sesión nuevamente.",
                    "Error de Autenticación");
                return;
            }

            var phones = PhonesList.Select(p => new PhoneItemModel
            {
                Number = p.FullNumber,
                Name = p.Name
            }).ToList();

            var request = new PhoneRequest
            {
                Request = new PhoneRequestData { Phones = phones }
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            
            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"{Configuration.BaseUrl}/api/v1/phone", content);

            if (response.IsSuccessStatusCode)
            {
                await CustomAlert.ShowSuccessAsync(
                    $"TELÉFONOS ENVIADOS EXITOSAMENTE\n\n" +
                    $"Total enviados: {phones.Count}\n" +
                    $"Los números se han guardado en la base de datos.",
                    "Envío Exitoso");

                PhonesList.Clear();
                OnPropertyChanged(nameof(TotalPhones));
                OnPropertyChanged(nameof(PhonesToday));
                
                // Actualizar la lista de la base de datos después del envío exitoso
                await LoadDatabasePhonesAsync();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                
                // Mejorar el manejo específico de errores de validación
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    await HandleValidationErrorAsync(error);
                }
                else
                {
                    var errorMessage = response.StatusCode switch
                    {
                        System.Net.HttpStatusCode.NotFound => "El servicio de teléfonos no está disponible en el servidor.",
                        System.Net.HttpStatusCode.Unauthorized => "Tu sesión ha expirado. Por favor, inicia sesión nuevamente.",
                        System.Net.HttpStatusCode.InternalServerError => "Error interno del servidor. Intenta nuevamente en unos minutos.",
                        _ => $"Error del servidor ({response.StatusCode}). Contacta al administrador si el problema persiste."
                    };

                    await CustomAlert.ShowErrorAsync(errorMessage, "Error al Enviar");
                }
            }
        }
        catch (HttpRequestException httpEx)
        {
            await CustomAlert.ShowErrorAsync(
                $"No se pudo conectar con el servidor.\n\n" +
                $"Verifica tu conexión a internet y que el servidor esté disponible.\n\n" +
                $"Detalles técnicos: {httpEx.Message}",
                "Error de Conexión");
        }
        catch (TaskCanceledException)
        {
            await CustomAlert.ShowErrorAsync(
                "La operación tardó demasiado tiempo en responder.\n\n" +
                "El servidor puede estar sobrecargado. Intenta nuevamente en unos minutos.",
                "Tiempo Agotado");
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync(
                $"Error inesperado del sistema.\n\n" +
                $"Si el problema persiste, contacta al administrador.\n\n" +
                $"Detalles técnicos: {ex.Message}",
                "Error del Sistema");
        }
    }

    private async Task HandleValidationErrorAsync(string errorResponse)
    {
        try
        {
            // Verificar si es un error de número duplicado
            if (errorResponse.Contains("ya existe en la base de datos") || 
                errorResponse.Contains("ValidationFailed") ||
                errorResponse.Contains("already exists"))
            {
                // Extraer el número del mensaje de error si es posible
                var phoneMatch = Regex.Match(errorResponse, @"\+57\d+");
                var duplicateNumber = phoneMatch.Success ? phoneMatch.Value : "número indicado";

                await CustomAlert.ShowWarningAsync(
                    $"NÚMERO DUPLICADO DETECTADO\n\n" +
                    $"El número {duplicateNumber} ya está registrado en la base de datos.\n\n" +
                    $"OPCIONES DISPONIBLES:\n" +
                    $"• Elimina el número duplicado de tu lista\n" +
                    $"• Verifica que el número sea correcto\n" +
                    $"NOTA: Los demás números (si los hay) no se han enviado para evitar errores adicionales.",
                    "Número Ya Registrado");
            }
            else if (errorResponse.Contains("formato") || errorResponse.Contains("format"))
            {
                await CustomAlert.ShowErrorAsync(
                    $"ERROR EN EL FORMATO DE DATOS\n\n" +
                    $"El servidor no pudo procesar los números enviados debido a un problema en el formato.\n\n" +
                    $"SOLUCIONES:\n" +
                    $"• Verifica que todos los números tengan entre 7 y 12 dígitos\n" +
                    $"• Asegúrate de que solo contengan números\n" +
                    $"• Los nombres no deben tener caracteres especiales\n\n" +
                    $"Intenta agregar los números nuevamente uno por uno.",
                    "Error de Formato");
            }
            else
            {
                // Error de validación genérico
                await CustomAlert.ShowErrorAsync(
                    $"ERROR DE VALIDACIÓN\n\n" +
                    $"El servidor rechazó algunos de los datos enviados.\n\n" +
                    $"Detalles del servidor:\n{errorResponse}\n\n" +
                    $"REVISA QUE:\n" +
                    $"• Los números no estén duplicados\n" +
                    $"• Los nombres sean válidos\n" +
                    $"• Los números tengan el formato correcto",
                    "Error de Validación");
            }
        }
        catch
        {
            // Si falla el parsing del error, mostrar mensaje genérico mejorado
            await CustomAlert.ShowErrorAsync(
                $"ERROR AL PROCESAR LOS DATOS\n\n" +
                $"El servidor encontró un problema con los números enviados.\n\n" +
                $"Esto puede deberse a:\n" +
                $"• Números duplicados en la base de datos\n" +
                $"• Formato incorrecto de algún número\n" +
                $"• Nombres con caracteres no válidos\n\n" +
                $"SOLUCIÓN: Intenta enviar los números uno por uno para identificar el problema.",
                "Error de Validación");
        }
    }

    private async Task LoadDatabasePhonesAsync()
    {
        try
        {
            IsLoadingDatabasePhones = true;

            if (string.IsNullOrEmpty(_authToken))
            {
                await CustomAlert.ShowErrorAsync(
                    "No se encontró el token de autenticación.\n\nPor favor, inicia sesión nuevamente.",
                    "Error de Autenticación");
                return;
            }

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            var response = await _httpClient.GetAsync($"{Configuration.BaseUrl}/api/v1/phone");

            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                
                // Intentar deserializar como array directo primero
                try
                {
                    var phoneArray = JsonSerializer.Deserialize<PhoneModel[]>(jsonContent, new JsonSerializerOptions 
                    { 
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        PropertyNameCaseInsensitive = true 
                    });
                    
                    if (phoneArray != null)
                    {
                        DatabasePhonesList.Clear();
                        foreach (var phone in phoneArray.OrderByDescending(p => p.CreatedAt))
                        {
                            DatabasePhonesList.Add(phone);
                        }
                        OnPropertyChanged(nameof(DatabasePhonesCount));
                        return;
                    }
                }
                catch
                {
                    // Si falla, intentar como objeto con propiedad data
                    try
                    {
                        var phoneResponse = JsonSerializer.Deserialize<PhoneResponseModel>(jsonContent, new JsonSerializerOptions 
                        { 
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            PropertyNameCaseInsensitive = true 
                        });
                        
                        if (phoneResponse?.Data != null)
                        {
                            DatabasePhonesList.Clear();
                            foreach (var phone in phoneResponse.Data.OrderByDescending(p => p.CreatedAt))
                            {
                                DatabasePhonesList.Add(phone);
                            }
                            OnPropertyChanged(nameof(DatabasePhonesCount));
                            return;
                        }
                    }
                    catch
                    {
                        // Si ambos fallan, mostrar error
                        await CustomAlert.ShowErrorAsync(
                            "No se pudo procesar la respuesta del servidor.\n\nFormato de datos no reconocido.",
                            "Error de Formato");
                        return;
                    }
                }
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                var errorMessage = response.StatusCode switch
                {
                    System.Net.HttpStatusCode.NotFound => "El servicio de consulta de teléfonos no está disponible.",
                    System.Net.HttpStatusCode.Unauthorized => "Tu sesión ha expirado. Por favor, inicia sesión nuevamente.",
                    System.Net.HttpStatusCode.InternalServerError => "Error interno del servidor. Intenta nuevamente en unos minutos.",
                    _ => $"Error del servidor ({response.StatusCode}). No se pudieron cargar los teléfonos."
                };

                await CustomAlert.ShowErrorAsync(errorMessage, "Error al Cargar");
            }
        }
        catch (HttpRequestException httpEx)
        {
            await CustomAlert.ShowErrorAsync(
                $"No se pudo conectar con el servidor.\n\n" +
                $"Verifica tu conexión a internet.\n\n" +
                $"Detalles: {httpEx.Message}",
                "Error de Conexión");
        }
        catch (TaskCanceledException)
        {
            await CustomAlert.ShowErrorAsync(
                "La consulta tardó demasiado tiempo en responder.\n\n" +
                "Intenta nuevamente.",
                "Tiempo Agotado");
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync(
                $"Error inesperado al cargar los teléfonos.\n\n" +
                $"Detalles: {ex.Message}",
                "Error del Sistema");
        }
        finally
        {
            IsLoadingDatabasePhones = false;
        }
    }

    public async Task RefreshDatabasePhonesAsync()
    {
        await CustomAlert.ShowInfoAsync("Actualizando lista de teléfonos...", "Cargando");
        await LoadDatabasePhonesAsync();
        
        if (DatabasePhonesList.Any())
        {
            await CustomAlert.ShowSuccessAsync(
                $"Lista actualizada exitosamente.\n\n" +
                $"Total de teléfonos en base de datos: {DatabasePhonesCount}",
                "Actualización Completa");
        }
    }

    private void ClearForm()
    {
        PhoneNumber = string.Empty;
        OwnerName = string.Empty;
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}