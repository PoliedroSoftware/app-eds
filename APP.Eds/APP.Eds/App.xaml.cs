using APP.Eds.UsesCases.Court;
using APP.Eds.UsesCases.Navigation;
using APP.Eds.Services.Authentication;

namespace APP.Eds;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        HandlerInitialize();
        
        // Ensure app follows system theme (Light/Dark mode)
        // UserAppTheme = AppTheme.Unspecified allows system theme to control
        UserAppTheme = AppTheme.Unspecified;
        
        var sessionManager = new BackendSessionManager();
        
        // Limpiar sesión anterior
        sessionManager.ClearSession();
        MainPage = new NavigationPage(new MainPage());
    }

    protected override void OnStart()
    {
        base.OnStart();
        // Ensure theme is applied on app start
        RequestedThemeChanged += OnRequestedThemeChanged;
    }

    private void OnRequestedThemeChanged(object sender, AppThemeChangedEventArgs e)
    {
        // This method will be called when system theme changes
        // Force a refresh of the current page to apply new theme
        if (MainPage != null)
        {
            MainPage.Handler?.DisconnectHandler();
        }
    }

    private void HandlerInitialize()
    {
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("EntryCustomization", (handler, view) =>
        {
#if ANDROID
            handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
#elif __IOS__
            handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
            handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#elif WINDOWS
            handler.PlatformView.FontWeight = Microsoft.UI.Text.FontWeights.Thin;
#endif
        });
    }
}