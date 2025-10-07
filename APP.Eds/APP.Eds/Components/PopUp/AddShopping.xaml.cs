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
            // Bloquear botón para evitar dobles taps
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

            // Validaciones básicas de UI (sin mostrar modal verde aún)
            if (vm.SelectedProductCompartimentPair is null)
            {
                await CustomAlert.ShowErrorAsync("Debe seleccionar un producto y compartimento para continuar", "Producto y Compartimento Requeridos");
                return;
            }

            if (!vm.Quantity.HasValue || vm.Quantity <= 0)
            {
                await CustomAlert.ShowErrorAsync("Debe ingresar una cantidad válida de galones (mayor que 0)", "Cantidad Inválida");
                return;
            }

            if (!vm.PurchasePrice.HasValue || vm.PurchasePrice <= 0)
            {
                await CustomAlert.ShowErrorAsync("Debe ingresar un precio de compra válido (mayor que 0)", "Precio de Compra Inválido");
                return;
            }

            if (!vm.SellPrice.HasValue || vm.SellPrice <= 0)
            {
                await CustomAlert.ShowErrorAsync("Debe ingresar un precio de venta válido (mayor que 0)", "Precio de Venta Inválido");
                return;
            }

            // Guardar valores para la ficha de éxito (solo si se agrega)
            double quantityValue = vm.Quantity.Value;
            double purchasePriceValue = vm.PurchasePrice.Value;
            double sellPriceValue = vm.SellPrice.Value;
            double totalValue = quantityValue * purchasePriceValue;
            string productName = vm.SelectedProductCompartimentPair?.ProductName ?? "Producto";
            double stockAnterior = vm.CurrentStock ?? 0;

            // ⛔ Llamamos al servicio: si devuelve false, NO mostramos el modal verde
            bool agregado = await shoppingService.AddShoppingProductFromPopup();
            if (!agregado)
            {
                // Se mostró el alert de “Precio inválido” dentro del servicio.
                // Solo salimos sin mostrar el modal verde.
                return;
            }

            // ✅ Solo si se agregó, mostramos el modal verde
            double stockActualizado = stockAnterior + quantityValue; // ajusta si tu stock debe sumar/restar

            await CustomAlert.ShowSuccessAsync(
                $"Producto agregado exitosamente a la compra\n\n" +
                $"Producto: {productName}\n" +
                $"Stock anterior: {stockAnterior:N3} gal\n" +
                $"Cantidad comprada: {quantityValue:N3} gal\n" +
                $"Stock actualizado: {stockActualizado:N3} gal\n" +
                $"Valor total: ${totalValue:N0}",
                "Producto Agregado");

            // Limpiar formulario
            vm.ResetProductForm();
            if (ProductCompartimentPicker != null) ProductCompartimentPicker.SelectedItem = null;
            if (FirstEntry != null) { FirstEntry.Text = string.Empty; FirstEntry.IsEnabled = true; }
            if (SecondEntry != null) { SecondEntry.Text = string.Empty; SecondEntry.IsEnabled = true; }
            if (ThirdEntry != null) { ThirdEntry.Text = string.Empty; ThirdEntry.IsEnabled = true; }

            // Cerrar popup
            try { Close(); } catch { /* ignorar si ya está disposed */ }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in Add_Product: {ex.Message}");
            await CustomAlert.ShowErrorAsync($"Error al agregar el producto: {ex.Message}", "Error del Sistema");
        }
        finally
        {
            // Rehabilitar botón
            if (sender is Button button)
            {
                button.IsEnabled = true;
                button.Text = "🚀 Agregar a la Compra";
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