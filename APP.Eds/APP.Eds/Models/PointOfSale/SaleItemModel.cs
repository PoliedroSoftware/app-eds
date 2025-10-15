using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace APP.Eds.Models.PointOfSale;

public class SaleItemModel : INotifyPropertyChanged
{
    private int _quantity;

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
        }
    }

    public double TotalPrice => UnitPrice * Quantity;
    public string ImageUrl { get; set; }
    public int Stock { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}