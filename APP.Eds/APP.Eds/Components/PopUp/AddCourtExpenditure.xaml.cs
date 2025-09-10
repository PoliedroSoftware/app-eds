using System.Threading.Tasks;
using APP.Eds.Models.Court;
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
        // Fix: Set the BindingContext
        BindingContext = courtService;
    }

    private void OnCloseTapped(object sender, EventArgs e)
    {
        try
        {
            Close();
        }
        catch (ObjectDisposedException ex)
        {
            System.Diagnostics.Debug.WriteLine($"AddCourtExpenditure popup was already disposed during close: {ex.Message}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error closing AddCourtExpenditure popup: {ex.Message}");
        }
    }

    private async void Add_Expenditure(object sender, EventArgs e)
    {
        try
        {
            // Fix: Improved validation logic
            if (courtService.SelectedExpenditure is null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Por favor, seleccione un tipo de Egreso", "OK");
                return;
            }
            
            if (courtService.CourtExpenditureAmount <= 0)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Por favor, el egreso debe ser mayor a 0", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(courtService.ExpenditureDescription))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Por favor, ingrese una descripci�n para el gasto", "OK");
                return;
            }

            // Disable button to prevent multiple submissions
            if (sender is Button button)
            {
                button.IsEnabled = false;
            }

            await courtService.AddCourtExpenditureFromPopup();

            // Reset form
            ExpenditurePicker.SelectedItem = null;
            FirstEntry.IsEnabled = false;
            SecondEntry.IsEnabled = false;
            FirstEntry.Text = string.Empty;
            SecondEntry.Text = string.Empty;
            
            await CloseAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error adding expenditure: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Error", $"Error al agregar el gasto: {ex.Message}", "OK");
        }
        finally
        {
            // Re-enable button
            if (sender is Button button)
            {
                button.IsEnabled = true;
            }
        }
    }

    private async Task CloseAsync()
    {
        try
        {
            await Task.Delay(100);
            try
            {
                Close();
            }
            catch (ObjectDisposedException ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddCourtExpenditure popup was already disposed during async close: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in AddCourtExpenditure CloseAsync: {ex.Message}");
        }
    }

    private void ExpenditureSelected(object sender, EventArgs e)
    {
        try
        {
            if (ExpenditurePicker.SelectedIndex != -1)
            {
                FirstEntry.IsEnabled = true;
                SecondEntry.IsEnabled = true;
                FirstEntry.Focus();
                
                // Fix: Safer cursor positioning
                if (!string.IsNullOrEmpty(FirstEntry.Text))
                {
                    FirstEntry.CursorPosition = FirstEntry.Text.Length;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in ExpenditureSelected: {ex.Message}");
        }
    }

    private async void EntryAmountCompleted(object sender, EventArgs e)
    {
        try
        {
            if (courtService.CourtExpenditureAmount <= 0)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Por favor, el egreso debe ser mayor a 0", "OK");
                return;
            }
            
            SecondEntry.Focus();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in EntryAmountCompleted: {ex.Message}");
        }
    }

    private void FirstEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            if (sender is Entry entry)
            {
                string newText = e.NewTextValue;

                if (string.IsNullOrEmpty(newText))
                    return;

                // Fix: Better number validation with culture handling
                if (!decimal.TryParse(newText, System.Globalization.NumberStyles.Number,
                    System.Globalization.CultureInfo.InvariantCulture, out _))
                {
                    entry.Text = e.OldTextValue;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in FirstEntry_TextChanged: {ex.Message}");
        }
    }
}