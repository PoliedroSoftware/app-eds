using System.Globalization;

namespace APP.Eds.Converters
{
    public class SafeCountConverter : IValueConverter
    {
        public static readonly SafeCountConverter Instance = new SafeCountConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "0";
                
            if (value is int intValue)
                return intValue.ToString();
                
            if (value is System.Collections.ICollection collection)
                return collection.Count.ToString();
                
            return "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}