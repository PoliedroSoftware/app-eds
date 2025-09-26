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
    private string? _authToken;

    // Collections
    public ObservableCollection<PhoneModel> PhonesList { get; set; } = new();

    // Properties for form
    private string _phoneNumber = string.Empty;
    public string PhoneNumber
    {
        get => _phoneNumber;
        set
        {
            _phoneNumber = value;
            OnPropertyChanged(nameof(PhoneNumber));
            OnPropertyChanged(nameof(FullPhoneNumber));
            ValidatePhoneNumber();
        }
    }

    // Validation properties
    private bool _showPhoneError;
    public bool ShowPhoneError
    {
        get => _showPhoneError;
        set
        {
            _showPhoneError = value;
            OnPropertyChanged(nameof(ShowPhoneError));
        }
    }

    private string _phoneErrorMessage = string.Empty;
    public string PhoneErrorMessage
    {
        get => _phoneErrorMessage;
        set
        {
            _phoneErrorMessage = value;
            OnPropertyChanged(nameof(PhoneErrorMessage));
        }
    }

    // Display properties
    public string FullPhoneNumber => string.IsNullOrEmpty(PhoneNumber) ? "+57" : $"+57{PhoneNumber}";

    // Statistics
    private int _totalPhones;
    public int TotalPhones
    {
        get => _totalPhones;
        set
        {
            _totalPhones = value;
            OnPropertyChanged(nameof(TotalPhones));
        }
    }

    private int _phonesToday;
    public int PhonesToday
    {
        get => _phonesToday;
        set
        {
            _phonesToday = value;
            OnPropertyChanged(nameof(PhonesToday));
        }
    }

    // Commands
    public ICommand SavePhoneCommand { get; private set; }
    public ICommand DeletePhoneCommand { get; private set; }
    public ICommand ClearFormCommand { get; private set; }
    public ICommand SendPhonesCommand { get; private set; }

    public PhoneService()
    {
        _httpClient = new HttpClient();
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
        
        InitializeCommands();
        LoadPhonesAsync();
    }

    private void InitializeCommands()
    {
        SavePhoneCommand = new Command(async () => await SavePhoneAsync());
        DeletePhoneCommand = new Command<PhoneModel>(async (phone) => await DeletePhoneAsync(phone));
        ClearFormCommand = new Command(ClearForm);
        SendPhonesCommand = new Command(async () => await SendPhonesToServerAsync());
    }

    private void ValidatePhoneNumber()
    {
        ShowPhoneError = false;
        PhoneErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(PhoneNumber))
        {
            ShowPhoneError = true;
            PhoneErrorMessage = "El número de teléfono es requerido";
            return;
        }

        // Validar que solo contenga números
        if (!Regex.IsMatch(PhoneNumber, @"^\d+$"))
        {
            ShowPhoneError = true;
            PhoneErrorMessage = "El número solo debe contener dígitos";
            return;
        }

        // Validar longitud para números colombianos (10 dígitos típicamente)
        if (PhoneNumber.Length < 7 || PhoneNumber.Length > 12)
        {
            ShowPhoneError = true;
            PhoneErrorMessage = "El número debe tener entre 7 y 12 dígitos";
            return;
        }

        // Validar que no empiece con 0 para números colombianos
        if (PhoneNumber.StartsWith("0"))
        {
            ShowPhoneError = true;
            PhoneErrorMessage = "El número no debe empezar con 0";
            return;
        }

        // Verificar si el número ya existe
        if (PhonesList.Any(p => p.Number == PhoneNumber))
        {
            ShowPhoneError = true;
            PhoneErrorMessage = "Este número de teléfono ya está registrado";
            return;
        }
    }

    public async Task SavePhoneAsync()
    {
        try
        {
            ValidatePhoneNumber();
            
            if (ShowPhoneError)
            {
                await CustomAlert.ShowErrorAsync(PhoneErrorMessage, "Error de Validación");
                return;
            }

            var newPhone = new PhoneModel
            {
                IdPhone = PhonesList.Count + 1, // Temporal ID
                Number = PhoneNumber,
                CountryCode = "57",
                CreatedAt = DateTime.Now
            };

            PhonesList.Insert(0, newPhone);
            UpdateStatistics();

            await CustomAlert.ShowSuccessAsync(
                $"Teléfono registrado exitosamente:\n\n" +
                $"• Número: {newPhone.FullNumber}\n" +
                $"• Fecha: {newPhone.CreatedAt:dd/MM/yyyy HH:mm}",
                "Registro Exitoso");

            ClearForm();
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync(
                $"Error al registrar el teléfono:\n\n{ex.Message}",
                "Error del Sistema");
        }
    }

    public async Task DeletePhoneAsync(PhoneModel phone)
    {
        try
        {
            var result = await CustomAlert.ShowConfirmAsync(
                $"¿Está seguro de eliminar el número {phone.FullNumber}?\n\n" +
                "Esta acción no se puede deshacer.",
                "Confirmar Eliminación",
                "Eliminar",
                "Cancelar");

            if (result)
            {
                PhonesList.Remove(phone);
                UpdateStatistics();
                
                await CustomAlert.ShowSuccessAsync(
                    $"Número {phone.FullNumber} eliminado correctamente",
                    "Eliminación Exitosa");
            }
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync(
                $"Error al eliminar el teléfono:\n\n{ex.Message}",
                "Error del Sistema");
        }
    }

    public async Task SendPhonesToServerAsync()
    {
        try
        {
            if (!PhonesList.Any())
            {
                await CustomAlert.ShowWarningAsync(
                    "No hay números de teléfono registrados para enviar",
                    "Lista Vacía");
                return;
            }

            if (string.IsNullOrEmpty(_authToken))
            {
                await CustomAlert.ShowErrorAsync(
                    "No se encontró el token de autenticación",
                    "Error de Autenticación");
                return;
            }

            // Crear el array de números completos con el indicativo +57
            var phoneNumbers = PhonesList
                .Select(p => p.FullNumber) // Ya incluye +57
                .ToList();

            // Crear el request en el formato que espera tu backend
            var request = new PhoneRequest
            {
                Request = new PhoneRequestData
                {
                    Numbers = phoneNumbers
                }
            };

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            // Usar el endpoint correcto /api/v1/phone (singular)
            var response = await _httpClient.PostAsync($"{Configuration.BaseUrl}/api/v1/phone", content);

            if (response.IsSuccessStatusCode)
            {
                await CustomAlert.ShowSuccessAsync(
                    $"✅ Números enviados exitosamente\n\n" +
                    $"• Total enviados: {phoneNumbers.Count}\n" +
                    $"• Estado: Procesados correctamente\n" +
                    $"• Indicativo: +57 (Colombia)\n\n" +
                    $"Los números se han guardado automáticamente en la base de datos.",
                    "Envío Exitoso");

                // Limpiar la lista local después del envío exitoso
                PhonesList.Clear();
                UpdateStatistics();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await CustomAlert.ShowErrorAsync(
                    $"Error del servidor: {response.StatusCode}\n\n{error}",
                    "Error al Enviar");
            }
        }
        catch (HttpRequestException)
        {
            await CustomAlert.ShowErrorAsync(
                "No se pudo conectar con el servidor.\n\n" +
                "Verifique su conexión a internet e intente nuevamente.",
                "Error de Conexión");
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync(
                $"Error inesperado al enviar los teléfonos:\n\n{ex.Message}",
                "Error del Sistema");
        }
    }

    private async Task LoadPhonesAsync()
    {
        try
        {
            // Inicializar estadísticas
            UpdateStatistics();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando teléfonos: {ex.Message}");
        }
    }

    private void UpdateStatistics()
    {
        TotalPhones = PhonesList.Count;
        PhonesToday = PhonesList.Count(p => p.CreatedAt.Date == DateTime.Today);
    }

    private void ClearForm()
    {
        PhoneNumber = string.Empty;
        ShowPhoneError = false;
        PhoneErrorMessage = string.Empty;
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