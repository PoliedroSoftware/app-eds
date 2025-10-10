using APP.Eds.Models.ProductCompartiment;
using System.Globalization;

namespace APP.Eds.Converters
{
    public class ProductDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ProductModelResponse product)
            {
                return product.Name;
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}