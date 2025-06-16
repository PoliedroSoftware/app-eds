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
    }

    private void HandlerInitialize()
    {
        
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("EntryCustomization", (handler, view) =>
        {
#if __ANDROID__
            handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
#elif __IOS__
            handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#elif WINDOWS
            handler.PlatformView.FontWeight = Microsoft.UI.Text.FontWeights.Thin;
#endif
        });
    }
}