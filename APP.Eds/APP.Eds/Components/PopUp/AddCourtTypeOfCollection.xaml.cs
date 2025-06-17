using APP.Eds.Services.Court;
using CommunityToolkit.Maui.Views;
using System.Globalization;

namespace APP.Eds.Components.PopUp;

public partial class AddCourtTypeOfCollection : Popup
{
    private readonly CourtService courtService;

    public AddCourtTypeOfCollection(CourtService courtService)
	{
		InitializeComponent();
        this.courtService = courtService;
    }

    private void OnCloseTapped(object sender, EventArgs e)
    {
        Close();

    }

    private async void Add_Dispenser(object sender, EventArgs e)
    {
        if (BindingContext is CourtService vm && vm.SelectedTypeOfCollection is not null)
        {
            var selectedExpenditure = vm.SelectedTypeOfCollection.IdTypeOfCollection;
            
        }
        else
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Por favor, seleccione un Tipo de Recaudo", "OK");
        }
        if (courtService.CourtTypeOfCollectionAmount <= 0)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Por favor, el egreso debe ser mayor a 0", "OK");
            return;
        }
        if (string.IsNullOrWhiteSpace(courtService.CourtTypeOfCollectionDescription))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Por favor, ingrese una descripcion", "OK");
            return;
        }
        await courtService.AddCourtTypeOfCollectionFromPopup();

        TypeOfCollentionPicker.SelectedItem = null;
        FirstEntry.IsEnabled = false;
        SecondEntry.IsEnabled = false;

        //Close();
    }

    private void TypeOfCollentionSelected(object sender, EventArgs e)
    {
        if (TypeOfCollentionPicker.SelectedIndex != -1)
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
    private async void EntryAmountCompleted(object sender, EventArgs e)
    {
        if (BindingContext is CourtService)
        {
            if (courtService.CourtTypeOfCollectionAmount <= 0)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Por favor, el egreso debe ser mayor a 0", "OK");
                return;

            }
            SecondEntry.Focus();
        }
    }
    
    
    private async void EntryDescriptionCompleted(object sender, EventArgs e)
    {
        if (BindingContext is CourtService)
        {
            if (string.IsNullOrWhiteSpace(courtService.CourtTypeOfCollectionDescription))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Por favor, ingrese una descripcion", "OK");
                return;
            }  
        }
        AddButton.Focus();
    }


}