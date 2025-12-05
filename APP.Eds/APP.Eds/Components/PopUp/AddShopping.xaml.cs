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
            
            // ✅ Verificar si hay productos disponibles y mostrar el estado apropiado
            UpdateEmptyStateVisibility();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing AddShopping popup: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// Actualiza la visibilidad del mensaje de estado vacío según los productos disponibles
    /// </summary>
    private void UpdateEmptyStateVisibility()
    {
        try
        {
            bool hasProducts = shoppingService.FilteredProductCompartimentPairs != null && 
                              shoppingService.FilteredProductCompartimentPairs.Count > 0;
            
            // Mostrar/ocultar el picker y el mensaje de estado vacío
            if (PickerContainer != null)
                PickerContainer.IsVisible = hasProducts;
                
            if (EmptyStateContainer != null)
                EmptyStateContainer.IsVisible = !hasProducts;
            
            // Deshabilitar el botón de agregar si no hay productos
            if (AddButton != null)
                AddButton.IsEnabled = hasProducts;
            
            // Actualizar el mensaje de estado vacío con información del EDS
            if (!hasProducts && EmptyStateMessage != null && shoppingService.SelectedEds != null)
            {
                EmptyStateMessage.Text = $"No hay productos ni compartimentos configurados para el EDS '{shoppingService.SelectedEds.Name}'.\n\n" +
                                        "Por favor, verifique:\n" +
                                        "• Que el EDS tenga tanques asignados\n" +
                                        "• Que los tanques tengan compartimentos\n" +
                                        "• Que los compartimentos tengan productos";
            }
            
            System.Diagnostics.Debug.WriteLine($"Empty state visibility updated - Has products: {hasProducts}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating empty state visibility: {ex.Message}");
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
            if (!vm.Quantity.HasValue || vm.Quantity <= 0)
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
            if (!vm.PurchasePrice.HasValue || vm.PurchasePrice <= 0)
            {
                await CustomAlert.ShowErrorAsync("Debe ingresar un precio de compra válido (mayor que 0)", "Precio de Compra Inválido");
                return;
            }

            // Validate sell price
            if (!vm.SellPrice.HasValue || vm.SellPrice <= 0)
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

            // ✅ GUARDAR LOS VALORES ANTES DE LLAMAR AL MÉTODO PARA EVITAR QUE SE LIMPIEN
            double quantityValue = vm.Quantity.Value;
            double purchasePriceValue = vm.PurchasePrice.Value;
            double sellPriceValue = vm.SellPrice.Value;
            double totalValue = quantityValue * purchasePriceValue;
            string productName = vm.SelectedProductCompartimentPair?.ProductName ?? "Producto seleccionado";

            // ✅ GUARDAR EL STOCK ANTERIOR ANTES DE QUE SE ACTUALICE
            double stockAnterior = vm.CurrentStock ?? 0;

            // Mostrar información de debug para verificar los valores
            System.Diagnostics.Debug.WriteLine($"Adding product - Quantity: {quantityValue}, PurchasePrice: {purchasePriceValue}, Total: {totalValue}, Stock anterior: {stockAnterior}");

            // ✅ Agregar el producto al carrito y verificar si fue exitoso
            bool productoAgregadoExitosamente = await shoppingService.AddShoppingProductFromPopup();

            // ✅ SOLO MOSTRAR EL MENSAJE DE ÉXITO SI EL PRODUCTO SE AGREGÓ CORRECTAMENTE
            if (productoAgregadoExitosamente)
            {
                // ✅ CALCULAR EL STOCK ACTUALIZADO DESPUÉS DE LA COMPRA
                double stockActualizado = stockAnterior + quantityValue;

                // ✅ MOSTRAR EL MENSAJE DE CONFIRMACIÓN CON EL FORMATO SOLICITADO
                await CustomAlert.ShowSuccessAsync(
                    $"Producto agregado exitosamente a la compra\n\n" +
                    $"Producto: {productName}\n" +
                    $"Stock anterior: {stockAnterior:N3} gal\n" +
                    $"Cantidad comprada: {quantityValue:N3} gal\n" +
                    $"Stock actualizado: {stockActualizado:N3} gal\n" +
                    $"Valor total: ${totalValue:N0}",
                    "Producto Agregado");

                // ✅ AHORA SÍ LIMPIAR EL FORMULARIO DESPUÉS DE MOSTRAR EL MENSAJE
                vm.ResetProductForm();

                // Clear form UI elements
                if (ProductCompartimentPicker != null)
                    ProductCompartimentPicker.SelectedItem = null;

                if (FirstEntry != null)
                {
                    FirstEntry.Text = string.Empty;
                    FirstEntry.IsEnabled = true; // Re-enable for next entry
                }

                if (SecondEntry != null)
                {
                    SecondEntry.Text = string.Empty;
                    SecondEntry.IsEnabled = true; // Re-enable for next entry
                }

                if (ThirdEntry != null)
                {
                    ThirdEntry.Text = string.Empty;
                    ThirdEntry.IsEnabled = true; // Re-enable for next entry
                }

                try
                {
                    Close();
                }
                catch (ObjectDisposedException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"AddShopping popup was disposed after operation: {ex.Message}");
                }
            }
            else
            {
                // ❌ Si el producto NO se agregó (por capacidad excedida u otro error),
                // NO mostrar el mensaje de éxito y NO cerrar el popup
                // El usuario verá solo el mensaje de error que ya mostró AddShoppingProductFromPopup()
                System.Diagnostics.Debug.WriteLine("Producto NO agregado debido a validación fallida");
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
                button.Text = "🛒 Agregar a la Compra";
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