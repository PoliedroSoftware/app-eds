using System.Diagnostics;
using System.Text.Json;
using System.Text;
using APP.Eds.Services.Authentication;
using APP.Eds.UsesCases.Navigation;
using APP.Eds.Services.Config;
using APP.Eds.Helpers;

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
                LoadingIndicator.IsVisible = true;
                LoadingIndicator.IsRunning = true;
                ErrorLabel.IsVisible = false;

                string username = UsernameEntry.Text;
                string password = PasswordEntry.Text;

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    ShowError("Please enter both username and password");
                    return;
                }

                var jsonResponse = await _keycloakService.AuthenticateRawJsonAsync(username, password);

                var tokenResponse = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
                var token = tokenResponse.GetProperty("access_token").GetString();


                if (string.IsNullOrEmpty(token))
                {
                    ShowError("Authentication failed. No access token returned.");
                    return;
                }

               
                var tokenParts = token.Split('.');
                if (tokenParts.Length < 2)
                {
                    ShowError("Token inválido.");
                    return;
                }

                var payload = tokenParts[1];
                var jsonBytes = Convert.FromBase64String(PadBase64(payload));
                var jsonPayload = Encoding.UTF8.GetString(jsonBytes);

                var tokenPayload = JsonSerializer.Deserialize<JsonElement>(jsonPayload);

               
                var roles = tokenPayload
                    .GetProperty("resource_access")
                    .GetProperty("application-eds")
                    .GetProperty("roles")
                    .EnumerateArray()
                    .Select(r => r.GetString())
                    .ToList();

                Dictionary<string, bool> config = new Dictionary<string, bool>();
                if (tokenPayload.TryGetProperty("Config", out JsonElement configElement))
                {
                    foreach (var prop in configElement.EnumerateObject())
                    {
                        config[prop.Name] = prop.Value.GetBoolean();
                    }
                }

                var configJson = JsonSerializer.Serialize(config);
                Preferences.Set("userConfig", configJson);


                TokenHelper.SaveToken(token, _clientId, _realm);

                
                if (roles.Contains("Admin"))
                {
                    Preferences.Set("userRole", "Admin");
                    Application.Current.MainPage = new NavigationPage(new Main());
                }
                else if (roles.Contains("User"))
                {
                    Preferences.Set("userRole", "User");
                    Application.Current.MainPage = new NavigationPage(new Main());
                }
                else
                {
                    ShowError("Rol no autorizado.");
                }

            }
            catch (Exception ex)
            {
                ShowError($"An error occurred: {ex.Message}");
            }
            finally
            {
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
            }
        }

       
        private void ShowError(string message, string color = "Red")
        {
            ErrorLabel.Text = message;
            ErrorLabel.TextColor = color == "Red" ? Colors.Red : Colors.Green;
            ErrorLabel.IsVisible = true;
        }

        
        //private void SaveToken(string token, string clientId, string realm)
        //{

            
            
        //    Preferences.Set("CURRENT_AUTH_REALM", realm);
        //    Preferences.Set("CURRENT_AUTH_CLIENT_ID", clientId);

            
        //    string prefix = $"AUTH_{realm}_{clientId}_";
        //    var tokenParts = SplitTokenIntoParts(token);  
        //    Preferences.Set($"{prefix}TOKEN_PART_COUNT", tokenParts.Length);

        //    for (int i = 0; i < tokenParts.Length; i++)
        //    {
        //        Preferences.Set($"{prefix}TOKEN_PART_{i}", tokenParts[i]);
        //    }
        //}

        
        //private string[] SplitTokenIntoParts(string token)
        //{
        //    const int chunkSize = 512; 
        //    int partCount = (int)Math.Ceiling((double)token.Length / chunkSize);

        //    var parts = new string[partCount];
        //    for (int i = 0; i < partCount; i++)
        //    {
        //        int startIndex = i * chunkSize;
        //        int length = Math.Min(chunkSize, token.Length - startIndex);
        //        parts[i] = token.Substring(startIndex, length);
        //    }

        //    return parts;
        //}

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



        //private string LoadToken(string clientId, string realm)
        //{
        //    try
        //    {
        //        string prefix = $"AUTH_{realm}_{clientId}_";

               
        //        int chunkCount = Preferences.Get($"{prefix}TOKEN_PART_COUNT", 0);
        //        if (chunkCount == 0) return null; 

        //        var tokenBuilder = new StringBuilder();
        //        for (int i = 0; i < chunkCount; i++)
        //        {
        //            tokenBuilder.Append(Preferences.Get($"{prefix}TOKEN_PART_{i}", string.Empty));
        //        }
        //        return tokenBuilder.ToString();  
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"Error loading token: {ex}");
        //        return null;
        //    }
        //}
    }
}
