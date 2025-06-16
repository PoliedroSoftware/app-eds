using APP.Eds.Services.Navigation;
using APP.Eds.Services.Authentication;

namespace APP.Eds.UsesCases.Navigation;

public partial class Main : ContentPage
{
    private KeycloakSessionManager _sessionManager;
    public Main()
	{
        InitializeComponent();
        BindingContext = new MainService();
        _sessionManager = new KeycloakSessionManager();
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