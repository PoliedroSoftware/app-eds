using APP.Eds.Models.Product;
using APP.Eds.Services.Product;
using APP.Eds.UsesCases.ProductType;
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
                button.Text = "Guardando...";
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(_productService.Name))
            {
                await DisplayAlert("Error", "Por favor ingrese el nombre del producto", "OK");
                return;
            }

            if (_productService.SelectProductType == null)
            {
                await DisplayAlert("Error", "Por favor seleccione un tipo de producto", "OK");
                return;
            }

            if (_productService.Price <= 0)
            {
                await DisplayAlert("Error", "Por favor ingrese un precio válido (mayor que 0)", "OK");
                return;
            }

            LoadingOverlay.ShowLoading();
            await _productService.SaveProductDataAsync();
            wasSuccessful = true;

            // Clear form fields only if successful
            Name = string.Empty;
            _productService.SelectProductType = null;
            Price = 0;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error inesperado: {ex.Message}", "OK");
        }
        finally
        {
            LoadingOverlay.HideLoading();

            // Re-enable and restore button
            if (button != null)
            {
                button.IsEnabled = true;
                button.Text = "?? Registrar Producto";
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
            await DisplayAlert("Error", $"Error al navegar: {ex.Message}", "OK");
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
        }
    }

    private async void OnEditProduct(object obj)
    {
        // This method is kept for future use when ProductList with ProductModelResponse is implemented
        if (obj is ProductModelResponse product)
        {
            Name = product.Name;
            // Note: ProductModelResponse doesn't have Price, so this would need to be fetched
            await DisplayAlert("Editar", $"Función de edición será implementada para: {product.Name}", "OK");
        }
    }

    private async void OnDeleteProduct(object obj)
    {
        // This method is kept for future use when ProductList with ProductModelResponse is implemented
        if (obj is ProductModelResponse product)
        {
            bool confirm = await DisplayAlert("Confirmar", 
                $"¿Desea eliminar el producto '{product.Name}'?", "Sí", "No");
            if (confirm)
            {
                try
                {
                    LoadingOverlay.ShowLoading();
                    bool deleted = await _productService.DeleteProductAsync(product.IdProduct);
                    if (deleted)
                    {
                        await DisplayAlert("Éxito", "Producto eliminado correctamente", "OK");
                    }
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