using APP.Eds.Services.Tank;
using APP.Eds.Components.PopUp;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace APP.Eds.UsesCases.Tank;

public partial class TankPostView : ContentPage
{
    private TankService _tankService;
    
    public TankPostView()
    {
        InitializeComponent();
        _tankService = new TankService();
        BindingContext = _tankService;
    }

    private async void SendData(object sender, EventArgs e)
    {
        if (BindingContext is TankService vm)
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
                if (string.IsNullOrWhiteSpace(vm.Number))
                {
                    await CustomAlert.ShowErrorAsync("El número del tanque es obligatorio para identificar el equipo", "Número Requerido");
                    return;
                }

                if (vm.Number.Length < 2)
                {
                    await CustomAlert.ShowErrorAsync("El número del tanque debe tener al menos 2 caracteres", "Número Muy Corto");
                    return;
                }

                if (vm.Number.Length > 20)
                {
                    await CustomAlert.ShowErrorAsync("El número del tanque no puede exceder 20 caracteres", "Número Muy Largo");
                    return;
                }

                if (vm.Compartment <= 0)
                {
                    await CustomAlert.ShowErrorAsync("Debe especificar un número válido de compartimientos (mayor que 0)", "Compartimientos Inválidos");
                    return;
                }

                if (vm.Compartment > 10)
                {
                    await CustomAlert.ShowErrorAsync("El número de compartimientos no puede ser mayor a 10 por razones de seguridad", "Demasiados Compartimientos");
                    return;
                }

                if (vm.Ability <= 0)
                {
                    await CustomAlert.ShowErrorAsync("Debe especificar una capacidad válida del tanque (mayor que 0 litros)", "Capacidad Inválida");
                    return;
                }

                if (vm.Ability > 100000)
                {
                    await CustomAlert.ShowErrorAsync("La capacidad del tanque no puede exceder 100,000 litros", "Capacidad Excesiva");
                    return;
                }

                // Validate stock if provided
                if (vm.Stock.HasValue && vm.Stock < 0)
                {
                    await CustomAlert.ShowErrorAsync("El stock actual no puede ser negativo", "Stock Inválido");
                    return;
                }

                if (vm.Stock.HasValue && vm.Stock > vm.Ability)
                {
                    bool confirm = await CustomAlert.ShowConfirmAsync(
                        $"El stock actual ({vm.Stock:F2} L) es mayor que la capacidad del tanque ({vm.Ability:F2} L).\n\n¿Está seguro de que estos valores son correctos?",
                        "Stock Excede Capacidad",
                        "Continuar",
                        "Revisar");
                    
                    if (!confirm) return;
                }

                LoadingOverlay.ShowLoading();
                await vm.SaveTankDataAsync();
                
                await CustomAlert.ShowSuccessAsync(
                    $"Tanque #{vm.Number} registrado exitosamente:\n\n" +
                    $"• Compartimientos: {vm.Compartment}\n" +
                    $"• Capacidad: {vm.Ability:F2} L\n" +
                    $"• Stock: {(vm.Stock?.ToString("F2") ?? "No especificado")} L",
                    "Tanque Registrado");

                // Clear form fields after successful submission
                Number = string.Empty;
                Compartment = null;
                Ability = null;
                Stock = null;
            
            }
            catch (Exception ex)
            {
                await CustomAlert.ShowErrorAsync($"Error al guardar el tanque:\n\n{ex.Message}", "Error del Sistema");
            }
            finally
            {
                LoadingOverlay.HideLoading();

                

                // Re-enable button
                if (sender is Button button)
                {
                    button.IsEnabled = true;
                    button.Text = _tankService.SendData; // Restore original text from translations
                }
            }
        }
        else
        {
            await CustomAlert.ShowErrorAsync("Error interno del sistema. Por favor, intente nuevamente", "Error de Contexto");
        }
    }

    public string Number
    {
        get => _tankService.Number;
        set
        {
            _tankService.Number = value;
            OnPropertyChanged();
        }
    }
    
    public int? Compartment
    {
        get => _tankService.Compartment;
        set
        {
            _tankService.Compartment = value;
            OnPropertyChanged();
        }
    }

    public double? Ability
    {
        get => _tankService.Ability;
        set
        {
            _tankService.Ability = value;
            OnPropertyChanged();
        }
    }
    
    public double? Stock
    {
        get => _tankService.Stock;
        set
        {
            _tankService.Stock = value;
            OnPropertyChanged();
        }
    }
}