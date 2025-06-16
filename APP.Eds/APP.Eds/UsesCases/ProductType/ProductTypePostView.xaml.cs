using APP.Eds.Services.ProductType;

namespace APP.Eds.UsesCases.ProductType;

public partial class ProductTypePostView : ContentPage
{
    private ProductTypeService _productTypeService;
    public ProductTypePostView()
	{
		InitializeComponent();
        _productTypeService = new ProductTypeService();
        BindingContext = _productTypeService;
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        try
        {
            LoadingOverlay.ShowLoading();
            await _productTypeService.SaveProductTypeDataAsync();
        }
        finally
        {
            LoadingOverlay.HideLoading();

            Description = string.Empty;
        }
        
    }

    public string Description
    {
        get => _productTypeService.Description;
        set
        {
            _productTypeService.Description = value;
            OnPropertyChanged();
        }
    }
}