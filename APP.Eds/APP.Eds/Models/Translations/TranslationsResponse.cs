namespace APP.Eds.Models.Translations;

public class TranslationsResponse
{
    public Dictionary<string, Dictionary<string, string>> Translations { get; set; }
    public List<Language> Languages { get; set; }
}
