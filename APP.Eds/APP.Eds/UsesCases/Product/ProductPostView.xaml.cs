using APP.Eds.Services.Product;


namespace APP.Eds.UsesCases.Product;

public partial class ProductPostView : ContentPage
{
    private ProductService _productTypeService;

    public ProductPostView()
	{
		InitializeComponent();
        _productTypeService = new ProductService();
        BindingContext = _productTypeService;
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