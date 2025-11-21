using APP.Eds.Services.Navigation;
using APP.Eds.Services.Authentication;

namespace APP.Eds.UsesCases.Navigation;

public partial class Main : ContentPage
{
    private KeycloakSessionManager _sessionManager;
    
    public Main()
    {
        InitializeComponent();
        _sessionManager = new KeycloakSessionManager();
        BindingContext = new MainService();
        
        // Cargar información del usuario en el badge
        LoadUserBadge();
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
    
    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        LoadingOverlay.IsVisible = true;

        await Task.Delay(1000);

        _sessionManager.ClearCurrentSession();

        LoadingOverlay.IsVisible = false;

        Application.Current.MainPage = new NavigationPage(new MainPage());
    }
}