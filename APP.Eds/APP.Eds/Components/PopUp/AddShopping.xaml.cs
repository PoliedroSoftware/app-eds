using APP.Eds.Services.Shopping;
using CommunityToolkit.Maui.Views;

namespace APP.Eds.Components.PopUp;

public partial class AddShopping : Popup
{
    private readonly ShoppingService shoppingService;

    public AddShopping(ShoppingService shoppingService)
	{
		InitializeComponent();
        this.shoppingService = shoppingService;
        BindingContext = shoppingService;
    }

    private void OnCloseTapped(object sender, EventArgs e)
    {
        Close();

    }

    private async void Add_Product(object sender, EventArgs e)
    {
        if (BindingContext is ShoppingService vm && vm.SelectedProduct is not null)
        {
            var selectedExpenditure = vm.SelectedProduct.IdProduct;
            await shoppingService.AddShoppingProductFromPopup();
        }
        else
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Por favor, seleccione un Producto", "OK");
        }

        ShopingProductPicker.SelectedItem = null;
        FirstEntry.IsEnabled = false;
        SecondEntry.IsEnabled = false;
        
        Close();
    }

    private void ShopingProductSelected(object sender, EventArgs e)
    {
        if (ShopingProductPicker.SelectedIndex != -1)
        {
            FirstEntry.IsEnabled = true;
            FirstEntry.Focus();
            FirstEntry.CursorPosition = FirstEntry.Text.Length;
        }
    }

    private void EntryPriceCompleted(object sender, EventArgs e)
    {
        if (BindingContext is ShoppingService vm)
        {
            SecondEntry.Focus();
        }
    }

    private void EntryQuantityCompleted(object sender, EventArgs e)
    {
        if (BindingContext is ShoppingService vm)
        {
            AddButton.Focus();
        }
    }

    private void FirstEntry_Completed(object sender, EventArgs e)
    {

    }
}