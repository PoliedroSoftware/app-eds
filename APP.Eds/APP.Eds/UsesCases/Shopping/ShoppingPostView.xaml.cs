using APP.Eds.Components.PopUp;
using APP.Eds.Services.Shopping;
using CommunityToolkit.Maui.Views;

namespace APP.Eds.UsesCases.Shopping;

public partial class ShoppingPostView : ContentPage
{
    private ShoppingService _shoppingService;
    
    public ShoppingPostView()
    {
        InitializeComponent();
        _shoppingService = ShoppingService.Instance;
        BindingContext = _shoppingService;
    }

    private void OpenShoppingPopUp(object sender, EventArgs e)
    {
        _shoppingService.ResetProductForm(); 
        this.ShowPopup(new AddShopping(_shoppingService));
    }
    
    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        if (BindingContext is not ShoppingService vm)
            return; 
            
        try
        {
            // Disable button to prevent multiple submissions
            if (sender is Button button)
            {
                button.IsEnabled = false;
            }

            // Enhanced validation with professional alerts
            if (string.IsNullOrWhiteSpace(vm.Invoice))
            {
                await CustomAlert.ShowErrorAsync("El número de factura es obligatorio para registrar la compra", "Factura Requerida");
                return;
            }

            if (vm.Invoice.Length < 3)
            {
                await CustomAlert.ShowErrorAsync("El número de factura debe tener al menos 3 caracteres", "Factura Inválida");
                return;
            }

            if (vm.SelectedProvider is null)
            {
                await CustomAlert.ShowErrorAsync("Debe seleccionar el proveedor que realiza la venta", "Proveedor Requerido");
                return;
            }

            if (vm.SelectedCategory is null)
            {
                await CustomAlert.ShowErrorAsync("Debe seleccionar la categoría de combustible correspondiente", "Categoría Requerida");
                return;
            }

            if (vm.ShoppingProduct?.Count == 0)
            {
                await CustomAlert.ShowErrorAsync("Debe agregar al menos un producto a la compra antes de guardar", "Productos Requeridos");
                return;
            }

            // Validate total amount
            double totalAmount = vm.ShoppingProduct?.Sum(p => p.Quantity * p.PurchasePrice) ?? 0;
            if (totalAmount <= 0)
            {
                await CustomAlert.ShowErrorAsync("El monto total de la compra debe ser mayor que cero", "Monto Inválido");
                return;
            }

            LoadingOverlay.ShowLoading();
            await vm.SaveShoppingDataAsync();
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error al guardar la compra:\n\n{ex.Message}", "Error del Sistema");
        }
        finally
        {
            LoadingOverlay.HideLoading();

            // Clear form fields after successful submission
            Invoice = string.Empty;
            Date = DateTime.Now;
            Amount = 0;
            _shoppingService.SelectedProvider = null;
            _shoppingService.SelectedCategory = null;

            // Re-enable button
            if (sender is Button button)
            {
                button.IsEnabled = true;
            }
        }
    }

    private void InvoiceEntryCompleted(object sender, EventArgs e)
    {
        ProviderPicker.Focus();
    }

    private void ProviderSelected(object sender, EventArgs e)
    {
        if (ProviderPicker.SelectedIndex != -1)
        {
            CategoryPicker.Focus();
        }
    }

    public string Invoice
    {
        get => _shoppingService.Invoice;
        set
        {
            _shoppingService.Invoice = value;
            OnPropertyChanged();
        }
    }

    public DateTime Date
    {
        get => _shoppingService.Date;
        set
        {
            _shoppingService.Date = value;
            OnPropertyChanged();
        }
    }

    public double? Amount
    {
        get => _shoppingService.Amount;
        set
        {
            _shoppingService.Amount = value;
            OnPropertyChanged();
        }
    }
}