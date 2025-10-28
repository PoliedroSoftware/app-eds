using APP.Eds.Services.HoseHistory;
using APP.Eds.Controls;
using APP.Eds.Components.PopUp;

namespace APP.Eds.UsesCases.HoseHistory;

public partial class HoseHistoryPostView : ContentPage
{
    private HoseHistoryService _hosehistoryService;

    public HoseHistoryPostView()
    {
        InitializeComponent();
        _hosehistoryService = new HoseHistoryService();
        BindingContext = _hosehistoryService;
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        if (BindingContext is HoseHistoryService vm)
        {
            try
            {
                // Disable button to prevent multiple submissions
                if (sender is HoverButton hoverButton)
                {
                    hoverButton.IsEnabled = false;
                }

                // Enhanced validation with professional alerts
                if (vm.Date.Date > DateTime.Now.Date)
                {
                    await CustomAlert.ShowWarningAsync("La fecha seleccionada es futura. Se recomienda usar la fecha actual para registros de historial.", "Fecha Futura");
                }
                else if (vm.Date.Date < DateTime.Now.Date.AddDays(-30))
                {
                    bool confirm = await CustomAlert.ShowConfirmAsync(
                        $"La fecha seleccionada ({vm.Date:dd/MM/yyyy}) es muy antigua (más de 30 días).\n\n¿Confirma que desea registrar este historial con esta fecha?",
                        "Fecha Antigua",
                        "Confirmar",
                        "Cambiar Fecha");
                    
                    if (!confirm) return;
                }

                if (vm.AccumulatedAmount <= 0)
                {
                    await CustomAlert.ShowErrorAsync("Debe especificar un monto acumulado válido (mayor que 0)", "Monto Inválido");
                    return;
                }

                if (vm.AccumulatedAmount > 5000000)
                {
                    bool confirm = await CustomAlert.ShowConfirmAsync(
                        $"El monto acumulado (${vm.AccumulatedAmount:F2}) es muy elevado.\n\n¿Confirma que este valor es correcto?",
                        "Monto Elevado",
                        "Confirmar",
                        "Revisar");
                    
                    if (!confirm) return;
                }

                if (vm.AccumulatedGallons <= 0)
                {
                    await CustomAlert.ShowErrorAsync("Debe especificar galones acumulados válidos (mayor que 0)", "Galones Inválidos");
                    return;
                }

                if (vm.AccumulatedGallons > 20000)
                {
                    await CustomAlert.ShowWarningAsync("La cantidad de galones acumulados parece muy alta. Por favor verifique.", "Galones Elevados");
                }

                if (vm.SelectedDispensers is null)
                {
                    await CustomAlert.ShowErrorAsync("Debe seleccionar el dispensador para registrar el historial", "Dispensador Requerido");
                    return;
                }

                if (vm.SelectHose is null)
                {
                    await CustomAlert.ShowErrorAsync("Debe seleccionar la manguera correspondiente", "Manguera Requerida");
                    return;
                }

                // Calculate and validate price per gallon
                double pricePerGallon = vm.AccumulatedAmount / vm.AccumulatedGallons;
                if (pricePerGallon < 1000 || pricePerGallon > 20000)
                {
                    bool confirmPrice = await CustomAlert.ShowConfirmAsync(
                        $"El precio por galón calculado parece inusual:\n\n" +
                        $"• Monto: ${vm.AccumulatedAmount:F2}\n" +
                        $"• Galones: {vm.AccumulatedGallons:F2}\n" +
                        $"• Precio/Galón: ${pricePerGallon:F0}\n\n" +
                        $"¿Los valores son correctos?",
                        "Precio Inusual",
                        "Continuar",
                        "Revisar");
                    
                    if (!confirmPrice) return;
                }

                LoadingOverlay.ShowLoading();
                await vm.SaveHoseHistoryDataAsync();
                
                await CustomAlert.ShowSuccessAsync(
                    $"Historial de manguera registrado exitosamente:\n\n" +
                    $"• Fecha: {vm.Date:dd/MM/yyyy}\n" +
                    $"• Dispensador: {vm.SelectedDispensers.Code}\n" +
                    $"• Manguera: #{vm.SelectHose.Number}\n" +
                    $"• Monto Acumulado: ${vm.AccumulatedAmount:F2}\n" +
                    $"• Galones: {vm.AccumulatedGallons:F2}\n" +
                    $"• Precio/Galón: ${pricePerGallon:F0}",
                    "Historial Registrado");

                // Clear form fields after successful submission
                _hosehistoryService.Date = DateTime.Now;
                AccumulatedAmount = 0;
                AccumulatedGallons = 0;
                _hosehistoryService.SelectedDispensers = null;
                _hosehistoryService.SelectHose = null;
            }
            catch (Exception ex)
            {
                await CustomAlert.ShowErrorAsync($"Error al registrar el historial de manguera:\n\n{ex.Message}", "Error del Sistema");
            }
            finally
            {
                LoadingOverlay.HideLoading();

                // Re-enable button
                if (sender is HoverButton hoverButton)
                {
                    hoverButton.IsEnabled = true;
                }
            }
        }
        else
        {
            await CustomAlert.ShowErrorAsync("Error interno del sistema. Por favor, intente nuevamente", "Error de Contexto");
        }
    }

    public double AccumulatedAmount
    {
        get => _hosehistoryService.AccumulatedAmount;
        set
        {
            _hosehistoryService.AccumulatedAmount = value;
            OnPropertyChanged();
        }
    }
    
    public double AccumulatedGallons
    {
        get => _hosehistoryService.AccumulatedGallons;
        set
        {
            _hosehistoryService.AccumulatedGallons = value;
            OnPropertyChanged();
        }
    }
}
