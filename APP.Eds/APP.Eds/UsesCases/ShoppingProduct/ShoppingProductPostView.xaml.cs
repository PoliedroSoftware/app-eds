using System.Diagnostics;
using APP.Eds.Services.ShoppingProduct;

namespace APP.Eds.UsesCases.ShoppingProduct;

public partial class ShoppingProductPostView : ContentPage
{
    private ShoppingProductService _shoppingProductService;
    public ShoppingProductPostView()
	{
		InitializeComponent();
        _shoppingProductService = new ShoppingProductService();
        BindingContext = _shoppingProductService;
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        if (BindingContext is ShoppingProductService vm)
        {
            try
            {
                LoadingOverlay.ShowLoading();
                if (vm.IdShopping <= 0)
                {
                    await DisplayAlert("Error", "Please enter a valid Shopping ID", "OK");
                    return;
                }

                if (vm.IdProduct <= 0)
                {
                    await DisplayAlert("Error", "Please enter a valid Product ID", "OK");
                    return;
                }

                if (vm.Quantity <= 0)
                {
                    await DisplayAlert("Error", "Please enter a valid Quantity (must be greater than 0)", "OK");
                    return;
                }

                if (vm.Price <= 0)
                {
                    await DisplayAlert("Error", "Please enter a valid Price (must be greater than 0)", "OK");
                    return;
                }

                if (vm.IdCompartment <= 0)
                {
                    await DisplayAlert("Error", "Please select a valid Compartment", "OK");
                    return;
                }

                await vm.SaveShoppingProductDataAsync();
            }
            finally
            {
                LoadingOverlay.HideLoading();

                _shoppingProductService.SelectedShopping = null;
                _shoppingProductService.SelectedProduct = null;
                Quantity = 0;
                Price = 0;
                _shoppingProductService.SelectedCompartiment = null;
            }
            
        }
        else
        {
            await DisplayAlert("Error", "Context error", "OK");
        }
    }

    public double Quantity
    {
        get => _shoppingProductService.Quantity;
        set
        {
            _shoppingProductService.Quantity = value;
            OnPropertyChanged();
        }
    }

    public double Price
    {
        get => _shoppingProductService.Price;
        set
        {
            _shoppingProductService.Price = value;
            OnPropertyChanged();
        }
    }

}