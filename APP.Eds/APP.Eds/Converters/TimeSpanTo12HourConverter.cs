using System.Globalization;
using Microsoft.Maui.Controls;

namespace APP.Eds.Converters;

public class TimeSpanTo12HourConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TimeSpan timeSpan)
        {
            DateTime dateTime = DateTime.Today.Add(timeSpan);
            // Use Spanish culture for AM/PM formatting
            var spanishCulture = new CultureInfo("es-CO");
            return dateTime.ToString("h:mm tt", spanishCulture);
        }

        if (value is string timeString)
        {
            var spanishCulture = new CultureInfo("es-CO");

            // Try parsing as TimeSpan first (format: HH:mm:ss)
            if (TimeSpan.TryParse(timeString, out TimeSpan parsedTime))
            {
                DateTime dateTime = DateTime.Today.Add(parsedTime);
                return dateTime.ToString("h:mm tt", spanishCulture);
            }

            // Try parsing as DateTime (in case the string is already a time format)
            if (DateTime.TryParse(timeString, out DateTime parsedDateTime))
            {
                return parsedDateTime.ToString("h:mm tt", spanishCulture);
            }

            // If we can't parse it, try some common formats
            string[] timeFormats = { "HH:mm:ss", "HH:mm", "H:mm:ss", "H:mm" };
            foreach (string format in timeFormats)
            {
                if (DateTime.TryParseExact(timeString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                {
                    return result.ToString("h:mm tt", spanishCulture);
                }
            }
        }

        return value?.ToString() ?? "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string timeString)
        {
            var spanishCulture = new CultureInfo("es-CO");
            if (DateTime.TryParseExact(timeString, "h:mm tt", spanishCulture, DateTimeStyles.None, out DateTime result))
            {
                return result.TimeOfDay;
            }
        }
        return TimeSpan.Zero;
    }
}