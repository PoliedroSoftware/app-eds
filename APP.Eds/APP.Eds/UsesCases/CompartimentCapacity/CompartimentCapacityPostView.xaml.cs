using APP.Eds.Components.PopUp;
using APP.Eds.Services.CompartimentCapacity;

namespace APP.Eds.UsesCases.CompartimentCapacity;

public partial class CompartimentCapacityPostView : ContentPage
{
    private readonly CompartimentCapacityService _compartimentCapacityService;

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
            if (sender is Button button)
            {
                button.IsEnabled = false;
                button.Text = "Enviando...";
            }

            if (_compartimentCapacityService.SelectCapacity == null)
            {
                await CustomAlert.ShowErrorAsync("Debe seleccionar un tanque para asignar la capacidad", "Tanque Requerido");
                return;
            }

            if (_compartimentCapacityService.SelectCompartiment == null)
            {
                await CustomAlert.ShowErrorAsync("Debe seleccionar un compartimento para configurar", "Compartimento Requerido");
                return;
            }

            if (!_compartimentCapacityService.Default.HasValue || _compartimentCapacityService.Default <= 0)
            {
                await CustomAlert.ShowErrorAsync("Debe ingresar una capacidad válida mayor que 0", "Capacidad Inválida");
                return;
            }

            if (_compartimentCapacityService.Default > 100000)
            {
                await CustomAlert.ShowErrorAsync("La capacidad no puede exceder 100,000 litros por motivos de seguridad", "Capacidad Excesiva");
                return;
            }

            string tankCode = _compartimentCapacityService.SelectCapacity.Code ?? "N/A";
            int compartmentNumber = _compartimentCapacityService.SelectCompartiment.Number;
            double capacityValue = _compartimentCapacityService.Default.Value;

            bool confirm = await CustomAlert.ShowConfirmAsync(
                $"¿Confirma que desea asignar {capacityValue:N2} L de capacidad al compartimento #{compartmentNumber} del tanque {tankCode}?\n\nEsta configuración afectará las operaciones del compartimento.",
                "Confirmar Configuración",
                "Confirmar",
                "Cancelar");

            if (!confirm) return;

            LoadingOverlay.ShowLoading();
            await _compartimentCapacityService.SaveCompartimentCapacityDataAsync();

            // Reset
            _compartimentCapacityService.SelectCapacity = null;
            _compartimentCapacityService.SelectCompartiment = null;
            Default = 0; // ahora double
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error al guardar la configuración de capacidad:\n\n{ex.Message}", "Error del Sistema");
        }
        finally
        {
            LoadingOverlay.HideLoading();
            if (sender is Button button)
            {
                button.IsEnabled = true;
                button.Text = "?? Enviar Datos";
            }
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            LoadingOverlay?.ShowLoading();
            await _compartimentCapacityService.GetAllCapacityDataAsync();
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

    // Cambiado a double (o double?)
    public double Default
    {
        get => _compartimentCapacityService.Default ?? 0;
        set
        {
            _compartimentCapacityService.Default = value;
            OnPropertyChanged();
        }
    }
}
