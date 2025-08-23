using APP.Eds.Models.Product;
using APP.Eds.Services.Product;
using APP.Eds.UsesCases.ProductType;
using System.Windows.Input;


namespace APP.Eds.UsesCases.Product;

public partial class ProductPostView : ContentPage
{
    private ProductService _productTypeService;

    public ICommand EditProductCommand { get; }
    public ICommand DeleteProductCommand { get; }

    public ProductPostView()
	{
		InitializeComponent();
        _productTypeService = new ProductService();
        BindingContext = _productTypeService;
        
        // These commands are kept for future use when ProductList is implemented
        EditProductCommand = new Command<object>(OnEditProduct);
        DeleteProductCommand = new Command<object>(OnDeleteProduct);
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        try
        {
            // Disable button to prevent multiple submissions
            if (sender is Button button)
            {
                button.IsEnabled = false;
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(_productTypeService.Name))
            {
                await DisplayAlert("Error", "Por favor ingrese el nombre del producto", "OK");
                return;
            }

            if (_productTypeService.SelectProductType == null)
            {
                await DisplayAlert("Error", "Por favor seleccione un tipo de producto", "OK");
                return;
            }

            if (_productTypeService.Price <= 0)
            {
                await DisplayAlert("Error", "Por favor ingrese un precio válido (mayor que 0)", "OK");
                return;
            }

            LoadingOverlay.ShowLoading();
            await _productTypeService.SaveProductDataAsync();
        }
        finally
        {
            LoadingOverlay.HideLoading();

            // Clear form fields after successful submission
            Name = string.Empty;
            _productTypeService.SelectProductType = null;
            Price = 0;

            // Re-enable button
            if (sender is Button button)
            {
                button.IsEnabled = true;
            }
        }
        
    }

    private async void OnAddProductTypeClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ProductTypePostView());
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
                    bool deleted = await _productTypeService.DeleteProductAsync(product.IdProduct);
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
        get => _productTypeService.Name;
        set
        {
            _productTypeService.Name = value;
            OnPropertyChanged();
        }
    }
    
    public double Price
    {
        get => _productTypeService.Price;
        set
        {
            _productTypeService.Price = value;
            OnPropertyChanged();
        }
    }
}