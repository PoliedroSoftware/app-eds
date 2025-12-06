using APP.Eds.Helpers;
using APP.Eds.Models.Court;
using APP.Eds.Models.WhastApp;
using APP.Eds.Services.Config;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace APP.Eds.Services.WhatsApp;

public interface IWhatsAppMessageService
{
    Task SendMessageAsync(string phoneNumber, CourtModel court);
}

public class WhatsAppMessageService : IWhatsAppMessageService
{
    private readonly string? _authToken;

    public WhatsAppMessageService()
    {
        _authToken = TokenHelper.LoadToken();
    }

    /// <summary>
    /// Enviar la factura al WhatsApp del cliente desde el backend (si existe endpoint)
    /// </summary>
    public async Task SendMessageAsync(string phoneNumber, CourtModel court)
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            WhatsAppMessageRequest request = new WhatsAppMessageRequest()
            {
                PhoneNumber = phoneNumber,
                Court = court
            };

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var url = $"{Configuration.BaseUrl}/api/v1/send-message";
            var response = await httpClient.PostAsync(url, content);

        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"⚠️ Error Al enviar factura al WhatsApp: {ex.Message}");
        }
    }
}