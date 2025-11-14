using APP.Eds.Components.PopUp;
using APP.Eds.Services.CompartimentCapacity;

namespace APP.Eds.UsesCases.CompartimentCapacity;

public partial class CompartimentCapacityPostView : ContentPage
{
    private CompartimentCapacityService _compartimentCapacityService;

    public CompartimentCapacityPostView()
    {
        InitializeComponent();
        _compartimentCapacityService = new CompartimentCapacityService();
        BindingContext = _compartimentCapacityService;
    }

    private async void Button_Clicked(object sender, EventArgs e)
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
            if (_compartimentCapacityService.SelectedTank == null)
            {
                await CustomAlert.ShowErrorAsync("Debe seleccionar un tanque para asignar la capacidad", "Tanque Requerido");
                return;
            }

            if (_compartimentCapacityService.SelectCompartiment == null)
            {
                await CustomAlert.ShowErrorAsync("Debe seleccionar un compartimento para configurar", "Compartimento Requerido");
                return;
            }

            if (_compartimentCapacityService.Default <= 0)
            {
                await CustomAlert.ShowErrorAsync("Debe ingresar una capacidad v�lida mayor que 0", "Capacidad Inv�lida");
                return;
            }

            if (_compartimentCapacityService.Default > 100000)
            {
                await CustomAlert.ShowErrorAsync("La capacidad no puede exceder 100,000 litros por motivos de seguridad", "Capacidad Excesiva");
                return;
            }

            // Show professional confirmation dialog
            string tankNumber = _compartimentCapacityService.SelectedTank.Number ?? "N/A";
            int compartmentNumber = _compartimentCapacityService.SelectCompartiment.Number;
            byte capacity = (byte)_compartimentCapacityService.Default;

            bool confirm = await CustomAlert.ShowConfirmAsync(
                $"¿Confirma que desea asignar {capacity} L de capacidad al compartimento #{compartmentNumber} del tanque {tankNumber}?\n\nEsta configuración afectará las operaciones del compartimento.", 
                "Confirmar Configuración", 
                "Confirmar", 
                "Cancelar");

            if (!confirm) return;

            LoadingOverlay.ShowLoading();
            await _compartimentCapacityService.SaveCompartimentCapacityDataAsync();

            // Clear form fields after successful submission
            _compartimentCapacityService.SelectedTank = null;
            _compartimentCapacityService.SelectCompartiment = null;
            Default = 0;
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error al guardar la configuraci�n de capacidad:\n\n{ex.Message}", "Error del Sistema");
        }
        finally
        {
            LoadingOverlay.HideLoading();

            // Re-enable button
            if (sender is Button button)
            {
                button.IsEnabled = true;
                button.Text = "?? Enviar Datos"; // Restore original text
            }
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            LoadingOverlay?.ShowLoading();
            await _compartimentCapacityService.GetAllTankDataAsync();
            await _compartimentCapacityService.GetAllCompartimentDataAsync();
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error al cargar los datos iniciales:\n\n{ex.Message}", "Error de Carga");
        }
        finally
        {
            LoadingOverlay?.HideLoading();
        }
    }

    public byte Default
    {
        get => _compartimentCapacityService.Default ?? 0;
        set
        {
            _compartimentCapacityService.Default = value;
            OnPropertyChanged();
        }
    }
}
