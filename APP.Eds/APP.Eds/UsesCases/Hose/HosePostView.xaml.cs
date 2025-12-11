using APP.Eds.Services.Hose;
using APP.Eds.UsesCases.Compartiment;
using APP.Eds.Controls;
using APP.Eds.Components.PopUp;

namespace APP.Eds.UsesCases.Hose;

public partial class HosePostView : ContentPage
{
    private HoseService _hoseService;

    public HosePostView()
    {
        InitializeComponent();
        _hoseService = new HoseService();
        BindingContext = _hoseService;
    }

     protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Llamada correcta al método GetHoseAsync del servicio
        await _hoseService.GetHoseAsync();

    }


    private async void OnCompartimentButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CompartimentPostView());
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        // Store button reference before try block
        var button = sender as HoverButton;
        
        if (BindingContext is HoseService vm)
        {
            try
            {
                // Disable button to prevent multiple submissions
                if (button != null)
                {
                    button.IsEnabled = false;
                }

                // Enhanced validation with professional alerts
                if (vm.Number <= 0)
                {
                    await CustomAlert.ShowErrorAsync("Debe especificar un número de manguera válido (mayor que 0)", "Número Inválido");
                    return;
                }

                if (vm.Number > 20)
                {
                    await CustomAlert.ShowErrorAsync("El número de manguera no puede exceder 20", "Número Excesivo");
                    return;
                }

                if (vm.AccumulatedAmount <= 0)
                {
                    await CustomAlert.ShowErrorAsync("Debe especificar un monto acumulado válido (mayor que 0)", "Monto Acumulado Inválido");
                    return;
                }

                if (vm.AccumulatedAmount > 10000000)
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

                if (vm.AccumulatedGallons > 50000)
                {
                    await CustomAlert.ShowWarningAsync("La cantidad de galones acumulados parece muy alta. Por favor verifique.", "Galones Elevados");
                }

                if (vm.SelectedDispensers is null)
                {
                    await CustomAlert.ShowErrorAsync("Debe seleccionar el dispensador al cual pertenece esta manguera", "Dispensador Requerido");
                    return;
                }

                if (vm.SelectProductType is null)
                {
                    await CustomAlert.ShowErrorAsync("Debe seleccionar el tipo de producto que maneja esta manguera", "Tipo de Producto Requerido");
                    return;
                }

                if (vm.SelectCompartiment is null)
                {
                    await CustomAlert.ShowErrorAsync("Debe seleccionar el compartimiento asociado a esta manguera", "Compartimiento Requerido");
                    return;
                }

                // Calculate price per gallon if possible
                double pricePerGallon = vm.AccumulatedAmount / vm.AccumulatedGallons;
                if (pricePerGallon < 1000 || pricePerGallon > 20000)
                {
                    bool confirmPrice = await CustomAlert.ShowConfirmAsync(
                        $"El precio por galón calculado (${pricePerGallon:F0}) parece inusual.\n\n" +
                        $"• Monto: ${vm.AccumulatedAmount:F2}\n" +
                        $"• Galones: {vm.AccumulatedGallons:F2}\n" +
                        $"• Precio/Galón: ${pricePerGallon:F0}\n\n" +
                        $"¿Desea continuar con estos valores?",
                        "Precio Inusual",
                        "Continuar",
                        "Revisar");
                    
                    if (!confirmPrice) return;
                }

                LoadingOverlay.ShowLoading();
                bool success = await vm.SaveHoseDataAsync();
                
                if (success)
                {
                    await CustomAlert.ShowSuccessAsync(
                        $"Manguera #{vm.Number} registrada exitosamente:\n\n" +
                        $"• Dispensador: {vm.SelectedDispensers.Code}\n" +
                        $"• Tipo de Producto: {vm.SelectProductType.Description}\n" +
                        $"• Monto Acumulado: ${vm.AccumulatedAmount:F2}\n" +
                        $"• Galones Acumulados: {vm.AccumulatedGallons:F2}\n" +
                        $"• Precio por Galón: ${pricePerGallon:F0}",
                        "Manguera Registrada");

                    // Clear form fields after successful submission
                    Number = 0;
                    AccumulatedAmount = 0;
                    AccumulatedGallons = 0;
                    _hoseService.SelectedDispensers = default!;
                    _hoseService.SelectProductType = default!;
                    _hoseService.SelectCompartiment = default!;
                }
            }
            catch (Exception ex)
            {
                await CustomAlert.ShowErrorAsync($"Error al registrar la manguera:\n\n{ex.Message}", "Error del Sistema");
            }
            finally
            {
                LoadingOverlay.HideLoading();

                // Re-enable button
                if (button != null)
                {
                    button.IsEnabled = true;
                }
            }
        }
        else
        {
            await CustomAlert.ShowErrorAsync("Error interno del sistema. Por favor, intente nuevamente", "Error de Contexto");
        }
    }

    public int Number
    {
        get => _hoseService.Number;
        set
        {
            _hoseService.Number = value;
            OnPropertyChanged();
        }
    }

    public double AccumulatedAmount
    {
        get => _hoseService.AccumulatedAmount;
        set
        {
            _hoseService.AccumulatedAmount = value;
            OnPropertyChanged();
        }
    }

    public double AccumulatedGallons
    {
        get => _hoseService.AccumulatedGallons;
        set
        {
            _hoseService.AccumulatedGallons = value;
            OnPropertyChanged();
        }
    }
}