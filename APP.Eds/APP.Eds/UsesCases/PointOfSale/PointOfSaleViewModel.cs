using APP.Eds.Models.PointOfSale;
using APP.Eds.Models.Product;
using APP.Eds.Models.Client;
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
    private ClientLegalModel _selectedClient;
    private bool _isClientSelectorVisible;
    private string _clientSearchText;
    private bool _isSearching;
    private string _clientWhatsAppNumber;

    public PointOfSaleViewModel(IPointOfSaleService pointOfSaleService)
    {
        _pointOfSaleService = pointOfSaleService;
        Products = new ObservableCollection<ProductModel>();
        CartItems = new ObservableCollection<SaleItemModel>();
        // Ya no necesitamos la lista de clientes precargada
        Clients = new ObservableCollection<ClientLegalModel>();

        AddToCartCommand = new Command<ProductModel>(AddToCart);
        RemoveFromCartCommand = new Command<SaleItemModel>(RemoveFromCart);
        IncreaseQuantityCommand = new Command<SaleItemModel>(IncreaseQuantity);
        DecreaseQuantityCommand = new Command<SaleItemModel>(DecreaseQuantity);
        UpdateTotalAmountCommand = new Command<SaleItemModel>(UpdateTotalAmount);
        ProcessPaymentCommand = new Command(async () => await ProcessPayment(), CanProcessPayment);
        ClearCartCommand = new Command(ClearCart);
        SelectPaymentMethodCommand = new Command<string>(SelectPaymentMethod);
        ToggleClientSelectorCommand = new Command(ToggleClientSelector);
        SearchClientCommand = new Command(async () => await SearchClient());

        LoadProducts();
        // Ya no llamamos LoadClients() aquí porque ahora buscaremos bajo demanda
    }

    public ObservableCollection<ProductModel> Products { get; }
    public ObservableCollection<SaleItemModel> CartItems { get; }
    public ObservableCollection<ClientLegalModel> Clients { get; }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public ClientLegalModel SelectedClient
    {
        get => _selectedClient;
        set
        {
            if (SetProperty(ref _selectedClient, value))
            {
                OnPropertyChanged(nameof(ClientButtonText));
                System.Diagnostics.Debug.WriteLine($"Cliente seleccionado: {value?.Name ?? "Ninguno"}");
            }
        }
    }

    public bool IsClientSelectorVisible
    {
        get => _isClientSelectorVisible;
        set => SetProperty(ref _isClientSelectorVisible, value);
    }

    public string ClientSearchText
    {
        get => _clientSearchText;
        set => SetProperty(ref _clientSearchText, value);
    }

    public bool IsSearching
    {
        get => _isSearching;
        set => SetProperty(ref _isSearching, value);
    }

    public string ClientWhatsAppNumber
    {
        get => _clientWhatsAppNumber;
        set => SetProperty(ref _clientWhatsAppNumber, value);
    }

    public string ClientButtonText
    {
        get
        {
            if (SelectedClient != null && SelectedClient.Id > 0)
            {
                // Mostrar: TipoDoc NumeroDoc - Nombre/Razón Social
                var docType = SelectedClient.DocumentType;
                var docNumber = SelectedClient.DocumentNumber;

                // Agregar dígito de verificación si es NIT
                if (SelectedClient.DocumentTypeId == 1 && SelectedClient.VerificationDigit > 0)
                {
                    docNumber = $"{docNumber}-{SelectedClient.VerificationDigit}";
                }

                // Convertir el nombre a mayúsculas
                var clientName = SelectedClient.Name.ToUpper();

                return $"{docType} {docNumber} - {clientName}";
            }

            return "Sin Cliente Seleccionado";
        }
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
    public ICommand ToggleClientSelectorCommand { get; }
    public ICommand SearchClientCommand { get; }

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

    private async Task SearchClient()
    {
        if (string.IsNullOrWhiteSpace(ClientSearchText))
        {
            await Application.Current.MainPage.DisplayAlert("Búsqueda de Cliente",
                        "Por favor ingrese un número de documento para buscar", "OK");
            return;
        }

        IsSearching = true;
        try
        {
            System.Diagnostics.Debug.WriteLine($"Buscando cliente con documento: {ClientSearchText}");

            var client = await _pointOfSaleService.SearchClientByDocumentAsync(ClientSearchText.Trim());

            if (client != null)
            {
                SelectedClient = client;
                ClientSearchText = string.Empty; // Limpiar el campo de búsqueda
                IsClientSelectorVisible = false; // Ocultar el selector

                // ✅ ELIMINADO: Ya no mostramos modal cuando se encuentra el cliente
                // El usuario verá el cliente seleccionado en la UI
            }
            else
            {
                // ⚠️ Solo mostramos modal cuando NO se encuentra el cliente
                await Application.Current.MainPage.DisplayAlert("Cliente No Encontrado",
                   $"No se encontró ningún cliente con el documento: {ClientSearchText}", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error buscando cliente: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Error",
     "Ocurrió un error al buscar el cliente. Por favor intente nuevamente.", "OK");
        }
        finally
        {
            IsSearching = false;
        }
    }

    private void ToggleClientSelector()
    {
        IsClientSelectorVisible = !IsClientSelectorVisible;
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
        if (!CanProcessPayment()) return;

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
                string clientInfo = SelectedClient != null && SelectedClient.Id > 0
                    ? $"\nCliente: {SelectedClient.Name}"
                    : "";

                await Application.Current.MainPage.DisplayAlert("Éxito",
                    $"Venta procesada correctamente{clientInfo}\nTotal: ${Total:F2}\nCambio: ${Change:F2}", "OK");
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
        SelectedClient = null;
        ClientWhatsAppNumber = string.Empty;
        IsClientSelectorVisible = false;
        OnPropertyChanged(nameof(SubTotal));
        OnPropertyChanged(nameof(Tax));
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(CanCompleteTransaction));
        OnPropertyChanged(nameof(ClientButtonText));
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