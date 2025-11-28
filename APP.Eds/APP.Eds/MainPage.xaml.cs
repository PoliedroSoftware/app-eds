using System.Diagnostics;
using System.Text.Json;
using APP.Eds.Services.Authentication;
using APP.Eds.UsesCases.Navigation;
using APP.Eds.Services.Config;
using APP.Eds.Helpers;
using APP.Eds.Models.Translations;

namespace APP.Eds
{
    public partial class MainPage : ContentPage
    {
        private readonly BackendAuthService _authService;
        
        // WhatsApp support configuration
        private const string WHATSAPP_SUPPORT_NUMBER = "573154286798"; // +57 315 428 6798 (Poliedro Software - Soporte)
        private const string WHATSAPP_SUPPORT_MESSAGE = "Hola, necesito ayuda con la App EDS.";
        
        public MainPage()
        {
            _authService = new BackendAuthService(Configuration.BaseUrl);
            InitializeComponent();

            // Verificar si ya hay una sesión activa
            var existingToken = TokenHelper.LoadToken();
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

                // Llamar al backend directamente
                var loginResponse = await _authService.LoginAsync(username, password);

                if (loginResponse == null || string.IsNullOrEmpty(loginResponse.AccessToken))
                {
                    var (message, icon, bgColor, textColor) = ErrorMessages.GetFriendlyErrorMessage("token_missing");
                    ShowError(message, icon, bgColor, textColor);
                    return;
                }

                // Guardar el token del backend
                TokenHelper.SaveToken(loginResponse.AccessToken);
                
                // Guardar refresh token si existe
                if (!string.IsNullOrEmpty(loginResponse.RefreshToken))
                {
                    TokenHelper.SaveRefreshToken(loginResponse.RefreshToken);
                }

                // Guardar el username
                Preferences.Set("Usernamelogin", username);

                // Extraer roles del token JWT (si vienen en el payload)
                var roles = ExtractRolesFromToken(loginResponse.AccessToken);

                // Guardar el rol del usuario
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
                    // Rol por defecto para cualquier usuario autenticado
                    Preferences.Set("userRole", "User");
                }

                Application.Current.MainPage = new NavigationPage(new Main());
            }
            catch (HttpRequestException httpEx)
            {
                Debug.WriteLine($"Error HTTP durante el inicio de sesión: {httpEx.Message}");
                var (message, icon, bgColor, textColor) = ErrorMessages.GetFriendlyErrorMessage("network_error");
                ShowError(message, icon, bgColor, textColor);
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

        /// <summary>
        /// Extrae los roles del token JWT
        /// </summary>
        private List<string> ExtractRolesFromToken(string token)
        {
            var roles = new List<string>();
            
            try
            {
                var tokenParts = token.Split('.');
                if (tokenParts.Length < 2)
                {
                    return roles;
                }

                var payload = tokenParts[1];
                var jsonBytes = Convert.FromBase64String(PadBase64(payload));
                var jsonPayload = System.Text.Encoding.UTF8.GetString(jsonBytes);

                var tokenPayload = JsonSerializer.Deserialize<JsonElement>(jsonPayload);

                // Intentar obtener roles de resource_access
                if (tokenPayload.TryGetProperty("resource_access", out var resourceAccess))
                {
                    if (resourceAccess.TryGetProperty("application-eds", out var clientAccess))
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

                // Intentar obtener roles de realm_access
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
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error extrayendo roles del token: {ex.Message}");
            }

            return roles;
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

        /// <summary>
        /// Opens WhatsApp to contact support when help button is tapped
        /// </summary>
        private async void OnHelpButtonTapped(object sender, EventArgs e)
        {
            try
            {
                // Format: https://wa.me/<country_code><phone_number>?text=<pre-filled_message>
                string message = Uri.EscapeDataString(WHATSAPP_SUPPORT_MESSAGE);
                string whatsappUrl = $"https://wa.me/{WHATSAPP_SUPPORT_NUMBER}?text={message}";

                await Launcher.OpenAsync(new Uri(whatsappUrl));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al abrir WhatsApp: {ex.Message}");
                // Fallback: Try opening WhatsApp without pre-filled message
                try
                {
                    await Launcher.OpenAsync(new Uri($"https://wa.me/{WHATSAPP_SUPPORT_NUMBER}"));
                }
                catch (Exception fallbackEx)
                {
                    Debug.WriteLine($"Error al abrir WhatsApp (fallback): {fallbackEx.Message}");
                    await DisplayAlert("Error", "No se pudo abrir WhatsApp. Por favor, instala la aplicación o contacta al soporte.", "OK");
                }
            }
        }
    }
}
