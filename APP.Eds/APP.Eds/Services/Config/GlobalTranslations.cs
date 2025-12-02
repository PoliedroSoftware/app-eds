namespace APP.Eds.Services.Config;

public static class GlobalTranslations
{
    private static Dictionary<string, string> _translations = new();

    // Default fallback translations for common keys
    private static readonly Dictionary<string, string> _defaultTranslations = new()
    {
        { "GestionCapacity", "Gestión de Capacidad" }
    };

    public static void SetTranslations(Dictionary<string, string> translations)
    {
        _translations = translations;
    }

    public static string Get(string key)
    {
        // First try to get from API translations
        if (_translations.TryGetValue(key, out var value))
        {
            return value;
        }
        
        // Then try fallback translations
        if (_defaultTranslations.TryGetValue(key, out var defaultValue))
        {
            return defaultValue;
        }
        
        // Finally return the key itself
        return key;
    }
}
