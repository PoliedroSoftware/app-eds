using APP.Eds.Components.PopUp;
using APP.Eds.Services.Shopping;
using APP.Eds.Constants;
using CommunityToolkit.Maui.Views;

namespace APP.Eds.UsesCases.Shopping;

public partial class ShoppingPostView : ContentPage
{
    private ShoppingService _shoppingService;
    private bool _isInvoiceDuplicate = false;

    public ShoppingPostView()
    {
        InitializeComponent();
        _shoppingService = ShoppingService.Instance;
        BindingContext = _shoppingService;
    }

    private async void OpenShoppingPopUp(object sender, EventArgs e)
    {
        Button button = sender as Button;
        
        try
        {
            // Disable the button temporarily to prevent double-tap
            if (button != null)
            {
                button.IsEnabled = false;
            }

            // ✅ VALIDACIÓN: Verificar que se haya seleccionado un EDS antes de abrir el popup
            if (_shoppingService.SelectedEds == null)
            {
                await CustomAlert.ShowErrorAsync(
                    ShoppingValidationMessages.EdsRequired,
                    ShoppingValidationMessages.EdsRequiredTitle);
                return;
            }

            // ✅ VALIDACIÓN: Verificar que existan productos/compartimentos para el EDS seleccionado
            // Capture EDS name before check to avoid potential race condition
            string edsName = _shoppingService.SelectedEds?.Name ?? "la EDS seleccionada";
            if (_shoppingService.FilteredProductCompartimentPairs == null || 
                _shoppingService.FilteredProductCompartimentPairs.Count == 0)
            {
                await CustomAlert.ShowWarningAsync(
                    ShoppingValidationMessages.GetNoProductsMessage(edsName),
                    ShoppingValidationMessages.NoProductsTitle);
                return;
            }

            _shoppingService.ResetProductForm();

            // Add a small delay to ensure UI thread is ready (especially important on physical devices)
            await Task.Delay(50);

            // Create the popup with error handling
            var popup = new AddShopping(_shoppingService);

            // Use async ShowPopupAsync for better compatibility with physical devices
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    await this.ShowPopupAsync(popup);
                }
                catch (InvalidOperationException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"InvalidOperationException showing popup: {ex.Message}");
                    // Try alternative approach for popup display
                    await ShowPopupAlternative(popup);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error showing popup with ShowPopupAsync: {ex.Message}");
                    // Fallback to synchronous method as last resort
                    try
                    {
                        this.ShowPopup(popup);
                    }
                    catch (Exception fallbackEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Fallback popup method also failed: {fallbackEx.Message}");
                        await CustomAlert.ShowErrorAsync("No se pudo abrir el formulario de agregar producto. Intente nuevamente.", "Error de Interfaz");
                    }
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OpenShoppingPopUp: {ex.Message}");
            await CustomAlert.ShowErrorAsync("No se pudo abrir el formulario de agregar producto", "Error de Interfaz");
        }
        finally
        {
            // Re-enable the button
            if (button != null)
            {
                // Add small delay before re-enabling to prevent rapid successive taps
                await Task.Delay(500);
                button.IsEnabled = true;
            }
        }
    }

    private async Task ShowPopupAlternative(Popup popup)
    {
        try
        {
            // Alternative approach: ensure we're on UI thread and try with additional delay
            await Task.Delay(100);

            if (MainThread.IsMainThread)
            {
                await this.ShowPopupAsync(popup);
            }
            else
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await this.ShowPopupAsync(popup);
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Alternative popup display method failed: {ex.Message}");
            throw;
        }
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
                await CustomAlert.ShowErrorAsync("Por favor ingrese un número de factura válido (mínimo 3 caracteres)", "Factura Inválida");
                return;
            }

            // Check if we already detected a duplicate during inline validation
            if (_isInvoiceDuplicate)
            {
                await CustomAlert.ShowErrorAsync(
                    $"❌ No se puede proceder con el guardado.\n\n" +
                    $"El número de factura '{vm.Invoice}' ya existe en la base de datos.\n\n" +
                    $"Debe modificar el número de factura para continuar.",
                    "Factura Duplicada - Operación Bloqueada");

                // Focus back to invoice field
                if (FirstEntry != null)
                {
                    await Task.Delay(100);
                    FirstEntry.Focus();
                }
                return;
            }

            // STRICT validation: Check if invoice exists BEFORE proceeding (as backup)
            bool invoiceExists = await vm.CheckInvoiceExistsAsync(vm.Invoice);
            if (invoiceExists)
            {
                _isInvoiceDuplicate = true;
                await CustomAlert.ShowErrorAsync(
                    $"❌ No se puede proceder con el guardado.\n\n" +
                    $"El número de factura '{vm.Invoice}' ya existe en la base de datos.\n\n" +
                    $"Debe modificar el número de factura para continuar.",
                    "Factura Duplicada - Operación Bloqueada");

                // Focus back to invoice field
                if (FirstEntry != null)
                {
                    await Task.Delay(100);
                    FirstEntry.Focus();
                }
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

            if (LoadingOverlay != null)
                LoadingOverlay.ShowLoading();

            await vm.SaveShoppingDataAsync();

            // Clear form fields after successful submission
            Invoice = string.Empty;
            Date = DateTime.Now;
            Amount = 0;
            _shoppingService.SelectedProvider = null;
            _shoppingService.SelectedCategory = null;
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error al guardar la compra:\n\n{ex.Message}", "Error del Sistema");
        }
        finally
        {
            if (LoadingOverlay != null)
                LoadingOverlay.HideLoading();

            // Re-enable button
            if (sender is Button button)
            {
                button.IsEnabled = true;
            }
        }
    }

    private async void InvoiceEntryUnfocused(object sender, FocusEventArgs e)
    {
        try
        {
            if (sender is Entry entry && !string.IsNullOrWhiteSpace(entry.Text))
            {
                // Check if invoice meets minimum length requirement first
                if (entry.Text.Length < 3)
                {
                    _isInvoiceDuplicate = false;
                    return; // Don't check for duplicates if length is invalid
                }

                // Check for duplicate invoice
                bool isDuplicate = await _shoppingService.CheckInvoiceExistsAsync(entry.Text);
                _isInvoiceDuplicate = isDuplicate;
                
                if (isDuplicate)
                {
                    // Show inline error for duplicate
                    InvoiceBorder.Stroke = Color.FromArgb("#EF4444"); // Red
                    InvoiceHelpIcon.Text = "❌";
                    InvoiceHelpIcon.TextColor = Color.FromArgb("#EF4444");
                    InvoiceHelpText.Text = "Este número de factura ya existe en el sistema";
                    InvoiceHelpText.TextColor = Color.FromArgb("#EF4444");
                }
                else
                {
                    // Keep the valid state (green)
                    InvoiceBorder.Stroke = Color.FromArgb("#10B981");
                    InvoiceHelpIcon.Text = "✅";
                    InvoiceHelpIcon.TextColor = Color.FromArgb("#10B981");
                    InvoiceHelpText.Text = "Número de factura válido";
                    InvoiceHelpText.TextColor = Color.FromArgb("#10B981");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in InvoiceEntryUnfocused: {ex.Message}");
        }
    }

    private void InvoiceEntryCompleted(object sender, EventArgs e)
    {
        try
        {
            if (ProviderPicker != null)
                ProviderPicker.Focus();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in InvoiceEntryCompleted: {ex.Message}");
        }
    }

    private void InvoiceEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            if (sender is Entry entry)
            {
                string invoiceText = entry.Text ?? string.Empty;
                
                // Reset duplicate flag when user types
                _isInvoiceDuplicate = false;
                
                // Update visual feedback based on invoice length
                if (string.IsNullOrWhiteSpace(invoiceText))
                {
                    // Empty state - show neutral help text
                    InvoiceBorder.Stroke = Color.FromArgb("#DDD6FE");
                    InvoiceHelpIcon.Text = "ℹ️";
                    InvoiceHelpIcon.TextColor = Color.FromArgb("#6B7280");
                    InvoiceHelpText.Text = "El número de factura debe tener al menos 3 caracteres";
                    InvoiceHelpText.TextColor = Color.FromArgb("#6B7280");
                }
                else if (invoiceText.Length < 3)
                {
                    // Invalid state - show warning
                    InvoiceBorder.Stroke = Color.FromArgb("#F59E0B");
                    InvoiceHelpIcon.Text = "⚠️";
                    InvoiceHelpIcon.TextColor = Color.FromArgb("#F59E0B");
                    InvoiceHelpText.Text = $"Faltan {3 - invoiceText.Length} caracteres para completar el mínimo requerido";
                    InvoiceHelpText.TextColor = Color.FromArgb("#F59E0B");
                }
                else
                {
                    // Valid state - show success (will check for duplicates on Unfocused)
                    InvoiceBorder.Stroke = Color.FromArgb("#10B981");
                    InvoiceHelpIcon.Text = "✅";
                    InvoiceHelpIcon.TextColor = Color.FromArgb("#10B981");
                    InvoiceHelpText.Text = "Número de factura válido";
                    InvoiceHelpText.TextColor = Color.FromArgb("#10B981");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in InvoiceEntryTextChanged: {ex.Message}");
        }
    }

    private void ProviderSelected(object sender, EventArgs e)
    {
        try
        {
            if (ProviderPicker?.SelectedIndex != -1 && CategoryPicker != null)
            {
                CategoryPicker.Focus();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in ProviderSelected: {ex.Message}");
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