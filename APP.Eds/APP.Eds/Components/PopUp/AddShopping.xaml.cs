using APP.Eds.Services.Shopping;
using APP.Eds.Components.PopUp;
using CommunityToolkit.Maui.Views;

namespace APP.Eds.Components.PopUp;

public partial class AddShopping : Popup
{
    private readonly ShoppingService shoppingService;

    public AddShopping(ShoppingService shoppingService)
	{
		InitializeComponent();
        this.shoppingService = shoppingService;
        BindingContext = shoppingService;
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
            if (vm.Price <= 0)
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
            if (vm.SellPrice <= vm.Price)
            {
                bool confirm = await CustomAlert.ShowConfirmAsync(
                    $"El precio de venta (${vm.SellPrice:F2}) es menor o igual al precio de compra (${vm.Price:F2}).\n\nEsto resultará en pérdidas. ¿Desea continuar de todas formas?", 
                    "Advertencia de Rentabilidad", 
                    "Continuar", 
                    "Revisar Precios");
                if (!confirm) return;
            }

            await shoppingService.AddShoppingProductFromPopup();

            // Clear form
            ProductCompartimentPicker.SelectedItem = null;
            FirstEntry.IsEnabled = false;
            SecondEntry.IsEnabled = false;
            ThirdEntry.IsEnabled = false;

            // Show professional success feedback
            await CustomAlert.ShowSuccessAsync($"Producto agregado exitosamente a la compra\n\nCantidad: {vm.Quantity:F2} galones\nValor total: ${(vm.Quantity * vm.Price):F2}", "Producto Agregado");

            try
            {
                Close();
            }
            catch (ObjectDisposedException ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddShopping popup was disposed after operation: {ex.Message}");
            }
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

    private void EntryPriceCompleted(object sender, EventArgs e)
    {
        if (BindingContext is ShoppingService vm)
        {
            SecondEntry.Focus();
            SecondEntry.CursorPosition = SecondEntry.Text?.Length ?? 0;
        }
    }

    private void EntryQuantityCompleted(object sender, EventArgs e)
    {
        if (BindingContext is ShoppingService vm)
        {
            ThirdEntry.Focus();
            ThirdEntry.CursorPosition = ThirdEntry.Text?.Length ?? 0;
        }
    }

    private void ProductCompartimentPickerSelected(object sender, EventArgs e)
    {
        if (ProductCompartimentPicker.SelectedIndex != -1)
        {
            FirstEntry.Focus();
            FirstEntry.CursorPosition = FirstEntry.Text?.Length ?? 0;
        }
    }
}