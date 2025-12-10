using APP.Eds.Services.PointOfSale;
using APP.Eds.Services.VersionCheck;
using APP.Eds.Services.WhatsApp;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace APP.Eds
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit();
            builder.UseMauiApp<App>().ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            }).UseMauiCommunityToolkit();

            // Register Services
            builder.Services.AddSingleton<IPointOfSaleService, PointOfSaleService>();
            builder.Services.AddSingleton<IWhatsAppMessageService, WhatsAppMessageService>();

            // Register Version Check Service
            builder.Services.AddSingleton<IVersionCheckService, VersionCheckService>();

#if DEBUG
            builder.Logging.AddDebug();
#endif
            return builder.Build();
        }
    }
}