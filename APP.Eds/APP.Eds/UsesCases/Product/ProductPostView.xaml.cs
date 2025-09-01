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

            if (_productService.Price <= 0)
            {
                await CustomAlert.ShowErrorAsync("Por favor ingrese un precio v�lido (mayor que 0)", "Precio Inv�lido");
                return;
            }

            this.LoadingOverlay.ShowLoading();
            await _productService.SaveProductDataAsync();
            wasSuccessful = true;

            // Clear form fields only if successful
            Name = string.Empty;
            _productService.SelectProductType = null;
            Price = 0;
            
            await CustomAlert.ShowSuccessAsync($"El producto '{_productService.Name}' ha sido registrado exitosamente con un precio de ${_productService.Price:F2}", "Producto Registrado");
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Ocurri� un error inesperado al registrar el producto:\n\n{ex.Message}", "Error del Sistema");
        }
        finally
        {
            this.LoadingOverlay.HideLoading();

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
            await CustomAlert.ShowErrorAsync($"No se pudo abrir la p�gina de tipos de producto:\n\n{ex.Message}", "Error de Navegaci�n");
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
            await CustomAlert.ShowInfoAsync($"La funci�n de edici�n ser� implementada pr�ximamente para el producto: {product.Name}", "Funci�n en Desarrollo");
        }
    }

    private async void OnDeleteProduct(object obj)
    {
        // This method is kept for future use when ProductList with ProductModelResponse is implemented
        if (obj is ProductModelResponse product)
        {
            bool confirm = await CustomAlert.ShowConfirmAsync(
                $"�Est� seguro de que desea eliminar el producto '{product.Name}'?\n\nEsta acci�n no se puede deshacer.", 
                "Confirmar Eliminaci�n", 
                "Eliminar", 
                "Cancelar");

            if (confirm)
            {
                try
                {
                    this.LoadingOverlay.ShowLoading();
                    bool deleted = await _productService.DeleteProductAsync(product.IdProduct);
                    if (deleted)
                    {
                        await CustomAlert.ShowSuccessAsync($"El producto '{product.Name}' ha sido eliminado correctamente", "Producto Eliminado");
                    }
                }
                catch (Exception ex)
                {
                    await CustomAlert.ShowErrorAsync($"Error al eliminar el producto:\n\n{ex.Message}", "Error de Eliminaci�n");
                }
                finally
                {
                    this.LoadingOverlay.HideLoading();
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
    
    public double Price
    {
        get => _productService.Price;
        set
        {
            _productService.Price = value;
            OnPropertyChanged();
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}