using APP.Eds.Helpers;
using APP.Eds.Models.Translations;
using APP.Eds.Ports;
using APP.Eds.Services.Config;
using System.Net.Http.Headers;
using System.Text.Json;

namespace APP.Eds.Services.Translations;

public class TranslationsService : ITranslationsService
{
    private string? _authToken;


    public async Task<Dictionary<string, string>> GetTranslationsByLanguageAsync(string languageTag)
    {
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
        if (string.IsNullOrEmpty(_authToken))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
            return new Dictionary<string, string>();
        }
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
        var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/translations");
        var data = JsonSerializer.Deserialize<TranslationsResponse>(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return data.Translations.TryGetValue(languageTag, out var translations)
            ? translations
            : new Dictionary<string, string>();
    }

}

