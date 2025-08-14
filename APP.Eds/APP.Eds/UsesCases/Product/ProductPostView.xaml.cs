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
        EditProductCommand = new Command<object>(OnEditProduct);
        //DeleteProductCommand = new Command<object>(OnDeleteProduct);
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        try
        {
            LoadingOverlay.ShowLoading();
            await _productTypeService.SaveProductDataAsync();
        }
        finally
        {
            LoadingOverlay.HideLoading();

            Name = string.Empty;
            _productTypeService.SelectProductType = null;
            Price = 0;
        }
        
    }

    private void OnAddProductTypeClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new ProductTypePostView());
    }

    private async void OnEditProduct(object obj)
    {
        if (obj is ProductModel product)
        {
            Name = product.Name;
            Price = product.Price;
            // Si tienes más campos, cárgalos aquí
        }
    }

    //private async void OnDeleteProduct(object obj)
    //{
    //    if (obj is ProductModel product)
    //    {
    //        bool confirm = await DisplayAlert("Confirmar", $"¿Desea eliminar el producto {product.Name}?", "Sí", "No");
    //        if (confirm)
    //        {
    //            await _productTypeService.DeleteProductAsync(product.IdProduct);
    //        }
    //    }
    //}

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