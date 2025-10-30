using System.Diagnostics;
using System.Text.Json;
using System.Text;
using APP.Eds.Services.Authentication;
using APP.Eds.UsesCases.Navigation;
using APP.Eds.Services.Config;
using APP.Eds.Helpers;
using APP.Eds.Models.Translations;

namespace APP.Eds
{
    public partial class MainPage : ContentPage
    {

        private readonly KeycloakService _keycloakService = new();
        private readonly string _clientId;
        private readonly string _realm;
        
        public MainPage()
        {

            _clientId = Configuration.KeycloakCliendId;
            _realm = Configuration.KeycloakRealms;
      
            InitializeComponent();


            var existingToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
            if (!string.IsNullOrEmpty(existingToken))
            {
                Application.Current.MainPage = new NavigationPage(new Main());

            }
        }

        
        private async void OnLoginClicked(object sender, EventArgs e)
        {
            try
            {
                LoadingSection.IsVisible = true;
                LoadingIndicator.IsVisible = true;
                LoadingIndicator.IsRunning = true;
                ErrorSection.IsVisible = false;
                LoginButton.IsEnabled = false;

                string username = UsernameEntry.Text;
                string password = PasswordEntry.Text;

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    var (message, icon, bgColor, textColor) = ErrorMessages.GetFriendlyErrorMessage("invalid_input");
                    ShowError(message, icon, bgColor, textColor);
                    return;
                }

                var jsonResponse = await _keycloakService.AuthenticateRawJsonAsync(username, password);

                var tokenResponse = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
                var token = tokenResponse.GetProperty("access_token").GetString();


                if (string.IsNullOrEmpty(token))
                {
                    var (message, icon, bgColor, textColor) = ErrorMessages.GetFriendlyErrorMessage("token_missing");
                    ShowError(message, icon, bgColor, textColor);
                    return;
                }

               
                var tokenParts = token.Split('.');
                if (tokenParts.Length < 2)
                {
                    var (message, icon, bgColor, textColor) = ErrorMessages.GetFriendlyErrorMessage("token_invalid");
                    ShowError(message, icon, bgColor, textColor);
                    return;
                }

                var payload = tokenParts[1];
                var jsonBytes = Convert.FromBase64String(PadBase64(payload));
                var jsonPayload = Encoding.UTF8.GetString(jsonBytes);

                var tokenPayload = JsonSerializer.Deserialize<JsonElement>(jsonPayload);

                var Username = tokenPayload.GetProperty("preferred_username").GetString();

                Preferences.Set("Usernamelogin", Username);

                // Collect roles from client (resource_access) and realm (realm_access), but do not require group membership
                var roles = new List<string>();

                if (tokenPayload.TryGetProperty("resource_access", out var resourceAccess))
                {
                    // Use configured client id instead of hard-coded value
                    if (resourceAccess.TryGetProperty(_clientId, out var clientAccess))
                    {
                        if (clientAccess.TryGetProperty("roles", out var clientRoles))
                        {
                            foreach (var r in clientRoles.EnumerateArray())
                            {
                                var role = r.GetString();
                                if (!string.IsNullOrEmpty(role)) roles.Add(role);
                            }
                        }
                    }
                }

                if (tokenPayload.TryGetProperty("realm_access", out var realmAccess))
                {
                    if (realmAccess.TryGetProperty("roles", out var realmRoles))
                    {
                        foreach (var r in realmRoles.EnumerateArray())
                        {
                            var role = r.GetString();
                            if (!string.IsNullOrEmpty(role)) roles.Add(role);
                        }
                    }
                }

                TokenHelper.SaveToken(token, _clientId, _realm);

                // If you need role-based UI, keep it, but default to allowing any authenticated user.
                if (roles.Contains("Admin", StringComparer.OrdinalIgnoreCase))
                {
                    Preferences.Set("userRole", "Admin");
                }
                else if (roles.Contains("User", StringComparer.OrdinalIgnoreCase))
                {
                    Preferences.Set("userRole", "User");
                }
                else
                {
                    // Default role for any authenticated user in the realm
                    Preferences.Set("userRole", "User");
                }

                Application.Current.MainPage = new NavigationPage(new Main());

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error durante el inicio de sesión: {ex.Message}");
                var (message, icon, bgColor, textColor) = ErrorMessages.GetFriendlyErrorMessage(ex.Message);
                ShowError(message, icon, bgColor, textColor);
            }
            finally
            {
                LoadingSection.IsVisible = false;
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
                LoginButton.IsEnabled = true;
            }
        }

       
        private void ShowError(string message, string icon, string backgroundColor, string textColor)
        {
            ErrorLabel.Text = message;
            ErrorIcon.Text = icon;
            ErrorSection.BackgroundColor = Color.FromArgb(backgroundColor);
            ErrorLabel.TextColor = Color.FromArgb(textColor);
            ErrorSection.IsVisible = true;
        }

        private string PadBase64(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: return base64 + "==";
                case 3: return base64 + "=";
                case 0: return base64;
                default: throw new FormatException("Invalid Base64 string");
            }
        }

        private void UsernameEntry_Completed(object sender, EventArgs e)
        {
            PasswordEntry.Focus();
        }

        private void OnRememberMeTapped(object sender, EventArgs e)
        {
            RememberMeCheckBox.IsChecked = !RememberMeCheckBox.IsChecked;
        }
    }
}
