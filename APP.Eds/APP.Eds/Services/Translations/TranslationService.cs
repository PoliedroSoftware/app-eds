using APP.Eds.Helpers;
using APP.Eds.Models.Translations;
using APP.Eds.Ports;
using APP.Eds.Services.Config;
using System.Net.Http.Headers;
using System.Text.Json;

namespace APP.Eds.Services.Translations;

public class TranslationsService : ITranslationsService
{
    public static string CurrentLanguage { get; private set; } = "es-CO"; // Valor por defecto
    private string? _authToken;


    public async Task<Dictionary<string, string>> GetTranslationsByLanguageAsync(string languageTag)
    {
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
        if (string.IsNullOrEmpty(_authToken))
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert("Error de Token", "No se encontró el token de autenticación. La traducción no funcionará.", "OK");
            }
            return new Dictionary<string, string>();
        }

        await Application.Current.MainPage.DisplayAlert("Info de Traducción", $"Intentando cargar traducciones para el idioma: {languageTag}", "OK");
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
        try
        {
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/translations");
            await Application.Current.MainPage.DisplayAlert("Respuesta API", $"Respuesta de la API de traducciones: {response.Substring(0, Math.Min(response.Length, 200))}...", "OK"); // Mostrar parte de la respuesta

            var data = JsonSerializer.Deserialize<TranslationsResponse>(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (data != null && data.Translations.TryGetValue(languageTag, out var translations))
            {
                await Application.Current.MainPage.DisplayAlert("Traducciones Cargadas", $"Se cargaron {translations.Count} traducciones para el idioma {languageTag}.", "OK");
                return translations;
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Error de Traducción", $"No se encontraron traducciones para el idioma {languageTag} en la respuesta de la API.", "OK");
                return new Dictionary<string, string>();
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error de Red/Deserialización", $"Error al obtener o procesar traducciones: {ex.Message}", "OK");
            return new Dictionary<string, string>();

        }
}
    }

