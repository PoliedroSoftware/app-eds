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
            // Disable button to prevent multiple submissions
            if (sender is Button button)
            {
                button.IsEnabled = false;
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(vm.Invoice))
            {
                await DisplayAlert("Error", "Por favor ingrese el número de factura", "OK");
                return;
            }

            if (vm.SelectedProvider is null)
            {
                await DisplayAlert("Error", "Por favor seleccione un proveedor", "OK");
                return;
            }

            if (vm.SelectedCategory is null)
            {
                await DisplayAlert("Error", "Por favor seleccione una categoría de combustible", "OK");
                return;
            }

            if (vm.ShoppingProduct?.Count == 0)
            {
                await DisplayAlert("Error", "Por favor agregue al menos un producto a la compra", "OK");
                return;
            }

            LoadingOverlay.ShowLoading();
            await vm.SaveShoppingDataAsync();
        }
        finally
        {
            LoadingOverlay.HideLoading();

            // Clear form fields after successful submission
            Invoice = string.Empty;
            Date = DateTime.Now;
            Amount = 0;
            _shoppingService.SelectedProvider = null;
            _shoppingService.SelectedCategory = null;

            // Re-enable button
            if (sender is Button button)
            {
                button.IsEnabled = true;
            }
        }
    }

    private void InvoiceEntryCompleted(object sender, EventArgs e)
    {
        ProviderPicker.Focus();
    }

    private void ProviderSelected(object sender, EventArgs e)
    {
        if (ProviderPicker.SelectedIndex != -1)
        {
            CategoryPicker.Focus();
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

    public double? Amount
    {
        get => _shoppingService.Amount;
        set
        {
            _shoppingService.Amount = value;
            OnPropertyChanged();
        }
    }
}