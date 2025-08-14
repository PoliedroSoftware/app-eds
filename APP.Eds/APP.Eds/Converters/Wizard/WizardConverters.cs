using System.Globalization;

namespace APP.Eds.Converters.Wizard
{
    public class PercentageToProgressConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double percentage)
            {
                return percentage / 100.0;
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double progress)
            {
                return progress * 100.0;
            }
            return 0.0;
        }
    }

    public class StepBackgroundConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length >= 3 && 
                values[0] is bool isCompleted && 
                values[1] is bool isActive && 
                values[2] is bool isEnabled)
            {
                if (isCompleted) return Colors.Green;
                if (isActive) return Colors.Purple;
                if (isEnabled) return Colors.Orange;
                return Colors.Gray;
            }
            
            return Colors.Gray;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StepTextColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length >= 3 && 
                values[0] is bool isCompleted && 
                values[1] is bool isActive && 
                values[2] is bool isEnabled)
            {
                if (isCompleted) return Colors.Green;
                if (isActive) return Colors.Purple;
                if (isEnabled) return Colors.Black;
                return Colors.Gray;
            }
            
            return Colors.Gray;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StepStatusIconConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length >= 2 && 
                values[0] is bool isCompleted && 
                values[1] is bool isActive)
            {
                if (isCompleted) return "✓";
                if (isActive) return "▶";
                return "";
            }
            
            return "";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StepStatusColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length >= 2 && 
                values[0] is bool isCompleted && 
                values[1] is bool isActive)
            {
                if (isCompleted) return Colors.Green;
                if (isActive) return Colors.Purple;
                return Colors.Gray;
            }
            
            return Colors.Gray;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}