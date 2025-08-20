using System.Globalization;

namespace APP.Eds.Converters
{
    public class NullToEmptyStringConverter : IValueConverter
    {
        public static readonly NullToEmptyStringConverter Instance = new NullToEmptyStringConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return string.Empty;
            
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}