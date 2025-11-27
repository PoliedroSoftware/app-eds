using APP.Eds.UsesCases.Court;
using APP.Eds.UsesCases.Navigation;
using APP.Eds.Services.Authentication;
using APP.Eds.Services.VersionCheck;
using APP.Eds.Services.Alert;
using APP.Eds.Services.Config;

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
        
        // Check for app updates (controlled by Configuration.EnableAutoVersionCheck flag)
        if (Configuration.EnableAutoVersionCheck)
        {
            CheckForUpdates();
        }
    }
    
    /// <summary>
    /// Checks for app updates on Google Play Store and shows version information.
    /// Always displays current version and notifies if an update is available.
    /// This method runs asynchronously and does not block app startup.
    /// Can be called manually for testing: ((App)Application.Current).CheckForUpdates();
    /// </summary>
    public async void CheckForUpdates()
    {
        try
        {
            // Create service instance (consider using DI in future if App constructor is refactored)
            using var versionCheckService = new VersionCheckService();
            var currentVersion = versionCheckService.GetCurrentVersion();
            var latestVersion = await versionCheckService.GetLatestVersionAsync();
            
            // Ensure UI updates happen on main thread
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (!string.IsNullOrEmpty(latestVersion))
                {
                    var updateAvailable = await versionCheckService.IsUpdateAvailableAsync();
                    
                    if (updateAvailable)
                    {
                        // Update available - show notification with update option
                        var shouldUpdate = await AlertService.ShowConfirmAsync(
                            $"Versión actual: {currentVersion}\n\nHay una nueva versión ({latestVersion}) disponible en Google Play Store. ¿Deseas actualizar ahora?",
                            "Actualización Disponible",
                            "Actualizar",
                            "Más tarde"
                        );
                        
                        if (shouldUpdate)
                        {
                            // Open Play Store app page
                            await Launcher.OpenAsync(new Uri("https://play.google.com/store/apps/details?id=com.companyname.app.EDS"));
                        }
                    }
                    else
                    {
                        // No update available - show current version info
                        await AlertService.ShowInfoAsync(
                            $"Versión actual: {currentVersion}\n\nTienes la última versión disponible en Google Play Store.",
                            "Información de Versión"
                        );
                    }
                }
                else
                {
                    // Could not fetch Play Store version - show current version only
                    await AlertService.ShowInfoAsync(
                        $"Versión actual: {currentVersion}\n\nNo se pudo verificar la versión en Google Play Store.",
                        "Información de Versión"
                    );
                }
            });
        }
        catch (Exception ex)
        {
            // Show error with current version info
            System.Diagnostics.Debug.WriteLine($"Error checking for updates: {ex.Message}");
            
            try
            {
                using var versionCheckService = new VersionCheckService();
                var currentVersion = versionCheckService.GetCurrentVersion();
                
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await AlertService.ShowInfoAsync(
                        $"Versión actual: {currentVersion}\n\nNo se pudo verificar actualizaciones en este momento.",
                        "Información de Versión"
                    );
                });
            }
            catch
            {
                // Silently fail if we can't even show current version
            }
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