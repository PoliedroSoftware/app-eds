using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using APP.Eds.Services.Translations;
using APP.Eds.Services.Config;
using System.Globalization;

namespace APP.Eds
{
    public static class MauiProgram
    {
        public static async Task<MauiApp> CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit();

            builder.Services.AddSingleton<TranslationsService>();

            builder.UseMauiApp<App>().ConfigureFonts(fonts =>

            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            }).UseMauiCommunityToolkit();
#if DEBUG
            builder.Logging.AddDebug();
#endif
            var app = builder.Build();

            // Cargar traducciones al inicio
            var translationsService = app.Services.GetService<TranslationsService>();
            if (translationsService != null)
            {
                var currentCulture = CultureInfo.CurrentCulture.Name;
                var translations = await translationsService.GetTranslationsByLanguageAsync(currentCulture);
                GlobalTranslations.SetTranslations(translations);
            }

            return app;
        }
    }
}