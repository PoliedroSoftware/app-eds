using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using APP.Eds.Services.Translations;
using APP.Eds.Services.Config;
using System.Globalization;

namespace APP.Eds
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<TranslationsService>();
#if DEBUG
            builder.Logging.AddDebug();
#endif
            var app = builder.Build();


            return app;
        }
    }
}