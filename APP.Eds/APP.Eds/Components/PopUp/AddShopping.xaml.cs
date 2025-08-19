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

    private ShoppingService GetShoppingService()
    {
        return shoppingService;
    }

    private async void Add_Product(object sender, EventArgs e)
    {
        if (BindingContext is not ShoppingService vm || vm.SelectedProductCompartimentPair is null)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Por favor, seleccione un Producto", "OK");
            return;
        }
        else if (vm.Quantity <= 0 || vm.Price <= 0 || vm.SellPrice <= 0)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Complete todos los campos", "OK");
            return;
        }



        await shoppingService.AddShoppingProductFromPopup();


        ProductCompartimentPicker.SelectedItem = null;
        FirstEntry.IsEnabled = false;
        SecondEntry.IsEnabled = false;
        ThirdEntry.IsEnabled = false;

        Close();
    }

    private void EntryPriceCompleted(object sender, EventArgs e)
    {
        if (BindingContext is ShoppingService vm)
        {
            SecondEntry.Focus();
            SecondEntry.CursorPosition = SecondEntry.Text.Length;
        }
    }

    private void EntryQuantityCompleted(object sender, EventArgs e)
    {
        if (BindingContext is ShoppingService vm)
        {
            ThirdEntry.Focus();
            ThirdEntry.CursorPosition = ThirdEntry.Text.Length;
        }
    }

    private void ProductCompartimentPickerSelected(object sender, EventArgs e)
    {
        if (ProductCompartimentPicker.SelectedIndex != -1)
        {
            FirstEntry.Focus();
            FirstEntry.CursorPosition = FirstEntry.Text.Length;
        }
    }
}