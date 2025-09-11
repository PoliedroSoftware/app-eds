using Microsoft.Maui.Controls;
using System.Globalization;

namespace APP.Eds.Converters
{
    public class MenuIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string title)
            {
                return title switch
                {
                    var t when t.Contains("Configuraci�n Inicial") => "?????",
                    var t when t.Contains("Administraci�n") => "??",
                    var t when t.Contains("Dispensadores") => "?",
                    var t when t.Contains("Compras") => "??",
                    var t when t.Contains("Tanques") => "???",
                    var t when t.Contains("EDS y otros") => "??",
                    var t when t.Contains("Inventario") => "??",
                    _ => "??"
                };
            }
            return "??";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}