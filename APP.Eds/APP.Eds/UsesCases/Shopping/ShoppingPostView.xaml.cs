using APP.Eds.Components.PopUp;
using APP.Eds.Services.Shopping;
using CommunityToolkit.Maui.Views;

namespace APP.Eds.UsesCases.Shopping;

public partial class ShoppingPostView : ContentPage
{
    private ShoppingService _shoppingService;
    public ShoppingPostView()
    {
        InitializeComponent();
        _shoppingService = ShoppingService.Instance;
        BindingContext = _shoppingService;
    }

    private void OpenShoppingPopUp(object sender, EventArgs e)
    {
        _shoppingService.ResetProductForm(); 
        this.ShowPopup(new AddShopping(_shoppingService));
    }


    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        if (BindingContext is not ShoppingService vm)
            return; 
        try
        {
            LoadingOverlay.ShowLoading();
            if (vm.SelectedProvider is null)
            {
                await DisplayAlert("Error", "Por favor, seleccione un Proveedor", "OK");
                return;
            }

            if (vm.SelectedCategory is null)
            {
                await DisplayAlert("Error", "Por favor, seleccione una Categor�a", "OK");
                return;
            }
                await vm.SaveShoppingDataAsync();
        }
            
        finally
        {
            LoadingOverlay.HideLoading();

            Invoice = string.Empty;
            Date = DateTime.Now;
            Amount = 0;
            _shoppingService.SelectedProvider = null;
            _shoppingService.SelectedCategory = null;

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

    public double Amount
    {
        get => _shoppingService.Amount;
        set
        {
            _shoppingService.Amount = value;
            OnPropertyChanged();
        }
    }
}