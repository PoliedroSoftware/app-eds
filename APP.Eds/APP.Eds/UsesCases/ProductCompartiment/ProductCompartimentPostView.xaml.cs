using APP.Eds.Services.ProductCompartiment;

namespace APP.Eds.UsesCases.ProductCompartiment;

public partial class ProductCompartimentPostView : ContentPage
{
    private ProductCompartimentService _productCompartimentService;

    public ProductCompartimentPostView()
    {
        InitializeComponent();
        _productCompartimentService = new ProductCompartimentService();
        BindingContext = _productCompartimentService;
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        try
        {
            LoadingOverlay.ShowLoading();
            await _productCompartimentService.SaveProductCompartimentDataAsync();
        }

        finally
        {
            LoadingOverlay.HideLoading();

            _productCompartimentService.SelectProduct = null;
            _productCompartimentService.SelectCompartiment = null;
            Stock = 0;
        }
        
    }

    public double Stock
    {
        get => _productCompartimentService.Stock;
        set
        {
            _productCompartimentService.Stock = value;
            OnPropertyChanged();
        }
    }
}
