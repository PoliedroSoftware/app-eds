using APP.Eds.Services.Shopping;
using APP.Eds.Components.PopUp;
using CommunityToolkit.Maui.Views;

namespace APP.Eds.Components.PopUp;

public partial class AddShopping : Popup
{
    private readonly ShoppingService shoppingService;

    public AddShopping(ShoppingService shoppingService)
    {
        try
        {
            InitializeComponent();
            this.shoppingService = shoppingService;
            BindingContext = shoppingService;
            
            // Ensure popup is properly sized for different screen sizes
            ConfigurePopupForDevice();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing AddShopping popup: {ex.Message}");
            throw;
        }
    }

    private void ConfigurePopupForDevice()
    {
        try
        {
            // Get screen dimensions
            var mainDisplayInfo = DeviceDisplay.Current.MainDisplayInfo;
            var screenWidth = mainDisplayInfo.Width / mainDisplayInfo.Density;
            var screenHeight = mainDisplayInfo.Height / mainDisplayInfo.Density;
            
            // Adjust popup size for smaller screens
            if (screenWidth < 400)
            {
                // For smaller screens, use percentage-based sizing
                var border = this.Content as Border;
                if (border != null)
                {
                    border.WidthRequest = screenWidth * 0.9; // 90% of screen width
                    border.HeightRequest = Math.Min(650, screenHeight * 0.8); // Max 80% of screen height
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"AddShopping popup configured for screen: {screenWidth}x{screenHeight}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error configuring popup for device: {ex.Message}");
        }
    }

    private void OnCloseTapped(object sender, EventArgs e)
    {
        try
        {
            Close();
        }
        catch (ObjectDisposedException ex)
        {
            System.Diagnostics.Debug.WriteLine($"AddShopping popup was already disposed during close: {ex.Message}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error closing AddShopping popup: {ex.Message}");
        }
    }

    private ShoppingService GetShoppingService()
    {
        return shoppingService;
    }

    private async void Add_Product(object sender, EventArgs e)
    {
        try
        {
            // Disable button to prevent multiple submissions
            if (sender is Button button)
            {
                button.IsEnabled = false;
                button.Text = "Agregando...";
            }

            if (BindingContext is not ShoppingService vm)
            {
                await CustomAlert.ShowErrorAsync("Error interno del sistema. Por favor, intente nuevamente", "Error de Contexto");
                return;
            }

            // Enhanced validation with professional alerts
            if (vm.SelectedProductCompartimentPair is null)
            {
                await CustomAlert.ShowErrorAsync("Debe seleccionar un producto y compartimento para continuar", "Producto y Compartimento Requeridos");
                return;
            }

            // Validate quantity
            if (vm.Quantity <= 0)
            {
                await CustomAlert.ShowErrorAsync("Debe ingresar una cantidad válida de galones (mayor que 0)", "Cantidad Inválida");
                return;
            }

            if (vm.Quantity > 50000) // Reasonable limit
            {
                await CustomAlert.ShowErrorAsync("La cantidad parece excesiva. Verifique el valor ingresado", "Cantidad Excesiva");
                return;
            }

            // Validate purchase price
            if (vm.PurchasePrice <= 0)
            {
                await CustomAlert.ShowErrorAsync("Debe ingresar un precio de compra válido (mayor que 0)", "Precio de Compra Inválido");
                return;
            }

            // Validate sell price
            if (vm.SellPrice <= 0)
            {
                await CustomAlert.ShowErrorAsync("Debe ingresar un precio de venta válido (mayor que 0)", "Precio de Venta Inválido");
                return;
            }

            // Validate that sell price is greater than purchase price with professional warning
            if (vm.SellPrice <= vm.PurchasePrice)
            {
                bool confirm = await CustomAlert.ShowConfirmAsync(
                    $"El precio de venta (${vm.SellPrice:F2}) es menor o igual al precio de compra (${vm.PurchasePrice:F2}).\n\nEsto resultará en pérdidas. ¿Desea continuar de todas formas?", 
                    "Advertencia de Rentabilidad", 
                    "Continuar", 
                    "Revisar Precios");
                if (!confirm) return;
            }

            await shoppingService.AddShoppingProductFromPopup();

            // Clear form
            if (ProductCompartimentPicker != null)
                ProductCompartimentPicker.SelectedItem = null;
            
            if (FirstEntry != null)
                FirstEntry.IsEnabled = false;
            
            if (SecondEntry != null)
                SecondEntry.IsEnabled = false;
            
            if (ThirdEntry != null)
                ThirdEntry.IsEnabled = false;

            // Show professional success feedback
            await CustomAlert.ShowSuccessAsync($"Producto agregado exitosamente a la compra\n\nCantidad: {vm.Quantity:F2} galones\nValor total: ${(vm.Quantity * vm.PurchasePrice):F2}", "Producto Agregado");

            try
            {
                Close();
            }
            catch (ObjectDisposedException ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddShopping popup was disposed after operation: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in Add_Product: {ex.Message}");
            await CustomAlert.ShowErrorAsync($"Error al agregar el producto: {ex.Message}", "Error del Sistema");
        }
        finally
        {
            // Re-enable button
            if (sender is Button button)
            {
                button.IsEnabled = true;
                button.Text = "?? Agregar a la Compra";
            }
        }
    }

    private void ProductCompartimentPickerSelected(object sender, EventArgs e)
    {
        try
        {
            if (ProductCompartimentPicker?.SelectedIndex != -1 && FirstEntry != null)
            {
                FirstEntry.Focus();
                FirstEntry.CursorPosition = FirstEntry.Text?.Length ?? 0;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in ProductCompartimentPickerSelected: {ex.Message}");
        }
    }

    private void EntryPurchasePriceCompleted(object sender, EventArgs e)
    {
        try
        {
            if (BindingContext is ShoppingService vm && SecondEntry != null)
            {
                SecondEntry.Focus();
                SecondEntry.CursorPosition = SecondEntry.Text?.Length ?? 0;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in EntryPurchasePriceCompleted: {ex.Message}");
        }
    }

    private void EntrySellPriceCompleted(object sender, EventArgs e)
    {
        try
        {
            if (BindingContext is ShoppingService vm && ThirdEntry != null)
            {
                ThirdEntry.Focus();
                ThirdEntry.CursorPosition = ThirdEntry.Text?.Length ?? 0;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in EntrySellPriceCompleted: {ex.Message}");
        }
    }
}