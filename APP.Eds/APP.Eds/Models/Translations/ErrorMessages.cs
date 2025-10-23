namespace APP.Eds.Models.Translations;

public static class ErrorMessages
{
    public static Dictionary<string, (string Message, string Icon, string BackgroundColor, string TextColor)> Mappings = new()
    {
        { "invalid_grant", ("Usuario o contraseña incorrectos. Verifique sus datos e intente de nuevo.", "⚠️", "#FFEBEE", "#C62828") },
        { "user_not_found", ("El usuario no se encuentra registrado.", "⚠️", "#FFEBEE", "#C62828") },
        { "server_unavailable", ("Revise su conexión a internet.", "⚠️", "#FFEBEE", "#C62828") },
        { "invalid_input", ("ingresa tu usuario y contraseña.", "⚠️", "#FFEBEE", "#C62828") },
        { "token_missing", ("Fallo de autenticación.", "⚠️", "#FFEBEE", "#C62828") },
        { "token_invalid", ("Token inválido.", "⚠️", "#FFEBEE", "#C62828") },
        { "default", ("Intente nuevamente o contacte al administrador.", "⚠️", "#FFEBEE", "#C62828") }
    };

    public static (string Message, string Icon, string BackgroundColor, string TextColor) GetFriendlyErrorMessage(string technicalError)
    {
        if (technicalError.Contains("invalid_grant", StringComparison.OrdinalIgnoreCase))
        {
            return Mappings["invalid_grant"];
        }
        // Aquí se podrían añadir más condiciones para otros errores técnicos específicos
        // Por ejemplo, si el mensaje de excepción contiene "connection refused" o similar para "server_unavailable"
        if (technicalError.Contains("connection refused", StringComparison.OrdinalIgnoreCase) ||
            technicalError.Contains("No connection could be made", StringComparison.OrdinalIgnoreCase))
        {
            return Mappings["server_unavailable"];
        }
        
        // Si el error es "Arg_KeyNotF", que no se encontró, lo mapeamos a un error genérico por ahora.
        // Si se identifica un patrón más específico para "user_not_found" o similar, se puede añadir aquí.
        if (technicalError.Contains("Arg_KeyNotF", StringComparison.OrdinalIgnoreCase))
        {
            return Mappings["default"]; // O un mensaje más específico si se identifica el origen
        }

        // Manejo de errores específicos para la UI que no provienen directamente de la excepción del servidor
        if (technicalError.Equals("invalid_input", StringComparison.OrdinalIgnoreCase))
        {
            return Mappings["invalid_input"];
        }
        if (technicalError.Equals("token_missing", StringComparison.OrdinalIgnoreCase))
        {
            return Mappings["token_missing"];
        }
        if (technicalError.Equals("token_invalid", StringComparison.OrdinalIgnoreCase))
        {
            return Mappings["token_invalid"];
        }

        return Mappings["default"];
    }
}