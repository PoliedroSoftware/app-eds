
using APP.Eds.Services.Config;

namespace APP.Eds.Services.Authentication;

public class KeycloakService
{
    
    public async Task<string> AuthenticateRawJsonAsync(
        string username,
        string password)
    {
        using var client = new HttpClient();

        var tokenEndpoint = $"{Configuration.KeycloakUrl}/{Configuration.KeycloakRealms}/protocol/openid-connect/token";

        var requestData = new Dictionary<string, string>
        {
            {"client_id", Configuration.KeycloakCliendId},
            {"grant_type", "password"},
            {"username", username},
            {"password", password}
        };

        var requestContent = new FormUrlEncodedContent(requestData);
        var response = await client.PostAsync(tokenEndpoint, requestContent);
        return await response.Content.ReadAsStringAsync();
    }
}
