using APP.Eds.Services.Expenditures;
using APP.Eds.Components.PopUp;
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
            await CustomAlert.ShowErrorAsync($"Error al cargar los datos de gastos:\n\n{ex.Message}", "Error de Inicialización");
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
            // Enhanced validation with professional alerts
            if (string.IsNullOrWhiteSpace(_expendituresService.SelectedCategory))
            {
                await CustomAlert.ShowErrorAsync("Debe seleccionar una categoría para clasificar el gasto", "Categoría Requerida");
                return;
            }

            if (string.IsNullOrWhiteSpace(_expendituresService.Amount))
            {
                await CustomAlert.ShowErrorAsync("Debe ingresar el monto del gasto para continuar", "Monto Requerido");
                return;
            }

            if (!decimal.TryParse(_expendituresService.Amount, out decimal amount) || amount <= 0)
            {
                await CustomAlert.ShowErrorAsync("El monto debe ser un número válido mayor que cero", "Monto Inválido");
                return;
            }

            if (amount > 1000000)
            {
                bool confirm = await CustomAlert.ShowConfirmAsync(
                    $"El monto ingresado (${amount:F2}) es muy elevado.\n\n¿Confirma que este valor es correcto?",
                    "Monto Elevado",
                    "Confirmar",
                    "Revisar");
                
                if (!confirm) return;
            }

            if (string.IsNullOrWhiteSpace(_expendituresService.Description))
            {
                await CustomAlert.ShowErrorAsync("Debe proporcionar una descripción detallada del gasto", "Descripción Requerida");
                return;
            }

            if (_expendituresService.Description.Length < 5)
            {
                await CustomAlert.ShowErrorAsync("La descripción debe tener al menos 5 caracteres para ser informativa", "Descripción Muy Corta");
                return;
            }

            if (_expendituresService.Description.Length > 200)
            {
                await CustomAlert.ShowErrorAsync("La descripción no puede exceder 200 caracteres", "Descripción Muy Larga");
                return;
            }

            LoadingOverlay?.ShowLoading();
            
            await _expendituresService.SaveExpendituresDataAsync();
            
            // Show success message with details
            await CustomAlert.ShowSuccessAsync(
                $"Gasto registrado exitosamente:\n\n" +
                $"• Categoría: {_expendituresService.SelectedCategory}\n" +
                $"• Monto: ${amount:F2}\n" +
                $"• Descripción: {_expendituresService.Description}\n" +
                $"• Fecha: {_expendituresService.ExpenseDate:dd/MM/yyyy}",
                "Gasto Registrado");
            
            // Clear form after successful save
            ClearForm();
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error al guardar el gasto:\n\n{ex.Message}", "Error del Sistema");
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