using APP.Eds.Models.Client;
using APP.Eds.Models.Court;
using APP.Eds.Models.PointOfSale;
using APP.Eds.Models.Product;
using APP.Eds.Services.Billing;
using APP.Eds.Services.PointOfSale;
using APP.Eds.Services.WhatsApp;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace APP.Eds.UsesCases.PointOfSale;

public class PointOfSaleViewModel : INotifyPropertyChanged
{
    private readonly IPointOfSaleService _pointOfSaleService;
    private readonly IWhatsAppMessageService _whatsAppMessageService;
    private readonly ElectronicBillingService _billingService;
    private bool _isLoading;
    private double _cashReceived;
    private double _change;
    private PaymentMethod _selectedPaymentMethod = PaymentMethod.Cash;
    private ClientLegalModel _selectedClient;
    private string _clientSearchText;
    private bool _isSearching;
    private string _clientWhatsAppNumber;

    public PointOfSaleViewModel(IPointOfSaleService pointOfSaleService, IWhatsAppMessageService whatsAppMessageService)
    {
        _pointOfSaleService = pointOfSaleService;
        _whatsAppMessageService = whatsAppMessageService;
        _billingService = new ElectronicBillingService();
        Products = [];
        CartItems = [];
        Clients = [];

        AddToCartCommand = new Command<ProductModel>(AddToCart);
        RemoveFromCartCommand = new Command<SaleItemModel>(RemoveFromCart);
        IncreaseQuantityCommand = new Command<SaleItemModel>(IncreaseQuantity);
        DecreaseQuantityCommand = new Command<SaleItemModel>(DecreaseQuantity);
        UpdateTotalAmountCommand = new Command<SaleItemModel>(UpdateTotalAmount);
        ProcessPaymentCommand = new Command(async () => await ProcessPayment());
        ClearCartCommand = new Command(ClearCart);
        SelectPaymentMethodCommand = new Command<string>(SelectPaymentMethod);
        SearchClientCommand = new Command(async () => await SearchClient());

        LoadProducts();
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
    public double Tax => SubTotal * 0;
    public double Total => SubTotal + Tax;

    public double CashReceived
    {
        get => _cashReceived;
        set
        {
            SetProperty(ref _cashReceived, value);
            Change = value - Total;
            OnPropertyChanged(nameof(Change));
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

    public bool CanCompleteTransaction => true;

    public ICommand AddToCartCommand { get; }
    public ICommand RemoveFromCartCommand { get; }
    public ICommand IncreaseQuantityCommand { get; }
    public ICommand DecreaseQuantityCommand { get; }
    public ICommand UpdateTotalAmountCommand { get; }
    public ICommand ProcessPaymentCommand { get; }
    public ICommand ClearCartCommand { get; }
    public ICommand SelectPaymentMethodCommand { get; }
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
                ClientSearchText = string.Empty;
            }
            else
            {
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

    private void AddToCart(ProductModel product)
    {
        var existingItem = CartItems.FirstOrDefault(item => item.ProductName == product.Name);
        if (existingItem != null)
        {
            if (existingItem.Quantity < product.Stock)
            {
                existingItem.Quantity++;
                existingItem.TotalAmount = existingItem.TotalPrice;
                existingItem.TotalAmountText = existingItem.TotalAmount.ToString("F0");
                OnPropertyChanged(nameof(SubTotal));
                OnPropertyChanged(nameof(Tax));
                OnPropertyChanged(nameof(Total));
            }
        }
        else
        {
            var newItem = new SaleItemModel
            {
                ProductName = product.Name,
                UnitPrice = product.SellPrice,
                Quantity = 1,
                Stock = product.Stock,
                TotalAmount = product.SellPrice,
                TotalAmountText = product.SellPrice.ToString("F0")
            };
            CartItems.Add(newItem);
            OnPropertyChanged(nameof(SubTotal));
            OnPropertyChanged(nameof(Tax));
            OnPropertyChanged(nameof(Total));
        }

        UpdateProductCartStatus();
    }

    private void RemoveFromCart(SaleItemModel item)
    {
        CartItems.Remove(item);
        OnPropertyChanged(nameof(SubTotal));
        OnPropertyChanged(nameof(Tax));
        OnPropertyChanged(nameof(Total));
        UpdateProductCartStatus();
    }

    private void IncreaseQuantity(SaleItemModel item)
    {
        if (item.Quantity < item.Stock)
        {
            item.Quantity++;
            item.TotalAmount = item.TotalPrice;
            item.TotalAmountText = item.TotalAmount.ToString("F0");
            OnPropertyChanged(nameof(SubTotal));
            OnPropertyChanged(nameof(Tax));
            OnPropertyChanged(nameof(Total));
        }
    }

    private void DecreaseQuantity(SaleItemModel item)
    {
        if (item.Quantity > 1)
        {
            item.Quantity--;
            item.TotalAmount = item.TotalPrice;
            item.TotalAmountText = item.TotalAmount.ToString("F0");
            OnPropertyChanged(nameof(SubTotal));
            OnPropertyChanged(nameof(Tax));
            OnPropertyChanged(nameof(Total));
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
    }

    private void SelectPaymentMethod(string method)
    {
        if (Enum.TryParse<PaymentMethod>(method, out var paymentMethod))
        {
            SelectedPaymentMethod = paymentMethod;
        }
    }

    private async Task ProcessPayment()
    {
        // ✅ VALIDACIÓN 1: Verificar que haya productos en el carrito
        if (!CartItems.Any())
        {
            await Application.Current.MainPage.DisplayAlert(
                "Carrito Vacío",
                "⚠️ Debe agregar al menos un producto al carrito antes de facturar.",
                "OK");
            return;
        }

        // ✅ VALIDACIÓN 2: Verificar que se haya seleccionado un cliente
        if (SelectedClient == null || SelectedClient.Id <= 0)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Cliente Requerido",
                "⚠️ Debe seleccionar un cliente antes de facturar.\n\n" +
                "Use el buscador para encontrar el cliente por su número de documento.",
                "OK");
            return;
        }

        IsLoading = true;
        try
        {
            bool isValidWhatsAppNumber = await ValidateWhatsAppNumberAsync();

            if (isValidWhatsAppNumber)
            {
                var sale = new SaleModel
                {
                    Items = CartItems.ToList(),
                    Tax = Tax,
                    PaymentMethod = SelectedPaymentMethod,
                    Status = SaleStatus.Pending,
                    Date = DateTime.Now,
                    SaleId = GenerateSaleId() // ✨ NUEVO: Generar ID único para la venta
                };

                // ✨ NUEVO: Procesar la venta localmente primero
                var success = await _pointOfSaleService.ProcessSaleAsync(sale, ClientWhatsAppNumber);

                if (!success)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        "❌ No se pudo procesar la venta.\n\n" +
                        "Verifique el stock disponible de los productos.",
                        "OK");
                    return;
                }


                var billingResult = await _billingService.GenerateElectronicInvoiceAsync(
                    sale,
                    SelectedClient,
                    ClientWhatsAppNumber
                );

                if (billingResult.Success)
                {
                    string clientInfo = $"\nCliente: {SelectedClient.Name}";
                    string whatsappInfo = !string.IsNullOrWhiteSpace(ClientWhatsAppNumber)
                         ? $"\nWhatsApp: {ClientWhatsAppNumber}"
                       : "";


                    string cudeInfo = !string.IsNullOrWhiteSpace(billingResult.InvoiceHash)
                            ? $"\n🔐 CUDE: {billingResult.InvoiceHash.Substring(0, Math.Min(16, billingResult.InvoiceHash.Length))}..."
                            : "";

                    await Application.Current.MainPage.DisplayAlert(
                       "✅ Factura Electrónica Generada",
                          $"Venta procesada y factura electrónica generada correctamente{clientInfo}{whatsappInfo}\n\n" +
                           $"📄 Número de Factura: FE-{billingResult.InvoiceNumber}\n" +
                    $"💰 Total: ${Total:N2}\n" +
                    $"📦 Productos: {CartItems.Count}{cudeInfo}\n\n" +
                      $"✅ La factura ha sido enviada al correo electrónico del cliente.\n" +
                   $"📱 Puede consultar el PDF desde el Historial de Facturas.",
                       "OK");


                    if (!string.IsNullOrWhiteSpace(ClientWhatsAppNumber))
                    {
                        CourtModel court = GetCourt();
                        await _whatsAppMessageService.SendMessageAsync(ClientWhatsAppNumber, court);
                    }

                    ClearCart();
                    LoadProducts();
                }
                else
                {

                    await Application.Current.MainPage.DisplayAlert(
                        "⚠️ Venta Procesada - Error en Factura Electrónica",
                        $"La venta se procesó correctamente, pero hubo un problema al generar la factura electrónica:\n\n" +
                        $"{billingResult.Message}\n\n" +
                        $"💰 Total: ${Total:N2}\n" +
                        $"📦 Productos: {CartItems.Count}\n\n" +
                        $"Por favor, contacte soporte técnico para generar la factura manualmente.",
                        "OK");

                    ClearCart();
                    LoadProducts();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error procesando pago: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert(
                "Error del Sistema",
                "❌ Ocurrió un error al procesar la venta.\n\n" +
                "Por favor intente nuevamente.",
                "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    //TO-DO REAL DATA
    private static CourtModel GetCourt()
    {
        return new CourtModel()
        {
            IdIslander = 1,
            IdEds = 1,
            Starttime = DateTime.Now.ToString("HH:mm:ss"),
            Endtime = DateTime.Now.AddHours(8).ToString("HH:mm:ss"),
            DateStarttime = DateTime.Today.ToString("yyyy-MM-dd"),
            DateEndtime = DateTime.Today.ToString("yyyy-MM-dd"),
            Descripcion = "Observación de prueba",
            CourtExpenditures = new List<CourtExpenditure>(),
            CourtTypeOfCollections = new List<CourtTypeOfCollection>(),
            CourtDispensers = new List<CourtDispenser>(),
            CourtDocuments = new List<CourtDocument>(),
        };
    }

    private async Task<bool> ValidateWhatsAppNumberAsync()
    {
        bool isValid = true;
        string? number = ClientWhatsAppNumber;

        if (!string.IsNullOrWhiteSpace(number))
        {
            // WhatsApp colombiano sin +57: debe empezar por 3 y tener 10 dígitos
            var regex = new Regex(@"^3\d{9}$");

            if (!regex.IsMatch(number))
            {
                await Application.Current.MainPage.DisplayAlert(
                   "Error del Sistema",
                   "El WhatsApp debe tener 10 dígitos, iniciar con 3 y no incluir + 57.",
                   "OK");
                isValid = false;
            }
        }

        return isValid;
    }

    private int GenerateSaleId()
    {
        return (int)(DateTime.Now.Ticks / TimeSpan.TicksPerSecond);
    }

    private void ClearCart()
    {
        CartItems.Clear();
        CashReceived = 0;
        SelectedClient = null;
        ClientWhatsAppNumber = string.Empty;
        OnPropertyChanged(nameof(SubTotal));
        OnPropertyChanged(nameof(Tax));
        OnPropertyChanged(nameof(Total));
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