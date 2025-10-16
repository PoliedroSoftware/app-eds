using APP.Eds.Models.PointOfSale;
using APP.Eds.Models.Product;
using APP.Eds.Services.PointOfSale;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace APP.Eds.UsesCases.PointOfSale;

public class PointOfSaleViewModel : INotifyPropertyChanged
{
    private readonly IPointOfSaleService _pointOfSaleService;
    private bool _isLoading;
    private double _cashReceived;
    private double _change;
    private PaymentMethod _selectedPaymentMethod = PaymentMethod.Cash;

    public PointOfSaleViewModel(IPointOfSaleService pointOfSaleService)
    {
        _pointOfSaleService = pointOfSaleService;
        Products = new ObservableCollection<ProductModel>();
        CartItems = new ObservableCollection<SaleItemModel>();
        
        AddToCartCommand = new Command<ProductModel>(AddToCart);
        RemoveFromCartCommand = new Command<SaleItemModel>(RemoveFromCart);
        IncreaseQuantityCommand = new Command<SaleItemModel>(IncreaseQuantity);
        DecreaseQuantityCommand = new Command<SaleItemModel>(DecreaseQuantity);
        UpdateTotalAmountCommand = new Command<SaleItemModel>(UpdateTotalAmount);
        ProcessPaymentCommand = new Command(async () => await ProcessPayment(), CanProcessPayment);
        ClearCartCommand = new Command(ClearCart);
        SelectPaymentMethodCommand = new Command<string>(SelectPaymentMethod);
        
        LoadProducts();
    }

    public ObservableCollection<ProductModel> Products { get; }
    public ObservableCollection<SaleItemModel> CartItems { get; }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public double SubTotal => CartItems.Sum(item => item.TotalPrice);
    public double Tax => SubTotal * 0.16; // 16% tax
    public double Total => SubTotal + Tax;

    public double CashReceived
    {
        get => _cashReceived;
        set
        {
            SetProperty(ref _cashReceived, value);
            Change = value - Total;
            OnPropertyChanged(nameof(Change));
            OnPropertyChanged(nameof(CanCompleteTransaction));
        }
    }

    public double Change
    {
        get => _change;
        private set => SetProperty(ref _change, value);
    }

    public PaymentMethod SelectedPaymentMethod
    {
        get => _selectedPaymentMethod;
        set => SetProperty(ref _selectedPaymentMethod, value);
    }

    public bool CanCompleteTransaction => 
        CartItems.Any() && 
        (SelectedPaymentMethod == PaymentMethod.Card || CashReceived >= Total);

    public ICommand AddToCartCommand { get; }
    public ICommand RemoveFromCartCommand { get; }
    public ICommand IncreaseQuantityCommand { get; }
    public ICommand DecreaseQuantityCommand { get; }
    public ICommand UpdateTotalAmountCommand { get; }
    public ICommand ProcessPaymentCommand { get; }
    public ICommand ClearCartCommand { get; }
    public ICommand SelectPaymentMethodCommand { get; }

