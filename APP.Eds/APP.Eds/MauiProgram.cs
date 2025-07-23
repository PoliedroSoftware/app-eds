using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using APP.Eds.Services.Translations;
using APP.Eds.Services.Config;
using APP.Eds.Ports;

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
            });

            builder.Services.AddSingleton<ITranslationsService, TranslationsService>();

#if DEBUG
            builder.Logging.AddDebug();
#endif
            var app = builder.Build();

            // Cargar traducciones al inicio
            Task.Run(async () =>
            {
                var translationService = app.Services.GetService<ITranslationsService>();
                if (translationService != null)
                {
                    var translations = await translationService.GetTranslationsByLanguageAsync("es"); // Asumiendo español como idioma predeterminado
                    GlobalTranslations.SetTranslations(translations);
                }
            }).Wait(); // Esperar a que las traducciones se carguen antes de continuar

            return app;
        }
    }
}