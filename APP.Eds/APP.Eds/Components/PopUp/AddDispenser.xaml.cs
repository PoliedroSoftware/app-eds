using APP.Eds.Services.Court;
using CommunityToolkit.Maui.Views;

namespace APP.Eds.Components.PopUp;

public partial class AddDispenser : Popup
{
    private readonly CourtService courtService;
    private bool isGallonsEditable = false;
    private bool canEditPrice = false;

    public AddDispenser(CourtService courtService)
    {
        InitializeComponent();
        this.courtService = courtService;
        SecondEntry.IsEnabled = isGallonsEditable;
        
        CheckUserRole();
    }

    private void CheckUserRole()
    {
        var userRole = Preferences.Get("userRole", "User");
        canEditPrice = userRole == "Admin" || userRole == "User";
        
        Dispatcher.Dispatch(() => UpdatePriceEditVisibility());
    }
    private void EditGallonsButton_Clicked(object sender, EventArgs e)
    {
        isGallonsEditable = !isGallonsEditable;
        SecondEntry.IsEnabled = isGallonsEditable;
        if (isGallonsEditable)
        {
            SecondEntry.Focus();
            SecondEntry.CursorPosition = SecondEntry.Text?.Length ?? 0;
        }
    }
    private void OnCloseTapped(object sender, EventArgs e)
    {
        if (BindingContext is CourtService vm)
        {
            vm.AccumulatedAmount = 0;
            vm.AccumulatedGallons = 0;
            vm.LastAccumulatedAmount = 0;
            vm.LastAccumulatedGallons = 0;
        }
        Close();
    }

    private async void Add_Dispenser(object sender, EventArgs e)
    {
        if (BindingContext is CourtService vm && vm.SelectedHose is not null)
        {

            var selectedHoseId = vm.SelectedHose.IdHose;

            if (vm.AccumulatedAmount == 0)
            {
                await Application.Current.MainPage.DisplayAlert("Eror", "Debe ingresar el monto de la venta", "OK");
            }

            if (vm.AccumulatedGallons == 0)
            {
                await Application.Current.MainPage.DisplayAlert("Eror", "Debe ingresar el número de galones vendidos", "OK");
            }

            if (vm.AccumulatedAmount < vm.LastAccumulatedAmount)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "El monto acumulado debe ser mayor que el último monto acumulado.", "OK");
                return;
            }

            if (vm.AccumulatedGallons < vm.LastAccumulatedGallons)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Los galones acumulados deben ser mayores que los últimos galones acumulados.", "OK");
                return;
            }

            
            await courtService.AddDispenserFromPopup();

            vm.AddSelectedHose(vm.SelectedHose);

            vm.AccumulatedAmount = 0;
            vm.AccumulatedGallons = 0;
            vm.LastAccumulatedAmount = 0;
            vm.LastAccumulatedGallons = 0;

            await CloseAsync();

        }
        else
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Por favor, seleccione una Manguera", "OK");
        }
    }

    private void EntryAccumulatedCompleted(object sender, EventArgs e)
    {
        UpdateAccumulatedValues();
    }
   
    private void OnEntryUnfocused(object sender, FocusEventArgs e)
    {
        UpdateAccumulatedValues();
    }

    private void UpdateAccumulatedValues()
    {
        if (BindingContext is CourtService vm && vm.SelectedHose is not null)
        {
            if (vm.AccumulatedAmount > vm.LastAccumulatedAmount)
            {
                double amountDifference = vm.AccumulatedAmount - vm.LastAccumulatedAmount;
                
                double currentPrice = vm.SelectedHose.Price;
                
                vm.AccumulatedGallons = vm.LastAccumulatedGallons + (amountDifference / currentPrice);
            }
            UpdateAccumulatedColors();
        }
    }

    private void UpdateAccumulatedColors()
    {
        if (BindingContext is CourtService vm)
        {
            AmountBoxView.Color = vm.AccumulatedAmount >= vm.LastAccumulatedAmount ? Colors.Green : Colors.Red;
            GallonBoxView.Color = vm.AccumulatedGallons >= vm.LastAccumulatedGallons ? Colors.Green : Colors.Red;
        }
    }


    private void EntryGallonsCompleted(object sender, EventArgs e)
    {
        UpdateAccumulatedColors();
        AddButton.Focus();
    }

    private void HoseSelected(object sender, EventArgs e)
    {
        if (HosePicker.SelectedIndex != -1)
        {
            FirstEntry.IsEnabled = true;
            FirstEntry.Focus();
            FirstEntry.CursorPosition = FirstEntry.Text.Length;

            if (BindingContext is CourtService vm && vm.SelectedHose is not null)
            {
                double price = vm.SelectedHose.Price;
                PricePerGallonLabel.Text = $"{price:C3}";
                
                if (canEditPrice && PriceEditEntry != null)
                {
                    PriceEditEntry.Text = price.ToString("F2");
                }
            }
            else
            {
                PricePerGallonLabel.Text = "##.###";
                if (canEditPrice && PriceEditEntry != null)
                {
                    PriceEditEntry.Text = "";
                }
            }
            
            UpdatePriceEditVisibility();
        }
        else
        {
            PricePerGallonLabel.Text = "##.###";
            if (canEditPrice && PriceEditEntry != null)
            {
                PriceEditEntry.Text = "";
            }
        }
    }

    private void UpdatePriceEditVisibility()
    {
        if (PriceEditEntry != null && PriceEditButton != null)
        {
            PriceEditEntry.IsVisible = canEditPrice;
            PriceEditButton.IsVisible = canEditPrice;
            
            if (PricePerGallonLabel != null)
            {
                PricePerGallonLabel.IsVisible = !canEditPrice;
            }
        }
    }

    private void PriceEditEntry_Completed(object sender, EventArgs e)
    {
        UpdateSelectedHosePrice();
    }

    private void PriceEditEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (BindingContext is CourtService vm && vm.SelectedHose is not null && PriceEditEntry != null)
        {
            if (double.TryParse(e.NewTextValue, out double newPrice) && newPrice > 0)
            {
                vm.SelectedHose.Price = newPrice;
                PricePerGallonLabel.Text = $"{newPrice:C2}";
                
                if (vm.AccumulatedAmount > vm.LastAccumulatedAmount)
                {
                    double amountDifference = vm.AccumulatedAmount - vm.LastAccumulatedAmount;
                    vm.AccumulatedGallons = vm.LastAccumulatedGallons + (amountDifference / newPrice);
                    UpdateAccumulatedColors();
                }
            }
        }
    }

    private void PriceEditButton_Clicked(object sender, EventArgs e)
    {
        if (PriceEditEntry != null)
        {
            PriceEditEntry.Focus();
        }
    }

    private void UpdateSelectedHosePrice()
    {
        if (BindingContext is CourtService vm && vm.SelectedHose is not null && PriceEditEntry != null)
        {
            if (double.TryParse(PriceEditEntry.Text, out double newPrice) && newPrice > 0)
            {
                vm.SelectedHose.Price = newPrice;
                PricePerGallonLabel.Text = $"{newPrice:C2}";
                
                if (vm.AccumulatedAmount > vm.LastAccumulatedAmount)
                {
                    double amountDifference = vm.AccumulatedAmount - vm.LastAccumulatedAmount;
                    vm.AccumulatedGallons = vm.LastAccumulatedGallons + (amountDifference / newPrice);
                    
                    UpdateAccumulatedColors();
                }
            }
            else
            {
                PriceEditEntry.Text = vm.SelectedHose.Price.ToString("F2");
            }
        }
    }

}