namespace APP.Eds.Services.Config;

public static class GlobalTranslations
{
    private static Dictionary<string, string> _translations = new();

    public static void SetTranslations(Dictionary<string, string> translations)
    {
        _translations = translations;
    }

    public static string Get(string key)
    {
        return _translations.TryGetValue(key, out var value) ? value : key;
    }
}
