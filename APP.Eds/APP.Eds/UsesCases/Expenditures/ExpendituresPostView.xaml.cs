using APP.Eds.Services.Expenditures;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace APP.Eds.UsesCases.Expenditures;

public partial class ExpendituresPostView : ContentPage, INotifyPropertyChanged
{
    private ExpendituresService _expendituresService;
    
    public ExpendituresPostView()
    {
        InitializeComponent();
        _expendituresService = new ExpendituresService();
        BindingContext = _expendituresService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            LoadingOverlay?.ShowLoading();
            await _expendituresService.InitializeAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al cargar datos: {ex.Message}", "OK");
        }
        finally
        {
            LoadingOverlay?.HideLoading();
        }
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (button != null)
        {
            button.IsEnabled = false;
        }

        try
        {
            // Enhanced validation
            if (string.IsNullOrWhiteSpace(_expendituresService.SelectedCategory))
            {
                await DisplayAlert("Error", "Por favor, seleccione una categoría de gasto", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(_expendituresService.Amount))
            {
                await DisplayAlert("Error", "Por favor, ingrese el monto del gasto", "OK");
                return;
            }

            if (!decimal.TryParse(_expendituresService.Amount, out decimal amount) || amount <= 0)
            {
                await DisplayAlert("Error", "Por favor, ingrese un monto válido mayor a cero", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(_expendituresService.Description))
            {
                await DisplayAlert("Error", "Por favor, agregue una descripción del gasto", "OK");
                return;
            }

            LoadingOverlay?.ShowLoading();
            
            await _expendituresService.SaveExpendituresDataAsync();
            
            // Clear form after successful save
            ClearForm();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al guardar: {ex.Message}", "OK");
        }
        finally
        {
            LoadingOverlay?.HideLoading();
            if (button != null)
            {
                button.IsEnabled = true;
            }
        }
    }

    private void ClearForm()
    {
        if (_expendituresService != null)
        {
            _expendituresService.SelectedCategory = null;
            _expendituresService.Amount = string.Empty;
            _expendituresService.Description = string.Empty;
            _expendituresService.ExpenseDate = DateTime.Now;
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}