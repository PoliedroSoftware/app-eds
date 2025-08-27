using APP.Eds.Services.Capacity;

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

            // Validate required fields
            if (string.IsNullOrWhiteSpace(_capacityService.Code))
            {
                await DisplayAlert("Error", "Por favor ingrese el código de la capacidad", "OK");
                return;
            }

            if (_capacityService.Code.Length < 2)
            {
                await DisplayAlert("Error", "El código debe tener al menos 2 caracteres", "OK");
                return;
            }

            if (_capacityService.Height == null || _capacityService.Height <= 0)
            {
                await DisplayAlert("Error", "Por favor ingrese una altura válida (mayor que 0)", "OK");
                return;
            }

            if (_capacityService.Height > 50)
            {
                await DisplayAlert("Error", "La altura no puede ser mayor a 50 metros", "OK");
                return;
            }

            if (_capacityService.Gallon == null || _capacityService.Gallon <= 0)
            {
                await DisplayAlert("Error", "Por favor ingrese una capacidad en galones válida (mayor que 0)", "OK");
                return;
            }

            if (_capacityService.Liters == null || _capacityService.Liters <= 0)
            {
                await DisplayAlert("Error", "Por favor ingrese una capacidad en litros válida (mayor que 0)", "OK");
                return;
            }

            // Validate conversion consistency (approximate)
            double expectedLiters = (_capacityService.Gallon ?? 0) * 3.78541;
            double tolerance = expectedLiters * 0.05; // 5% tolerance
            double actualLiters = _capacityService.Liters ?? 0;

            if (Math.Abs(expectedLiters - actualLiters) > tolerance)
            {
                bool confirm = await DisplayAlert("Advertencia", 
                    $"Los valores de galones y litros no coinciden con la conversión estándar.\n" +
                    $"Galones: {_capacityService.Gallon:F2}\n" +
                    $"Litros ingresados: {actualLiters:F0}\n" +
                    $"Litros calculados: {expectedLiters:F0}\n" +
                    $"¿Desea continuar?", 
                    "Sí", "No");
                if (!confirm) return;
            }

            LoadingOverlay.ShowLoading();
            await _capacityService.SaveCapacityDataAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al guardar la capacidad: {ex.Message}", "OK");
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