using System.Globalization;

namespace APP.Eds.Converters
{
    public class NumberToWordsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return string.Empty;

            if (decimal.TryParse(value.ToString(), out decimal amount))
            {
                if (amount == 0)
                    return string.Empty;

                return ConvertToWords(amount);
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static string ConvertToWords(decimal amount)
        {
            try
            {
                if (amount == 0)
                    return string.Empty;

                long integerPart = (long)Math.Floor(amount);
                int decimalPart = (int)((amount - integerPart) * 100);

                string words = ConvertIntegerToWords(integerPart);
                
                if (decimalPart > 0)
                {
                    words += $" con {decimalPart:00}/100";
                }

                // Agregar "pesos" al final
                words += " pesos";

                // Capitalizar la primera letra
                if (!string.IsNullOrEmpty(words))
                {
                    words = char.ToUpper(words[0]) + words.Substring(1);
                }

                return words;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string ConvertIntegerToWords(long number)
        {
            if (number == 0)
                return "cero";

            string[] units = { "", "uno", "dos", "tres", "cuatro", "cinco", "seis", "siete", "ocho", "nueve" };
            string[] teens = { "diez", "once", "doce", "trece", "catorce", "quince", "dieciséis", "diecisiete", "dieciocho", "diecinueve" };
            string[] tens = { "", "", "veinte", "treinta", "cuarenta", "cincuenta", "sesenta", "setenta", "ochenta", "noventa" };
            string[] hundreds = { "", "ciento", "doscientos", "trescientos", "cuatrocientos", "quinientos", "seiscientos", "setecientos", "ochocientos", "novecientos" };

            if (number < 10)
                return units[number];

            if (number < 20)
                return teens[number - 10];

            if (number < 100)
            {
                int ten = (int)(number / 10);
                int unit = (int)(number % 10);
                
                if (ten == 2 && unit > 0)
                    return "veinti" + units[unit];
                
                return tens[ten] + (unit > 0 ? " y " + units[unit] : "");
            }

            if (number == 100)
                return "cien";

            if (number < 1000)
            {
                int hundred = (int)(number / 100);
                long remainder = number % 100;
                
                string result = hundreds[hundred];
                if (remainder > 0)
                    result += " " + ConvertIntegerToWords(remainder);
                
                return result;
            }

            if (number < 1000000)
            {
                long thousands = number / 1000;
                long remainder = number % 1000;
                
                string result = "";
                if (thousands == 1)
                    result = "mil";
                else
                    result = ConvertIntegerToWords(thousands) + " mil";
                
                if (remainder > 0)
                    result += " " + ConvertIntegerToWords(remainder);
                
                return result;
            }

            if (number < 1000000000)
            {
                long millions = number / 1000000;
                long remainder = number % 1000000;
                
                string result = "";
                if (millions == 1)
                    result = "un millón";
                else
                    result = ConvertIntegerToWords(millions) + " millones";
                
                if (remainder > 0)
                    result += " " + ConvertIntegerToWords(remainder);
                
                return result;
            }

            // Para números más grandes, simplificar
            return "cantidad muy grande";
        }
    }
}