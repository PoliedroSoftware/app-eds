using System.Globalization;

namespace APP.Eds.Converters
{
    public class IsNotNullConverter : IValueConverter
    {
        public static readonly IsNotNullConverter Instance = new IsNotNullConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}