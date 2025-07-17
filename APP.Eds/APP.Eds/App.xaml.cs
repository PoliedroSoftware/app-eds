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
        var sessionManager = new KeycloakSessionManager();

        
        sessionManager.ClearCurrentSession();
        MainPage = new NavigationPage(new MainPage());
        _ = InitializeTranslationsAsync(); // Llamar al método asíncrono sin esperar para no bloquear el constructor
    }

    private async Task InitializeTranslationsAsync()
    {
        var translationsService = Current.MainPage.Handler.MauiContext.Services.GetService<APP.Eds.Services.Translations.TranslationsService>();
        if (translationsService != null)
        {
            var currentCulture = System.Globalization.CultureInfo.CurrentCulture.Name;
            var translations = await translationsService.GetTranslationsByLanguageAsync(currentCulture);
            APP.Eds.Services.Config.GlobalTranslations.SetTranslations(translations);
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