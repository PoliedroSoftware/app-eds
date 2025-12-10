using System.Globalization;

namespace APP.Eds.Converters;

/// <summary>
/// Convertidor genérico que permite usar funciones lambda para conversiones
/// </summary>
public class FuncConverter<TSource, TTarget> : IValueConverter
{
    private readonly Func<TSource, TTarget> _convertFunc;
    private readonly Func<TTarget, TSource> _convertBackFunc;

    public FuncConverter(Func<TSource, TTarget> convertFunc, Func<TTarget, TSource> convertBackFunc = null)
    {
        _convertFunc = convertFunc ?? throw new ArgumentNullException(nameof(convertFunc));
        _convertBackFunc = convertBackFunc;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TSource sourceValue)
        {
            return _convertFunc(sourceValue);
        }
        return default(TTarget);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (_convertBackFunc == null)
        {
            throw new NotImplementedException("ConvertBack is not implemented for this converter");
        }

        if (value is TTarget targetValue)
        {
            return _convertBackFunc(targetValue);
        }
        return default(TSource);
    }
}
