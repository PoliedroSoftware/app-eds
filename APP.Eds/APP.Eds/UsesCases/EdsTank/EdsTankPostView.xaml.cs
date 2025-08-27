using APP.Eds.Services.EdsTank;
using APP.Eds.Components.PopUp;

namespace APP.Eds.UsesCases.EdsTank;

public partial class EdsTankPostView : ContentPage
{
    private EdsTankService _edsTankService;

    public EdsTankPostView()
    {
        InitializeComponent();
        _edsTankService = new EdsTankService();
        BindingContext = _edsTankService;
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        try
        {
            // Disable button to prevent multiple submissions
            if (sender is Button button)
            {
                button.IsEnabled = false;
                button.Text = "Asignando...";
            }

            // Enhanced validation with professional alerts
            if (_edsTankService.SelectEds == null)
            {
                await CustomAlert.ShowErrorAsync("Debe seleccionar una estación de servicio (EDS) para asignar el tanque", "EDS Requerida");
                return;
            }

            if (_edsTankService.SelectTank == null)
            {
                await CustomAlert.ShowErrorAsync("Debe seleccionar un tanque para asignar a la estación de servicio", "Tanque Requerido");
                return;
            }

            // Show confirmation with details
            string edsName = _edsTankService.SelectEds.Name ?? "N/A";
            string tankNumber = _edsTankService.SelectTank.Number ?? "N/A";
            
            bool confirm = await CustomAlert.ShowConfirmAsync(
                $"¿Confirma que desea asignar el tanque #{tankNumber} a la estación de servicio '{edsName}'?\n\nEsta asignación establecerá la relación entre el tanque y la EDS.",
                "Confirmar Asignación",
                "Asignar",
                "Cancelar");

            if (!confirm) return;

            LoadingOverlay.ShowLoading();
            await _edsTankService.SaveEdsTankDataAsync();
            
            await CustomAlert.ShowSuccessAsync(
                $"Asignación completada exitosamente:\n\n" +
                $"• Tanque: #{tankNumber}\n" +
                $"• EDS: {edsName}\n\n" +
                $"El tanque ahora está disponible para operaciones en esta estación.",
                "Tanque Asignado");
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error al asignar el tanque:\n\n{ex.Message}", "Error del Sistema");
        }
        finally
        {
            LoadingOverlay.HideLoading();

            // Clear selections
            _edsTankService.SelectEds = null;
            _edsTankService.SelectTank = null;
            
            // Re-enable button
            if (sender is Button button)
            {
                button.IsEnabled = true;
                button.Text = "?? Asignar Tanque";
            }
        }
    }
}
