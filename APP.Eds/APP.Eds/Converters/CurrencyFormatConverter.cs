using System.Globalization;
using Microsoft.Maui.Controls;

namespace APP.Eds.Converters
{
    public class CurrencyFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                return $"$ {FormatDecimalValue(doubleValue)}";
            }
            
            if (value is decimal decimalValue)
            {
                return $"$ {FormatDecimalValue((double)decimalValue)}";
            }
            
            return "$ 0";
        }

        private string FormatDecimalValue(double value)
        {
            // Crear cultura personalizada con formato español (punto para miles, coma para decimales)
            var customCulture = new CultureInfo("es-ES");
            
            // Si el número es entero (sin decimales), mostrar sin decimales
            if (value == Math.Floor(value))
            {
                return value.ToString("N0", customCulture);
            }
            
            // Determinar cuántos decimales significativos tiene el número (máximo 3)
            string tempFormat = value.ToString("F10", customCulture); // Usar muchos decimales temporalmente
            
            // Encontrar cuántos decimales realmente necesitamos (eliminando ceros finales)
            var parts = tempFormat.Split(',');
            if (parts.Length > 1)
            {
                string decimalsString = parts[1].TrimEnd('0');
                int decimalPlaces = Math.Min(decimalsString.Length, 3); // máximo 3 decimales
                
                if (decimalPlaces == 0)
                {
                    return value.ToString("N0", customCulture);
                }
                else
                {
                    string format = $"N{decimalPlaces}";
                    return value.ToString(format, customCulture);
                }
            }
            
            return value.ToString("N0", customCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue && !string.IsNullOrWhiteSpace(stringValue))
            {
                // Remover el símbolo $ y espacios
                stringValue = stringValue.Replace("$", "").Trim();
                
                var customCulture = new CultureInfo("es-ES");
                if (double.TryParse(stringValue, NumberStyles.Number, customCulture, out double result))
                {
                    return result;
                }
            }
            
            return 0.0;
        }
    }
}