    private async void LoadProducts()
    {
        IsLoading = true;
        try
        {
            var products = await _pointOfSaleService.GetAvailableProductsAsync();
            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }
            UpdateProductCartStatus();
        }
        catch (Exception ex)
        {
            // Handle error
            await Application.Current.MainPage.DisplayAlert("Error", 
                "No se pudieron cargar los productos", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void AddToCart(ProductModel product)
    {
        if (product.Stock <= 0) return;

        var existingItem = CartItems.FirstOrDefault(item => item.ProductName == product.Name);
        if (existingItem != null)
        {
            if (existingItem.Quantity < product.Stock)
            {
                existingItem.Quantity++;
                existingItem.TotalAmount = existingItem.TotalPrice; // Update total amount to reflect quantity change
                existingItem.TotalAmountText = existingItem.TotalAmount.ToString("F0"); // Update text representation
                OnPropertyChanged(nameof(SubTotal));
                OnPropertyChanged(nameof(Tax));
                OnPropertyChanged(nameof(Total));
                OnPropertyChanged(nameof(CanCompleteTransaction));
            }
        }
        else
        {
            var newItem = new SaleItemModel
            {
                ProductName = product.Name,
                UnitPrice = product.SellPrice,
                Quantity = 1,
                Stock = product.Stock
            };
            newItem.TotalAmount = product.SellPrice; // Initialize with unit price
            newItem.TotalAmountText = product.SellPrice.ToString("F0"); // Initialize text representation
            CartItems.Add(newItem);
            OnPropertyChanged(nameof(SubTotal));
            OnPropertyChanged(nameof(Tax));
            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(CanCompleteTransaction));
        }
        
        UpdateProductCartStatus();
    }

    private void RemoveFromCart(SaleItemModel item)
    {
        CartItems.Remove(item);
        OnPropertyChanged(nameof(SubTotal));
        OnPropertyChanged(nameof(Tax));
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(CanCompleteTransaction));
        UpdateProductCartStatus();
    }

    private void IncreaseQuantity(SaleItemModel item)
    {
        if (item.Quantity < item.Stock)
        {
            item.Quantity++;
            item.TotalAmount = item.TotalPrice; // Update total amount
            item.TotalAmountText = item.TotalAmount.ToString("F0"); // Update text representation
            OnPropertyChanged(nameof(SubTotal));
            OnPropertyChanged(nameof(Tax));
            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(CanCompleteTransaction));
        }
    }

    private void DecreaseQuantity(SaleItemModel item)
    {
        if (item.Quantity > 1)
        {
            item.Quantity--;
            item.TotalAmount = item.TotalPrice; // Update total amount
            item.TotalAmountText = item.TotalAmount.ToString("F0"); // Update text representation
            OnPropertyChanged(nameof(SubTotal));
            OnPropertyChanged(nameof(Tax));
            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(CanCompleteTransaction));
        }
        else
        {
            RemoveFromCart(item);
        }
        UpdateProductCartStatus();
    }

    private void UpdateTotalAmount(SaleItemModel item)
    {
        // This method is called when the total amount entry changes
        // The calculation is handled in the SaleItemModel.TotalAmount setter
        OnPropertyChanged(nameof(SubTotal));
        OnPropertyChanged(nameof(Tax));
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(CanCompleteTransaction));
    }

    private void SelectPaymentMethod(string method)
    {
        if (Enum.TryParse<PaymentMethod>(method, out var paymentMethod))
        {
            SelectedPaymentMethod = paymentMethod;
            OnPropertyChanged(nameof(CanCompleteTransaction));
        }
    }

    private async Task ProcessPayment()
    {
        if (!CanCompleteTransaction) return;

        IsLoading = true;
        try
        {
            var sale = new SaleModel
            {
                Items = CartItems.ToList(),
                Tax = Tax,
                PaymentMethod = SelectedPaymentMethod,
                Status = SaleStatus.Pending
            };

            var success = await _pointOfSaleService.ProcessSaleAsync(sale);
            
            if (success)
            {
                await Application.Current.MainPage.DisplayAlert("Éxito", 
                    $"Venta procesada correctamente\nTotal: ${Total:F2}\nCambio: ${Change:F2}", "OK");
                ClearCart();
                LoadProducts(); // Recargar para actualizar stock
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Error", 
                    "No se pudo procesar la venta. Verifique el stock disponible.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", 
                "Ocurrió un error al procesar la venta", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanProcessPayment()
    {
        return CanCompleteTransaction && !IsLoading;
    }

    private void ClearCart()
    {
        CartItems.Clear();
        CashReceived = 0;
        OnPropertyChanged(nameof(SubTotal));
        OnPropertyChanged(nameof(Tax));
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(CanCompleteTransaction));
        UpdateProductCartStatus();
    }

    private void UpdateProductCartStatus()
    {
        foreach (var product in Products)
        {
            product.IsInCart = CartItems.Any(item => item.ProductName == product.Name);
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return false;

        backingStore = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}