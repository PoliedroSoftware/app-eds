using System.Globalization;

namespace APP.Eds.Converters
{
    /// <summary>
    /// Converter to handle time format conversion between 24-hour storage format and 12-hour AM/PM display format.
    /// - Database storage: Always in 24-hour format (HH:mm:ss)
    /// - UI display: 12-hour format with AM/PM (h:mm tt)
    /// </summary>
    public class TimeFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan timeSpan)
            {
                // Convert TimeSpan to 12-hour format with AM/PM
                var dateTime = DateTime.Today.Add(timeSpan);
                return dateTime.ToString("h:mm tt", culture);
            }
            
            if (value is DateTime dateTime2)
            {
                // Convert DateTime to 12-hour format with AM/PM
                return dateTime2.ToString("h:mm tt", culture);
            }
            
            if (value is string timeString && TimeSpan.TryParse(timeString, out var parsedTime))
            {
                // Convert string representation to 12-hour format with AM/PM
                var dateTime3 = DateTime.Today.Add(parsedTime);
                return dateTime3.ToString("h:mm tt", culture);
            }
            
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string timeString && !string.IsNullOrEmpty(timeString))
            {
                // Try to parse 12-hour format back to TimeSpan (24-hour format)
                if (DateTime.TryParseExact(timeString, "h:mm tt", culture, DateTimeStyles.None, out var dateTime) ||
                    DateTime.TryParseExact(timeString, "hh:mm tt", culture, DateTimeStyles.None, out dateTime) ||
                    DateTime.TryParse(timeString, culture, out dateTime))
                {
                    if (targetType == typeof(TimeSpan) || targetType == typeof(TimeSpan?))
                    {
                        return dateTime.TimeOfDay;
                    }
                    
                    if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
                    {
                        return dateTime;
                    }
                    
                    if (targetType == typeof(string))
                    {
                        // Return in 24-hour format for database storage
                        return dateTime.ToString("HH:mm:ss", culture);
                    }
                }
            }
            
            return value;
        }
    }
}