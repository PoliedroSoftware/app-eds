using APP.Eds.Models.Product;
using APP.Eds.Services.Product;
using APP.Eds.UsesCases.ProductType;
using APP.Eds.Services.Alert;
using APP.Eds.Components.PopUp;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;


namespace APP.Eds.UsesCases.Product;

public partial class ProductPostView : ContentPage, INotifyPropertyChanged
{
    private ProductService _productService;

    public ICommand EditProductCommand { get; }
    public ICommand DeleteProductCommand { get; }

    public ProductPostView()
	{
		InitializeComponent();
        _productService = new ProductService();
        BindingContext = _productService;
        
        // These commands are kept for future use when ProductList is implemented
        EditProductCommand = new Command<object>(OnEditProduct);
        DeleteProductCommand = new Command<object>(OnDeleteProduct);
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        bool wasSuccessful = false;

        try
        {
            // Disable button to prevent multiple submissions
            if (button != null)
            {
                button.IsEnabled = false;
                button.Text = "Enviando...";
            }

            // Validate required fields using custom alerts
            if (string.IsNullOrWhiteSpace(_productService.Name))
            {
                await CustomAlert.ShowErrorAsync("Por favor ingrese el nombre del producto", "Campo Requerido");
                return;
            }

            if (_productService.SelectProductType == null)
            {
                await CustomAlert.ShowErrorAsync("Por favor seleccione un tipo de producto", "Campo Requerido");
                return;
            }

            // Validate that negative values are not allowed (but 0 is acceptable)
            if (_productService.PurchasePrice < 0)
            {
                await CustomAlert.ShowErrorAsync("El precio de compra no puede ser negativo", "Precio de Compra Inválido");
                return;
            }

            if (_productService.SellPrice < 0)
            {
                await CustomAlert.ShowErrorAsync("El precio de venta no puede ser negativo", "Precio de Venta Inválido");
                return;
            }

            if (_productService.Stock < 0)
            {
                await CustomAlert.ShowErrorAsync("El stock no puede ser negativo", "Stock Inválido");
                return;
            }

            // Optional validation for business logic (only if both prices are greater than 0)
            if (_productService.SellPrice > 0 && _productService.PurchasePrice > 0 && 
                _productService.SellPrice <= _productService.PurchasePrice)
            {
                bool confirm = await CustomAlert.ShowConfirmAsync(
                    $"El precio de venta (${_productService.SellPrice:F2}) es menor o igual al precio de compra (${_productService.PurchasePrice:F2}).\n\n" +
                    $"Esto podría resultar en pérdidas. ¿Desea continuar de todas formas?",
                    "Advertencia de Rentabilidad",
                    "Continuar",
                    "Revisar Precios");
                
                if (!confirm) return;
            }

            LoadingOverlay.ShowLoading();
            await _productService.SaveProductDataAsync();
            wasSuccessful = true;

            // Clear form fields only if successful
            _productService.Name = string.Empty;
            _productService.SelectProductType = null;
            _productService.PurchasePrice = 0;
            _productService.SellPrice = 0;
            _productService.Stock = 0;
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Ocurrió un error inesperado al registrar el producto:\n\n{ex.Message}", "Error del Sistema");
        }
        finally
        {
            LoadingOverlay.HideLoading();

            // Re-enable and restore button
            if (button != null)
            {
                button.IsEnabled = true;
                button.Text = "Enviar Datos";
            }
        }
    }

    private async void OnAddProductTypeClicked(object sender, EventArgs e)
    {
        try
        {
            await Navigation.PushAsync(new ProductTypePostView());
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"No se pudo abrir la página de tipos de producto:\n\n{ex.Message}", "Error de Navegación");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Refresh product types when returning from ProductType creation
        try
        {
            await _productService.RefreshProductTypesAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error refreshing product types: {ex.Message}");
            // Don't show error to user as this is not critical
        }
    }

    private async void OnEditProduct(object obj)
    {
        // This method is kept for future use when ProductList with ProductModelResponse is implemented
        if (obj is ProductModelResponse product)
        {
            Name = product.Name;
            // Note: ProductModelResponse doesn't have Price, so this would need to be fetched
            await CustomAlert.ShowInfoAsync($"La función de edición será implementada próximamente para el producto: {product.Name}", "Función en Desarrollo");
        }
    }

    private async void OnDeleteProduct(object obj)
    {
        // This method is kept for future use when ProductList with ProductModelResponse is implemented
        if (obj is ProductModelResponse product)
        {
            bool confirm = await CustomAlert.ShowConfirmAsync(
                $"¿Está seguro de que desea eliminar el producto '{product.Name}'?\n\nEsta acción no se puede deshacer.", 
                "Confirmar Eliminación", 
                "Eliminar", 
                "Cancelar");

            if (confirm)
            {
                try
                {
                    LoadingOverlay.ShowLoading();
                    bool deleted = await _productService.DeleteProductAsync(product.IdProduct);
                    if (deleted)
                    {
                        await CustomAlert.ShowSuccessAsync($"El producto '{product.Name}' ha sido eliminado correctamente", "Producto Eliminado");
                    }
                }
                catch (Exception ex)
                {
                    await CustomAlert.ShowErrorAsync($"Error al eliminar el producto:\n\n{ex.Message}", "Error de Eliminación");
                }
                finally
                {
                    LoadingOverlay.HideLoading();
                }
            }
        }
    }

    public string Name
    {
        get => _productService.Name;
        set
        {
            _productService.Name = value;
            OnPropertyChanged();
        }
    }

    public double PurchasePrice
    {
        get => _productService.PurchasePrice;
        set
        {
            _productService.PurchasePrice = value;
            OnPropertyChanged();
        }
    }

    public double SellPrice
    {
        get => _productService.SellPrice;
        set
        {
            _productService.SellPrice = value;
            OnPropertyChanged();
        }
    }

    public int Stock
    {
        get => _productService.Stock;
        set
        {
            _productService.Stock = value;
            OnPropertyChanged();
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}