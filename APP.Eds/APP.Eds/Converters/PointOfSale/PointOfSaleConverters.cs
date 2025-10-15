using APP.Eds.Models.PointOfSale;
using System.Globalization;

namespace APP.Eds.Converters.PointOfSale;

public class PaymentMethodColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is PaymentMethod paymentMethod && parameter is string methodString)
        {
            var targetMethod = Enum.Parse<PaymentMethod>(methodString);
            return paymentMethod == targetMethod ? 
                Color.FromArgb("#1976D2") : // Primary color
                Color.FromArgb("#9E9E9E");   // Gray color
        }
        return Color.FromArgb("#9E9E9E");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class PaymentMethodVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is PaymentMethod paymentMethod && parameter is string methodString)
        {
            var targetMethod = Enum.Parse<PaymentMethod>(methodString);
            return paymentMethod == targetMethod;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ChangeColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double change)
        {
            return change >= 0 ? 
                Color.FromArgb("#4CAF50") : // Green
                Color.FromArgb("#F44336");   // Red
        }
        return Color.FromArgb("#9E9E9E");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}