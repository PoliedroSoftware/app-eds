using APP.Eds.Services.EdsTank;
using APP.Eds.Components.PopUp;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace APP.Eds.UsesCases.EdsTank;

public partial class EdsTankPostView : ContentPage, INotifyPropertyChanged
{
    private EdsTankService _edsTankService;

    public EdsTankPostView()
    {
        InitializeComponent();
        _edsTankService = new EdsTankService();
        BindingContext = _edsTankService;
        
        // Configurar eventos de selección
        _edsTankService.PropertyChanged += OnServicePropertyChanged;
    }

    private void OnServicePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // Actualizar propiedades de la UI cuando cambien las selecciones
        if (e.PropertyName == nameof(_edsTankService.SelectEds) || 
            e.PropertyName == nameof(_edsTankService.SelectTank))
        {
            OnPropertyChanged(nameof(ShowAssignmentSummary));
            OnPropertyChanged(nameof(CanAssignTank));
        }
    }

    // Propiedad para mostrar el resumen de asignación
    public bool ShowAssignmentSummary => 
        _edsTankService?.SelectEds != null && _edsTankService?.SelectTank != null;

    // Propiedad para habilitar el botón de asignación
    public bool CanAssignTank => 
        _edsTankService?.SelectEds != null && _edsTankService?.SelectTank != null;

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
                await CustomAlert.ShowWarningAsync(
                    "Por favor seleccione una estación de servicio (EDS) de la lista disponible",
                    "EDS No Seleccionada");
                return;
            }

            if (_edsTankService.SelectTank == null)
            {
                await CustomAlert.ShowWarningAsync(
                    "Por favor seleccione un tanque de la lista de tanques disponibles",
                    "Tanque No Seleccionado");
                return;
            }

            // Verificar si ya existe una asignación
            bool hasExistingAssignment = await _edsTankService.CheckExistingAssignmentAsync();
            if (hasExistingAssignment)
            {
                bool overwrite = await CustomAlert.ShowConfirmAsync(
                    $"El tanque #{_edsTankService.SelectTank.Number} ya está asignado a otra EDS.\n\n¿Desea reasignarlo a '{_edsTankService.SelectEds.Name}'?",
                    "Reasignación de Tanque",
                    "Reasignar",
                    "Cancelar");
                
                if (!overwrite) return;
            }

            // Show detailed confirmation
            string edsName = _edsTankService.SelectEds.Name ?? "N/A";
            string tankNumber = _edsTankService.SelectTank.Number ?? "N/A";
            double capacity = _edsTankService.SelectTank.Ability;
            int compartments = _edsTankService.SelectTank.Compartment;
            
            bool confirm = await CustomAlert.ShowConfirmAsync(
                $"¿Confirma la asignación del siguiente tanque?\n\n" +
                $"EDS: {edsName}\n" +
                $"Tanque: #{tankNumber}\n" +
                $"Capacidad: {capacity:N0} litros\n" +
                $"Compartimientos: {compartments}\n\n" +
                $"Esta asignación habilitará el tanque para operaciones en la estación.",
                "Confirmar Asignación",
                "Asignar",
                "Cancelar");

            if (!confirm) return;

            LoadingOverlay.IsVisible = true;
            await _edsTankService.SaveEdsTankDataAsync();
            
            await CustomAlert.ShowSuccessAsync(
                $"Asignación completada exitosamente\n\n" +
                $"Estación: {edsName}\n" +
                $"Tanque: #{tankNumber}\n" +
                $"Capacidad: {capacity:N0} L\n" +
                $"Compartimientos: {compartments}\n\n" +
                $"El tanque está ahora disponible para operaciones en esta estación de servicio.",
                "Tanque Asignado");
                
            // Limpiar selecciones después del éxito
            await ClearSelections();
            
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync(
                $"Error al realizar la asignación del tanque:\n\n{ex.Message}\n\nPor favor, verifique su conexión e intente nuevamente.",
                "Error de Asignación");
        }
        finally
        {
            LoadingOverlay.IsVisible = false;
            
            // Re-enable button
            if (sender is Button button)
            {
                button.IsEnabled = true;
                button.Text = "Asignar Tanque a EDS";
            }
        }
    }

    private async void OnClearClicked(object sender, EventArgs e)
    {
        try
        {
            bool confirm = await CustomAlert.ShowConfirmAsync(
                "¿Está seguro de que desea limpiar las selecciones actuales?",
                "Limpiar Selecciones",
                "Limpiar",
                "Cancelar");
                
            if (confirm)
            {
                await ClearSelections();
                await CustomAlert.ShowInfoAsync("Las selecciones han sido limpiadas correctamente", "Selecciones Limpiadas");
            }
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error al limpiar las selecciones:\n\n{ex.Message}", "Error");
        }
    }

    private async void OnViewAssignmentsClicked(object sender, EventArgs e)
    {
        try
        {
            LoadingOverlay.IsVisible = true;
            
            // Cargar y mostrar asignaciones actuales
            var assignments = await _edsTankService.GetCurrentAssignmentsAsync();
            
            if (assignments?.Any() == true)
            {
                string assignmentsList = string.Join("\n", assignments.Select(a => 
                    $"{a.EdsName} - Tanque #{a.TankNumber}"));
                    
                await CustomAlert.ShowInfoAsync(
                    $"Asignaciones actuales de tanques:\n\n{assignmentsList}",
                    "Asignaciones Actuales");
            }
            else
            {
                await CustomAlert.ShowInfoAsync(
                    "No hay asignaciones de tanques registradas en el sistema",
                    "Sin Asignaciones");
            }
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error al cargar las asignaciones:\n\n{ex.Message}", "Error de Carga");
        }
        finally
        {
            LoadingOverlay.IsVisible = false;
        }
    }

    private async Task ClearSelections()
    {
        _edsTankService.SelectEds = null;
        _edsTankService.SelectTank = null;
        
        // Actualizar propiedades de la UI
        OnPropertyChanged(nameof(ShowAssignmentSummary));
        OnPropertyChanged(nameof(CanAssignTank));
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        try
        {
            LoadingOverlay.IsVisible = true;
            
            // Cargar datos frescos al aparecer la página
            await _edsTankService.RefreshDataAsync();
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error al cargar los datos:\n\n{ex.Message}", "Error de Carga");
        }
        finally
        {
            LoadingOverlay.IsVisible = false;
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        // Desconectar eventos para evitar memory leaks
        if (_edsTankService != null)
        {
            _edsTankService.PropertyChanged -= OnServicePropertyChanged;
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
