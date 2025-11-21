using APP.Eds.Components.PopUp;
using APP.Eds.Helpers;
using APP.Eds.Models.IoT;
using APP.Eds.Services.Config;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace APP.Eds.Services.IoT;

/// <summary>
/// Servicio para controlar dispositivos IoT mediante AWS
/// </summary>
public class IoTService : INotifyPropertyChanged
{
    private readonly string? _authToken;
    private readonly HttpClient _httpClient;
    private const string IoT_API_URL = "https://mg5tj7bmve.execute-api.us-east-2.amazonaws.com/eds/api/v1/iot/publish";

    public event PropertyChangedEventHandler? PropertyChanged;

    // Estado de la válvula
    private bool _valveIsOpen;
    public bool ValveIsOpen
    {
        get => _valveIsOpen;
        set
        {
            if (_valveIsOpen != value)
            {
                _valveIsOpen = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ValveStatusText));
                OnPropertyChanged(nameof(ValveStatusColor));
            }
        }
    }

    // Texto del estado de la válvula
    public string ValveStatusText => ValveIsOpen ? "Valvula Abierta" : "Valvula Cerrada";

    // Color del estado de la válvula
    public Color ValveStatusColor => ValveIsOpen ? Color.FromArgb("#4CAF50") : Color.FromArgb("#F44336");

    // Estado de carga
    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (_isLoading != value)
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }
    }

    // Último mensaje de error
    private string? _lastError;
    public string? LastError
    {
        get => _lastError;
        set
        {
            if (_lastError != value)
            {
                _lastError = value;
                OnPropertyChanged();
            }
        }
    }

    public IoTService()
    {
        _authToken = TokenHelper.LoadToken();
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    /// <summary>
    /// Publica un mensaje IoT para controlar la válvula
    /// </summary>
    /// <param name="openValve">true para abrir la válvula, false para cerrarla</param>
    /// <returns>true si la operación fue exitosa</returns>
    public async Task<bool> PublishValveCommandAsync(bool openValve)
    {
        IsLoading = true;
        LastError = null;

        try
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                LastError = "No se encontró el token de autenticación";
                await CustomAlert.ShowErrorAsync(LastError, "Error de Autenticación");
                return false;
            }

            // Crear el request IoT
            var iotRequest = new IoTRequest
            {
                Message = new IoTMessage
                {
                    Input1 = "0",  // Siempre "0"
                    Input2 = "0",  // Siempre "0"
                    // Lógica correcta: Abierta (1,0) | Cerrada (0,1)
                    Output1 = openValve ? "1" : "0",
                    Output2 = openValve ? "0" : "1"
                },
                Topic = "psr/dev/psr-4g-at/cmd/"
            };

            // Configurar headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_authToken}");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Serializar el request
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
            var json = JsonSerializer.Serialize(iotRequest, jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            System.Diagnostics.Debug.WriteLine($"?? Enviando comando IoT: {json}");

            // Enviar el request
            var response = await _httpClient.PostAsync(IoT_API_URL, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"?? Respuesta IoT ({response.StatusCode}): {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                // Parsear la respuesta
                var iotResponse = JsonSerializer.Deserialize<IoTResponse>(responseContent, jsonOptions);

                if (iotResponse?.Success == true && iotResponse.Data?.Success == true)
                {
                    // ? Actualizar el estado de la válvula sin mostrar alerta
                    ValveIsOpen = openValve;
                    
                    // Log silencioso para debugging
                    System.Diagnostics.Debug.WriteLine($"? Válvula {(openValve ? "ABIERTA" : "CERRADA")} exitosamente");

                    return true;
                }
                else
                {
                    LastError = iotResponse?.Message ?? "Error desconocido en la respuesta";
                    await CustomAlert.ShowErrorAsync(
                        $"?? Error en la Respuesta\n\n" +
                        $"La API respondió correctamente pero con un error:\n\n" +
                        $"{LastError}",
                        "Error del Servidor");
                    return false;
                }
            }
            else
            {
                LastError = $"Error HTTP {response.StatusCode}: {responseContent}";
                await CustomAlert.ShowErrorAsync(
                    $"? Error de Comunicación\n\n" +
                    $"No se pudo comunicar con el servidor IoT:\n\n" +
                    $"Código: {response.StatusCode}\n" +
                    $"Detalles: {responseContent}",
                    "Error de Conexión");
                return false;
            }
        }
        catch (HttpRequestException httpEx)
        {
            LastError = $"Error de conexión: {httpEx.Message}";
            System.Diagnostics.Debug.WriteLine($"? Error HTTP: {httpEx.Message}");
            await CustomAlert.ShowErrorAsync(
                $"?? Error de Conexión\n\n" +
                $"No se pudo conectar con el servidor IoT de AWS:\n\n" +
                $"{httpEx.Message}\n\n" +
                $"Verifique su conexión a internet e intente nuevamente.",
                "Error de Red");
            return false;
        }
        catch (TaskCanceledException)
        {
            LastError = "Timeout: La operación tardó demasiado tiempo";
            System.Diagnostics.Debug.WriteLine("? Timeout en la operación IoT");
            await CustomAlert.ShowErrorAsync(
                $"?? Tiempo de Espera Agotado\n\n" +
                $"La operación tardó demasiado tiempo en completarse.\n\n" +
                $"Por favor, verifique su conexión e intente nuevamente.",
                "Timeout");
            return false;
        }
        catch (JsonException jsonEx)
        {
            LastError = $"Error al procesar la respuesta: {jsonEx.Message}";
            System.Diagnostics.Debug.WriteLine($"? Error JSON: {jsonEx.Message}");
            await CustomAlert.ShowErrorAsync(
                $"?? Error de Formato\n\n" +
                $"Error al procesar la respuesta del servidor:\n\n" +
                $"{jsonEx.Message}",
                "Error de Datos");
            return false;
        }
        catch (Exception ex)
        {
            LastError = $"Error inesperado: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"? Error inesperado: {ex.Message}\nStack: {ex.StackTrace}");
            await CustomAlert.ShowErrorAsync(
                $"? Error Inesperado\n\n" +
                $"Ocurrió un error inesperado:\n\n" +
                $"{ex.Message}",
                "Error del Sistema");
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Alterna el estado de la válvula (abre si está cerrada, cierra si está abierta)
    /// </summary>
    public async Task<bool> ToggleValveAsync()
    {
        return await PublishValveCommandAsync(!ValveIsOpen);
    }

    /// <summary>
    /// Abre la válvula
    /// </summary>
    public async Task<bool> OpenValveAsync()
    {
        return await PublishValveCommandAsync(true);
    }

    /// <summary>
    /// Cierra la válvula
    /// </summary>
    public async Task<bool> CloseValveAsync()
    {
        return await PublishValveCommandAsync(false);
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
