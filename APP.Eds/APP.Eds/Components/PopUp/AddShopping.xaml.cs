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
        try
        {
            // Disable button to prevent multiple submissions
            if (sender is Button button)
            {
                button.IsEnabled = false;
                button.Text = "Agregando...";
            }

            if (BindingContext is not ShoppingService vm)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Error de contexto", "OK");
                return;
            }

            // Validate product selection
            if (vm.SelectedProductCompartimentPair is null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Por favor seleccione un producto y compartimento", "OK");
                return;
            }

            // Validate quantity
            if (vm.Quantity <= 0)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Por favor ingrese una cantidad válida (mayor que 0)", "OK");
                return;
            }

            // Validate purchase price
            if (vm.Price <= 0)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Por favor ingrese un precio de compra válido (mayor que 0)", "OK");
                return;
            }

            // Validate sell price
            if (vm.SellPrice <= 0)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Por favor ingrese un precio de venta válido (mayor que 0)", "OK");
                return;
            }

            // Validate that sell price is greater than purchase price
            if (vm.SellPrice <= vm.Price)
            {
                bool confirm = await Application.Current.MainPage.DisplayAlert(
                    "Advertencia", 
                    "El precio de venta es menor o igual al precio de compra. ¿Desea continuar?", 
                    "Sí", "No");
                if (!confirm) return;
            }

            await shoppingService.AddShoppingProductFromPopup();

            // Clear form
            ProductCompartimentPicker.SelectedItem = null;
            FirstEntry.IsEnabled = false;
            SecondEntry.IsEnabled = false;
            ThirdEntry.IsEnabled = false;

            // Show success feedback
            await Application.Current.MainPage.DisplayAlert("Éxito", "Producto agregado correctamente", "OK");

            Close();
        }
        finally
        {
            // Re-enable button
            if (sender is Button button)
            {
                button.IsEnabled = true;
                button.Text = "?? Agregar a la Compra";
            }
        }
    }

    private void EntryPriceCompleted(object sender, EventArgs e)
    {
        if (BindingContext is ShoppingService vm)
        {
            SecondEntry.Focus();
            SecondEntry.CursorPosition = SecondEntry.Text?.Length ?? 0;
        }
    }

    private void EntryQuantityCompleted(object sender, EventArgs e)
    {
        if (BindingContext is ShoppingService vm)
        {
            ThirdEntry.Focus();
            ThirdEntry.CursorPosition = ThirdEntry.Text?.Length ?? 0;
        }
    }

    private void ProductCompartimentPickerSelected(object sender, EventArgs e)
    {
        if (ProductCompartimentPicker.SelectedIndex != -1)
        {
            FirstEntry.Focus();
            FirstEntry.CursorPosition = FirstEntry.Text?.Length ?? 0;
        }
    }
}