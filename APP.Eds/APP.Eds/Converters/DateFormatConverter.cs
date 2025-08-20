using System.Globalization;

namespace APP.Eds.Converters
{
    /// <summary>
    /// Converter to ensure consistent date formatting across the application.
    /// Standardizes date display format to dd/MM/yyyy.
    /// </summary>
    public class DateFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                return dateTime.ToString("dd/MM/yyyy", culture);
            }
            
            if (value is DateOnly dateOnly)
            {
                return dateOnly.ToString("dd/MM/yyyy", culture);
            }
            
            if (value is string dateString && DateTime.TryParse(dateString, out var parsedDate))
            {
                return parsedDate.ToString("dd/MM/yyyy", culture);
            }
            
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string dateString && !string.IsNullOrEmpty(dateString))
            {
                // Try to parse date string back to DateTime
                if (DateTime.TryParseExact(dateString, "dd/MM/yyyy", culture, DateTimeStyles.None, out var dateTime) ||
                    DateTime.TryParse(dateString, culture, out dateTime))
                {
                    if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
                    {
                        return dateTime;
                    }
                    
                    if (targetType == typeof(DateOnly) || targetType == typeof(DateOnly?))
                    {
                        return DateOnly.FromDateTime(dateTime);
                    }
                }
            }
            
            return value;
        }
    }
}