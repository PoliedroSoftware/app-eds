using APP.Eds.Services.Court;
using CommunityToolkit.Maui.Views;

namespace APP.Eds.Components.PopUp;

public partial class AddCourtExpenditure : Popup
{
    private readonly CourtService courtService;

    public AddCourtExpenditure(CourtService courtService)
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
        if (BindingContext is CourtService vm && vm.SelectedExpenditure is not null)
        {
            var selectedExpenditure = vm.SelectedExpenditure.IdCourtExpenditure;
            await courtService.AddCourtExpenditureFromPopup();
        }
        else
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Por favor, seleccione un tipo de Egreso", "OK");
        }

        ExpenditurePicker.SelectedItem = null;
        FirstEntry.IsEnabled = false;
        SecondEntry.IsEnabled = false;

        Close();
    }

    private void ExpenditureSelected(object sender, EventArgs e)
    {
        if (ExpenditurePicker.SelectedIndex != -1)
        {
            FirstEntry.IsEnabled = true;
            SecondEntry.IsEnabled = true;
            FirstEntry.Focus();
            FirstEntry.CursorPosition = FirstEntry.Text.Length;
        }
    }

    private void EntryAmountCompleted(object sender, EventArgs e)
    {
        if (BindingContext is CourtService vm)
        {
            SecondEntry.Focus();
        }
    }

    private void EntryDescriptionCompleted(object sender, EventArgs e)
    {
        if (BindingContext is CourtService vm)
        {
            AddButton.Focus();
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