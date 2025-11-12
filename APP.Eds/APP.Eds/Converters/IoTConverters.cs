using System.Globalization;

namespace APP.Eds.Converters;

/// <summary>
/// Convertidor que convierte un bool a "1" o "0" para mostrar en UI
/// </summary>
public class BoolToOutputConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? "1" : "0";
        }
        return "0";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string stringValue)
        {
            return stringValue == "1";
        }
        return false;
    }
}

/// <summary>
/// Convertidor que invierte un valor booleano
/// </summary>
public class InvertedBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return false;
    }
}

/// <summary>
/// Convertidor para Input 2: muestra "1" cuando la válvula está cerrada (false), "0" cuando está abierta (true)
/// </summary>
public class BoolToInput2Converter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            // Invertido: cerrado (false) = "1", abierto (true) = "0"
            return boolValue ? "0" : "1";
        }
        return "1"; // Por defecto cerrado
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string stringValue)
        {
            // Invertido: "1" = cerrado (false), "0" = abierto (true)
            return stringValue == "0";
        }
        return false;
    }
}
