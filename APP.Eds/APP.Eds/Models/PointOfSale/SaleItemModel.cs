using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace APP.Eds.Models.PointOfSale;

public class SaleItemModel : INotifyPropertyChanged
{
    private int _quantity;
    private double _totalAmount;
    private string _totalAmountText;

    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public double UnitPrice { get; set; }

    public int Quantity
    {
        get => _quantity;
        set
        {
            _quantity = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TotalPrice));
            OnPropertyChanged(nameof(TotalAmount));
            OnPropertyChanged(nameof(GallonsDisplay));
            OnPropertyChanged(nameof(TotalAmountCurrency));
            OnPropertyChanged(nameof(TotalAmountInWords));
        }
    }

    public double TotalAmount
    {
        get => _totalAmount;
        set
        {
            if (_totalAmount != value)
            {
                _totalAmount = Math.Max(0, value); // Ensure non-negative values
                
                // Calculate quantity (gallons) based on total amount and unit price
                if (UnitPrice > 0 && _totalAmount > 0)
                {
                    // Calculate gallons with precision (allowing decimal quantities)
                    var calculatedQuantity = _totalAmount / UnitPrice;
                    _quantity = (int)Math.Round(calculatedQuantity, 0, MidpointRounding.AwayFromZero);
                    OnPropertyChanged(nameof(Quantity));
                    OnPropertyChanged(nameof(CalculatedGallons));
                }
                else
                {
                    _quantity = 0;
                    OnPropertyChanged(nameof(Quantity));
                    OnPropertyChanged(nameof(CalculatedGallons));
                }
                
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalPrice));
                OnPropertyChanged(nameof(GallonsDisplay));
                OnPropertyChanged(nameof(TotalAmountCurrency));
                OnPropertyChanged(nameof(TotalAmountInWords));
            }
        }
    }

    // New property to show exact gallon calculation with decimals
    public double CalculatedGallons => UnitPrice > 0 ? _totalAmount / UnitPrice : 0;

    // Text property for binding to Entry to avoid formatting issues
    public string TotalAmountText
    {
        get => _totalAmountText ?? _totalAmount.ToString("F0");
        set
        {
            if (_totalAmountText != value)
            {
                _totalAmountText = value;
                
                // Try to parse the text to double, handle errors gracefully
                if (!string.IsNullOrWhiteSpace(value))
                {
                    // Remove any currency symbols or spaces
                    var cleanValue = value.Replace("$", "").Replace(",", "").Replace(" ", "");
                    
                    if (double.TryParse(cleanValue, out double parsedValue))
                    {
                        TotalAmount = parsedValue;
                    }
                }
                else
                {
                    TotalAmount = 0;
                }
                
                OnPropertyChanged();
            }
        }
    }

    public double TotalPrice => UnitPrice * Quantity;
    
    // Display property for gallons with proper formatting
    public string GallonsDisplay => $"{CalculatedGallons:F2}";
    
    // Display property for total amount in currency format
    public string TotalAmountCurrency => $"${TotalAmount:N0}";
    
    // Display property for total amount in words
    public string TotalAmountInWords => NumberToWords((int)TotalAmount);
    
    public string ImageUrl { get; set; }
    public int Stock { get; set; }
    
    // Function to convert numbers to words in Spanish
    private static string NumberToWords(int number)
    {
        if (number == 0) return "cero pesos";
        
        if (number < 0) return "menos " + NumberToWords(-number);
        
        string words = "";
        
        if (number / 1000000 > 0)
        {
            words += NumberToWords(number / 1000000) + " millón";
            if (number / 1000000 > 1) words += "es";
            words += " ";
            number %= 1000000;
        }
        
        if (number / 1000 > 0)
        {
            words += NumberToWords(number / 1000) + " mil ";
            number %= 1000;
        }
        
        if (number / 100 > 0)
        {
            words += GetHundreds(number / 100) + " ";
            number %= 100;
        }
        
        if (number > 0)
        {
            words += GetTens(number);
        }
        
        return (words.Trim() + " pesos").Trim();
    }
    
    private static string GetHundreds(int number)
    {
        return number switch
        {
            1 => "cien",
            2 => "doscientos",
            3 => "trescientos",
            4 => "cuatrocientos",
            5 => "quinientos",
            6 => "seiscientos",
            7 => "setecientos",
            8 => "ochocientos",
            9 => "novecientos",
            _ => ""
        };
    }
    
    private static string GetTens(int number)
    {
        if (number < 10) return GetOnes(number);
        if (number < 20) return GetTeens(number);
        
        string[] tens = { "", "", "veinte", "treinta", "cuarenta", "cincuenta", 
                         "sesenta", "setenta", "ochenta", "noventa" };
        
        if (number % 10 == 0) return tens[number / 10];
        
        if (number / 10 == 2)
            return "veinti" + GetOnes(number % 10);
        
        return tens[number / 10] + " y " + GetOnes(number % 10);
    }
    
    private static string GetTeens(int number)
    {
        return number switch
        {
            10 => "diez",
            11 => "once",
            12 => "doce",
            13 => "trece",
            14 => "catorce",
            15 => "quince",
            16 => "dieciséis",
            17 => "diecisiete",
            18 => "dieciocho",
            19 => "diecinueve",
            _ => ""
        };
    }
    
    private static string GetOnes(int number)
    {
        return number switch
        {
            1 => "uno",
            2 => "dos",
            3 => "tres",
            4 => "cuatro",
            5 => "cinco",
            6 => "seis",
            7 => "siete",
            8 => "ocho",
            9 => "nueve",
            _ => ""
        };
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}