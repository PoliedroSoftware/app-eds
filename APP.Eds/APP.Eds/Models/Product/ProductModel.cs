using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace APP.Eds.Models.Product;

public class ProductModel : INotifyPropertyChanged
{
    private bool _isInCart;

    public string Name { get; set; }
    public int IdProductType { get; set; }
    public double SellPrice { get; set; }
    public double PurchasePrice { get; set; }
    public int Stock { get; set; }

    public bool IsInCart
    {
        get => _isInCart;
        set
        {
            _isInCart = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
