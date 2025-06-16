namespace APP.Eds.Ports;

public interface ITranslationsService
{
    Task<Dictionary<string, string>> GetTranslationsByLanguageAsync(string languageTag);
}