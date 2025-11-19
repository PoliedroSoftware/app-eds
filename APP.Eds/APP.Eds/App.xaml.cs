using APP.Eds.UsesCases.Court;
using APP.Eds.UsesCases.Navigation;
using APP.Eds.Services.Authentication;
using APP.Eds.Services.VersionCheck;
using APP.Eds.Services.Alert;

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
        
        var sessionManager = new KeycloakSessionManager();

        
        sessionManager.ClearCurrentSession();
        MainPage = new NavigationPage(new MainPage());
    }

    protected override void OnStart()
    {
        base.OnStart();
        // Ensure theme is applied on app start
        RequestedThemeChanged += OnRequestedThemeChanged;
        
        // Check for app updates
        CheckForUpdates();
    }
    
    /// <summary>
    /// Checks for app updates on Google Play Store and notifies the user if an update is available.
    /// This method runs asynchronously and does not block app startup.
    /// </summary>
    private async void CheckForUpdates()
    {
        try
        {
            // Create service instance (consider using DI in future if App constructor is refactored)
            using var versionCheckService = new VersionCheckService();
            var updateAvailable = await versionCheckService.IsUpdateAvailableAsync();
            
            if (updateAvailable)
            {
                var latestVersion = await versionCheckService.GetLatestVersionAsync();
                var currentVersion = versionCheckService.GetCurrentVersion();
                
                // Ensure UI updates happen on main thread
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var shouldUpdate = await AlertService.ShowConfirmAsync(
                        $"Hay una nueva versión ({latestVersion}) disponible en Google Play Store. Tu versión actual es {currentVersion}. ¿Deseas actualizar ahora?",
                        "Actualización Disponible",
                        "Actualizar",
                        "Más tarde"
                    );
                    
                    if (shouldUpdate)
                    {
                        // Open Play Store app page
                        await Launcher.OpenAsync(new Uri("https://play.google.com/store/apps/details?id=com.companyname.app.EDS"));
                    }
                });
            }
        }
        catch (Exception ex)
        {
            // Silently fail - don't disrupt user experience if version check fails
            System.Diagnostics.Debug.WriteLine($"Error checking for updates: {ex.Message}");
        }
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