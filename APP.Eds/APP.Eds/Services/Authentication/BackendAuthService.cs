using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace APP.Eds.Services.Authentication;

/// <summary>
/// Modelo de respuesta de login del backend
/// </summary>
public class LoginResponse
{
    public string AccessToken { get; set; }
    public int ExpiresIn { get; set; }
    public int RefreshExpiresIn { get; set; }
    public string RefreshToken { get; set; }
    public string TokenType { get; set; }
    public string Scope { get; set; }
}

/// <summary>
/// Modelo de request para el login
/// </summary>
public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}

/// <summary>
/// Servicio de autenticación que se comunica directamente con el backend
/// </summary>
public class BackendAuthService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public BackendAuthService(string baseUrl)
    {
        _httpClient = new HttpClient();
        _baseUrl = baseUrl;
    }

    /// <summary>
    /// Autentica al usuario contra el backend
    /// </summary>
    /// <param name="username">Nombre de usuario</param>
    /// <param name="password">Contraseña</param>
    /// <returns>Respuesta del login con el token de acceso</returns>
    public async Task<LoginResponse> LoginAsync(string username, string password)
    {
        try
        {
            var loginRequest = new LoginRequest
            {
                Username = username,
                Password = password
            };

            var json = JsonSerializer.Serialize(loginRequest, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/v1/auth/login", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error en login: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            
            var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return loginResponse;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en BackendAuthService.LoginAsync: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Valida si un token es válido
    /// </summary>
    /// <param name="token">Token a validar</param>
    /// <returns>True si el token es válido</returns>
    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Intentar hacer una llamada simple a un endpoint que requiera autenticación
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/v1/auth/validate");

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Refresca el token de acceso usando el refresh token
    /// </summary>
    /// <param name="refreshToken">Refresh token</param>
    /// <returns>Nueva respuesta de login con tokens actualizados</returns>
    public async Task<LoginResponse> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var content = new StringContent(
                JsonSerializer.Serialize(new { refreshToken }),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/v1/auth/refresh", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error refrescando token: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            
            return JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en BackendAuthService.RefreshTokenAsync: {ex.Message}");
            throw;
        }
    }
}
