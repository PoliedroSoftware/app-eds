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
                return "$ " + FormatDecimalValue(doubleValue);
            }
            
            if (value is decimal decimalValue)
            {
                return "$ " + FormatDecimalValue((double)decimalValue);
            }
            
            return "$0";
        }

        private string FormatDecimalValue(double value)
        {
            // Crear cultura personalizada con formato US/Colombia (coma para miles, punto para decimales)
            var customCulture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            customCulture.NumberFormat.NumberGroupSeparator = ",";
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            
            // Mostrar 1 decimal fijo
            return value.ToString("N1", customCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue && !string.IsNullOrWhiteSpace(stringValue))
            {
                // Remover el símbolo $ y espacios
                stringValue = stringValue.Replace("$", "").Trim();
                
                var customCulture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
                customCulture.NumberFormat.NumberGroupSeparator = ",";
                customCulture.NumberFormat.NumberDecimalSeparator = ".";
                
                if (double.TryParse(stringValue, NumberStyles.Number, customCulture, out double result))
                {
                    return result;
                }
            }
            
            return 0.0;
        }
    }
}