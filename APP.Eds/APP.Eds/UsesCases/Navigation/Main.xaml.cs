using APP.Eds.Services.Navigation;
using APP.Eds.Services.Authentication;

namespace APP.Eds.UsesCases.Navigation;

public partial class Main : ContentPage
{
    private BackendSessionManager _sessionManager;
    
    public Main()
    {
        InitializeComponent();
        _sessionManager = new BackendSessionManager();
        BindingContext = new MainService();
        
        // Cargar información del usuario en el badge
        LoadUserBadge();
        
        // Cargar versión de la aplicación
        LoadAppVersion();
    }
    
    private void LoadUserBadge()
    {
        try
        {
            // Obtener el nombre de usuario desde las preferencias
            var username = Preferences.Get("Usernamelogin", "Usuario");
            
            // Actualizar el label del nombre de usuario
            if (UserNameLabel != null)
            {
                UserNameLabel.Text = !string.IsNullOrWhiteSpace(username) ? username : "Usuario";
            }
            
            System.Diagnostics.Debug.WriteLine($"Main.LoadUserBadge: Usuario cargado - {username}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Main.LoadUserBadge: Error cargando información del usuario: {ex.Message}");
        }
    }
    
    private void LoadAppVersion()
    {
        try
        {
            // Obtener versión de la aplicación
            var version = AppInfo.Current.VersionString;
            var build = AppInfo.Current.BuildString;
            
            // Actualizar el label de versión
            if (VersionLabel != null)
            {
                VersionLabel.Text = $"v{version} ({build})";
            }
            
            System.Diagnostics.Debug.WriteLine($"Main.LoadAppVersion: Versión cargada - {version} ({build})");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Main.LoadAppVersion: Error cargando versión: {ex.Message}");
            if (VersionLabel != null)
            {
                VersionLabel.Text = "v1.0.1";
            }
        }
    }
    
    private async void OnUpdateClicked(object sender, EventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("Main.OnUpdateClicked: Abriendo Google Play Store");
            
            // Package name de la aplicación desde el .csproj
            var packageName = "com.companyname.app.EDS";
            
            // URL del Play Store
            var playStoreUrl = $"https://play.google.com/store/apps/details?id={packageName}";
            
            // Verificar si se puede abrir la URL
            var canOpen = await Launcher.CanOpenAsync(playStoreUrl);
            
            if (canOpen)
            {
                await Launcher.OpenAsync(new Uri(playStoreUrl));
                System.Diagnostics.Debug.WriteLine($"Main.OnUpdateClicked: Play Store abierto exitosamente");
            }
            else
            {
                // Si no se puede abrir, mostrar mensaje al usuario
                await DisplayAlert(
                    "No disponible", 
                    "No se pudo abrir Google Play Store. Verifica que tengas la aplicación instalada.", 
                    "OK");
                System.Diagnostics.Debug.WriteLine($"Main.OnUpdateClicked: No se pudo abrir Play Store");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Main.OnUpdateClicked: Error - {ex.Message}");
            await DisplayAlert(
                "Error", 
                "Ocurrió un error al intentar abrir Google Play Store", 
                "OK");
        }
    }
    
    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        LoadingOverlay.IsVisible = true;

        await Task.Delay(1000);

        _sessionManager.ClearSession();

        LoadingOverlay.IsVisible = false;

        Application.Current.MainPage = new NavigationPage(new MainPage());
    }
}