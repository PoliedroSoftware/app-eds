using System.Text;
using System.Text.Json;
using APP.Eds.Services.Alert;
using APP.Eds.UsesCases;

namespace APP.Eds.Services.Authentication;

public class AuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly IAlertService _alertService;

    public AuthenticationService()
    {
        _httpClient = new HttpClient();
        _alertService = new AlertService();
        _httpClient.BaseAddress = new Uri("https://poliedroapigateway1.azure-api.net/reader/api/v1/authentication/login");

    }

    public async Task<string> Login(string codeCountry, string phone, string password)
    {
        var loginData = new
        {
            codeCountry = "+57",
            phone,
            password
        };
        var content = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("login", content);

        if (password != null)
        {

            if (response.IsSuccessStatusCode)
            {
                Application.Current.MainPage = new NavigationPage(new CourtView())
                {
                    BarBackgroundColor = Color.FromArgb("#6200E8"),
                    BarTextColor = Color.FromArgb("#FFFFFF"),
                };
            }
            else
            {
                await _alertService.ShowAlert("Oops! Service is not working ", "Api server no found. Please try again.", "OK");
            }
        }


        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> Register(string codeCountry, string phone, string password)
    {
        var registerData = new
        {
            codeCountry,
            phone,
            password
        };
        var content = new StringContent(JsonSerializer.Serialize(registerData), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("registry", content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> VerifyPhone(string codeCountry, string phone)
    {
        var verifyData = new
        {
            codeCountry,
            phone
        };
        var content = new StringContent(JsonSerializer.Serialize(verifyData), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("verifyphone", content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
