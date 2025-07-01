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
            return;
        }
        if (vm.Quantity <= 0 || vm.Price <= 0 || /*vm.SelectedCompartiment == null ||*/ vm.SellPrice <= 0)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Complete todos los campos", "OK");
            return;
        }

        ProductPicker.SelectedItem = null;
        FirstEntry.IsEnabled = false;
        SecondEntry.IsEnabled = false;
        ThirdEntry.IsEnabled = false;
        CompartimentPicker.SelectedItem = null;

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

    private void ProductSelected(object sender, EventArgs e)
    {
        if (ProductPicker.SelectedIndex != -1)
        {
            CompartimentPicker.IsEnabled = true;
            CompartimentPicker.Focus();
        }
    }

    private void CompartimentSelected(object sender, EventArgs e)
    {
        if (ProductPicker.SelectedIndex != -1)
        {
            FirstEntry.IsEnabled = true;
            SecondEntry.IsEnabled = true;
            ThirdEntry.IsEnabled = true;
            FirstEntry.Focus();
            FirstEntry.CursorPosition = FirstEntry.Text.Length;
        }
    }
}