namespace APP.Eds.Constants;

/// <summary>
/// Constants for Shopping module validation messages
/// </summary>
public static class ShoppingValidationMessages
{
    /// <summary>
    /// Message displayed when no EDS is selected
    /// </summary>
    public const string EdsRequired = 
        "Debe seleccionar una Estación de Servicio (EDS) antes de agregar productos.\n\n" +
        "Por favor, seleccione un EDS en el campo correspondiente y luego intente agregar productos.";
    
    /// <summary>
    /// Title for EDS required error
    /// </summary>
    public const string EdsRequiredTitle = "EDS Requerido";
    
    /// <summary>
    /// Title for no products available warning
    /// </summary>
    public const string NoProductsTitle = "Sin Productos Disponibles";
    
    /// <summary>
    /// Gets the message for when an EDS has no products configured
    /// </summary>
    /// <param name="edsName">Name of the EDS without article (e.g., "Eds prueba", "La paz")</param>
    /// <returns>Formatted message</returns>
    public static string GetNoProductsMessage(string edsName)
    {
        // Always use "la EDS" article pattern for consistency, wrapping the name in quotes
        string displayName = $"la EDS '{edsName}'";
        
        return $"No hay productos ni compartimentos configurados para {displayName}.\n\n" +
               "Por favor, verifique:\n" +
               "• Que la EDS tenga tanques asignados\n" +
               "• Que los tanques tengan compartimentos\n" +
               "• Que los compartimentos tengan productos";
    }
}
