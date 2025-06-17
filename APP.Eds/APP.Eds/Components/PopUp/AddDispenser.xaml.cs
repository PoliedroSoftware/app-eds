using APP.Eds.Services.Court;
using CommunityToolkit.Maui.Views;

namespace APP.Eds.Components.PopUp;


public partial class AddDispenser : Popup
{
    private readonly CourtService courtService;
    private bool isGallonsEditable = false;

    public AddDispenser(CourtService courtService)
    {
        InitializeComponent();
        this.courtService = courtService;
        SecondEntry.IsEnabled = isGallonsEditable;
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

            Close();
        }
        else
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Por favor, seleccione una Manguera", "OK");
        }
    }
    private const double PricePerGallon = 5000.0;
    private void EntryAccumulatedCompleted(object sender, EventArgs e)
    {
        if (BindingContext is CourtService vm)
        {
            AmountBoxView.Color = vm.AccumulatedAmount >= vm.LastAccumulatedAmount ? Colors.Green : Colors.Red;
            GallonBoxView.Color = vm.AccumulatedGallons >= vm.LastAccumulatedGallons ? Colors.Green : Colors.Red;

            // Cálculo automático de galones
            if (vm.AccumulatedAmount > vm.LastAccumulatedAmount)
            {
                double diferenciaMonto = vm.AccumulatedAmount - vm.LastAccumulatedAmount;
                vm.AccumulatedGallons = vm.LastAccumulatedGallons + (diferenciaMonto / PricePerGallon);
            }

            SecondEntry.Focus();
            SecondEntry.CursorPosition = SecondEntry.Text.Length;
        }
    }
   
    private void OnEntryUnfocused(object sender, FocusEventArgs e)
    {

    }


    private void EntryGallonsCompleted(object sender, EventArgs e)
    {
        if (BindingContext is CourtService vm)
        {
            AmountBoxView.Color = vm.AccumulatedAmount >= vm.LastAccumulatedAmount ? Colors.Green : Colors.Red;
            GallonBoxView.Color = vm.AccumulatedGallons >= vm.LastAccumulatedGallons ? Colors.Green : Colors.Red;
            AddButton.Focus();
        }
    }

    private void HoseSelected(object sender, EventArgs e)
    {
        if(HosePicker.SelectedIndex != -1)
        {
            FirstEntry.IsEnabled = true;
            SecondEntry.IsEnabled = true;
            FirstEntry.Focus();
            FirstEntry.CursorPosition = FirstEntry.Text.Length;
        }
    }
    private void FirstEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is Entry entry)
        {
            string newText = e.NewTextValue;

            if (string.IsNullOrEmpty(newText))
                return;

            if (!decimal.TryParse(newText, System.Globalization.NumberStyles.Number,
                new System.Globalization.CultureInfo("es-CO"), out _))
            {
                entry.Text = e.OldTextValue;
            }
        }
    }

}   

