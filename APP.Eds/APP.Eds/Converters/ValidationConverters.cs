using System.Globalization;
using System.Collections;

namespace APP.Eds.Converters
{
    public class IsNotNullOrEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
                return !string.IsNullOrEmpty(str);
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CountToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                bool hasItems = false;
                
                if (value is ICollection collection)
                {
                    hasItems = collection.Count > 0;
                    System.Diagnostics.Debug.WriteLine($"CountToBoolConverter - Collection count: {collection.Count}, hasItems: {hasItems}");
                }
                else if (value is int count)
                {
                    hasItems = count > 0;
                    System.Diagnostics.Debug.WriteLine($"CountToBoolConverter - Int count: {count}, hasItems: {hasItems}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"CountToBoolConverter - Value is null or invalid type: {value?.GetType().Name ?? "null"}");
                }

                // Check if we need to invert the result
                if (parameter?.ToString()?.ToLower() == "inverse")
                {
                    return !hasItems;
                }

                return hasItems;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CountToBoolConverter Error: {ex.Message}");
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StringNotEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = !string.IsNullOrWhiteSpace(value?.ToString());
            System.Diagnostics.Debug.WriteLine($"StringNotEmptyConverter - Value: '{value}', Result: {result}");
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return false;
        }
    }

    // Nuevo convertidor para cambiar colores basado en valor booleano
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string colorPair)
            {
                var colors = colorPair.Split('|');
                if (colors.Length == 2)
                {
                    // Si el bool es true, usa el primer color, si es false usa el segundo
                    return Color.FromArgb(boolValue ? colors[0] : colors[1]);
                }
            }
            
            // Color por defecto
            return Color.FromArgb("#2196F3");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Nuevo convertidor para verificar si un string no está vacío
    public class StringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !string.IsNullOrWhiteSpace(value?.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}