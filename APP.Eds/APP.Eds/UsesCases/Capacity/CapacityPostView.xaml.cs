using APP.Eds.Services.Capacity;
using APP.Eds.Components.PopUp;

namespace APP.Eds.UsesCases.Capacity;

public partial class CapacityPostView : ContentPage
{
    private CapacityService _capacityService;
    
    public CapacityPostView()
    {
        InitializeComponent();
        _capacityService = new CapacityService();
        BindingContext = _capacityService;
    }

    private async void SendCapacityButton(object sender, EventArgs e)
    {
        try
        {
            // Disable button to prevent multiple submissions
            if (sender is Button button)
            {
                button.IsEnabled = false;
                button.Text = "Enviando...";
            }

            // Enhanced validation with professional alerts
            if (string.IsNullOrWhiteSpace(_capacityService.Code))
            {
                await CustomAlert.ShowErrorAsync("El código de la capacidad es obligatorio para identificar la configuración", "Código Requerido");
                return;
            }

            if (_capacityService.Code.Length < 2)
            {
                await CustomAlert.ShowErrorAsync("El código debe tener al menos 2 caracteres", "Código Muy Corto");
                return;
            }

            if (_capacityService.Code.Length > 20)
            {
                await CustomAlert.ShowErrorAsync("El código no puede exceder 20 caracteres", "Código Muy Largo");
                return;
            }

            if (_capacityService.Height == null || _capacityService.Height <= 0)
            {
                await CustomAlert.ShowErrorAsync("Debe especificar una altura válida (mayor que 0 metros)", "Altura Inválida");
                return;
            }

            if (_capacityService.Height > 50)
            {
                await CustomAlert.ShowErrorAsync("La altura no puede ser mayor a 50 metros por razones de seguridad", "Altura Excesiva");
                return;
            }

            if (_capacityService.Gallon == null || _capacityService.Gallon <= 0)
            {
                await CustomAlert.ShowErrorAsync("Debe especificar una capacidad en galones válida (mayor que 0)", "Capacidad en Galones Inválida");
                return;
            }

            if (_capacityService.Liters == null || _capacityService.Liters <= 0)
            {
                await CustomAlert.ShowErrorAsync("Debe especificar una capacidad en litros válida (mayor que 0)", "Capacidad en Litros Inválida");
                return;
            }

            // Validate conversion consistency (approximate)
            double expectedLiters = (_capacityService.Gallon ?? 0) * 3.78541;
            double tolerance = expectedLiters * 0.05; // 5% tolerance
            double actualLiters = _capacityService.Liters ?? 0;

            if (Math.Abs(expectedLiters - actualLiters) > tolerance)
            {
                bool confirm = await CustomAlert.ShowConfirmAsync(
                    $"Los valores de galones y litros no coinciden con la conversión estándar:\n\n" +
                    $"• Galones ingresados: {_capacityService.Gallon:F2}\n" +
                    $"• Litros ingresados: {actualLiters:F0} L\n" +
                    $"• Litros calculados: {expectedLiters:F0} L\n" +
                    $"• Diferencia: {Math.Abs(expectedLiters - actualLiters):F0} L\n\n" +
                    $"¿Desea continuar con estos valores?", 
                    "Conversión Inconsistente", 
                    "Continuar", 
                    "Revisar");
                
                if (!confirm) return;
            }

            LoadingOverlay.ShowLoading();
            await _capacityService.SaveCapacityDataAsync();
            
            await CustomAlert.ShowSuccessAsync(
                $"Capacidad registrada exitosamente:\n\n" +
                $"• Código: {_capacityService.Code}\n" +
                $"• Altura: {_capacityService.Height:F2} m\n" +
                $"• Capacidad: {_capacityService.Gallon:F2} gal / {_capacityService.Liters:F0} L",
                "Capacidad Registrada");
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error al guardar la capacidad:\n\n{ex.Message}", "Error del Sistema");
        }
        finally
        {
            LoadingOverlay.HideLoading();

            // Clear form fields after successful submission
            Code = null;
            Height = null;
            Gallon = null;
            Liters = null;

            // Re-enable button
            if (sender is Button button)
            {
                button.IsEnabled = true;
                button.Text = _capacityService.SendData; // Restore original text from translations
            }
        }
    }

    public string? Code
    {
        get => _capacityService.Code;
        set
        {
            _capacityService.Code = value;
            OnPropertyChanged();
        }
    }
    
    public double? Height
    {
        get => _capacityService.Height;
        set
        {
            _capacityService.Height = value;
            OnPropertyChanged();
        }
    }

    public double? Gallon
    {
        get => _capacityService.Gallon;
        set
        {
            _capacityService.Gallon = value;
            OnPropertyChanged();
            
            // Auto-calculate liters when gallons change (optional helper)
            if (value.HasValue && value > 0)
            {
                double calculatedLiters = value.Value * 3.78541;
                // Only auto-fill if Liters is empty or zero
                if ((_capacityService.Liters ?? 0) == 0)
                {
                    Liters = (int)Math.Round(calculatedLiters);
                }
            }
        }
    }

    public int? Liters
    {
        get => _capacityService.Liters;
        set
        {
            _capacityService.Liters = value;
            OnPropertyChanged();
        }
    }
}