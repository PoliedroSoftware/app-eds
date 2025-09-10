using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using APP.Eds.Models.Business;
using APP.Eds.Services.Business;

namespace APP.Eds.UsesCases.Business;

public class BusinessListViewModel : INotifyPropertyChanged
{
    private readonly BusinessService _businessService;
    private bool _isRefreshing;

    public ObservableCollection<BusinessModel> BusinessList { get; set; } = new();

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            _isRefreshing = value;
            OnPropertyChanged();
        }
    }

    // Commands
    public ICommand RefreshCommand { get; }
    public ICommand CreateBusinessCommand { get; }
    public ICommand ViewBusinessDetailCommand { get; }
    public ICommand EditBusinessCommand { get; }
    public ICommand BusinessOptionsCommand { get; }

    public BusinessListViewModel()
    {
        _businessService = new BusinessService();
        
        // Initialize commands
        RefreshCommand = new Command(async () => await RefreshBusinessesAsync());
        CreateBusinessCommand = new Command(async () => await CreateNewBusinessAsync());
        ViewBusinessDetailCommand = new Command<BusinessModel>(async (business) => await ViewBusinessDetailAsync(business));
        EditBusinessCommand = new Command<BusinessModel>(async (business) => await EditBusinessAsync(business));
        BusinessOptionsCommand = new Command<BusinessModel>(async (business) => await ShowBusinessOptionsAsync(business));
    }

    public async Task LoadBusinessesAsync()
    {
        try
        {
            await _businessService.GetBusinessesAsync(pageNumber: 1, pageSize: 100);
            
            BusinessList.Clear();
            foreach (var business in _businessService.BusinessList)
            {
                BusinessList.Add(business);
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error al cargar la lista de negocios:\n\n{ex.Message}", "OK");
        }
    }

    private async Task RefreshBusinessesAsync()
    {
        IsRefreshing = true;
        try
        {
            await LoadBusinessesAsync();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private async Task CreateNewBusinessAsync()
    {
        try
        {
            if (Application.Current?.MainPage is NavigationPage navPage)
            {
                await navPage.PushAsync(new BusinessPostView());
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error al abrir el formulario de creación:\n\n{ex.Message}", "OK");
        }
    }

    private async Task ViewBusinessDetailAsync(BusinessModel business)
    {
        if (business == null) return;

        try
        {
            // For now, show a detailed info popup. Later can be expanded to a dedicated detail page
            var message = $"Información del Negocio:\n\n" +
                         $"• ID: {business.IdBusiness}\n" +
                         $"• Nombre: {business.Name}\n" +
                         $"• Estado: Activo\n\n" +
                         $"Acciones disponibles:\n" +
                         $"• Editar información\n" +
                         $"• Ver estadísticas\n" +
                         $"• Gestionar EDS asociadas";

            await Application.Current.MainPage.DisplayAlert($"Detalles - {business.Name}", message, "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error al mostrar los detalles del negocio:\n\n{ex.Message}", "OK");
        }
    }

    private async Task EditBusinessAsync(BusinessModel business)
    {
        if (business == null) return;

        try
        {
            // Load business details and navigate to edit
            await _businessService.GetByIdBusinessDataAsync(business.IdBusiness);
            
            if (Application.Current?.MainPage is NavigationPage navPage)
            {
                var editView = new BusinessPostView();
                // Pass the business data to the edit view (this would require updating BusinessPostView)
                await navPage.PushAsync(editView);
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error al cargar los datos del negocio para edición:\n\n{ex.Message}", "OK");
        }
    }

    private async Task ShowBusinessOptionsAsync(BusinessModel business)
    {
        if (business == null) return;

        try
        {
            var action = await Application.Current.MainPage.DisplayActionSheet(
                $"Opciones para {business.Name}",
                "Cancelar",
                "Eliminar Negocio",
                "Ver Detalles Completos",
                "Editar Información",
                "Gestionar EDS",
                "Ver Estadísticas",
                "Exportar Datos");

            switch (action)
            {
                case "Ver Detalles Completos":
                    await ViewBusinessDetailAsync(business);
                    break;
                case "Editar Información":
                    await EditBusinessAsync(business);
                    break;
                case "Gestionar EDS":
                    await ManageEdsAsync(business);
                    break;
                case "Ver Estadísticas":
                    await ShowStatisticsAsync(business);
                    break;
                case "Exportar Datos":
                    await ExportDataAsync(business);
                    break;
                case "Eliminar Negocio":
                    await DeleteBusinessAsync(business);
                    break;
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error al mostrar las opciones:\n\n{ex.Message}", "OK");
        }
    }

    private async Task ManageEdsAsync(BusinessModel business)
    {
        await Application.Current.MainPage.DisplayAlert("Gestión de EDS", 
            $"Funcionalidad en desarrollo.\n\nEn esta sección podrás gestionar todas las estaciones de servicio (EDS) asociadas a '{business.Name}'.", 
            "OK");
    }

    private async Task ShowStatisticsAsync(BusinessModel business)
    {
        // Sample statistics - in a real app, this would fetch real data
        var stats = $"Estadísticas de {business.Name}:\n\n" +
                   $"• Total EDS: En desarrollo\n" +
                   $"• Ventas del mes: En desarrollo\n" +
                   $"• Isleros activos: En desarrollo\n" +
                   $"• Última actividad: En desarrollo\n\n" +
                   $"Nota: Las estadísticas detalladas estarán disponibles en futuras actualizaciones.";

        await Application.Current.MainPage.DisplayAlert($"Estadísticas - {business.Name}", stats, "OK");
    }

    private async Task ExportDataAsync(BusinessModel business)
    {
        await Application.Current.MainPage.DisplayAlert("Exportar Datos", 
            $"Funcionalidad de exportación en desarrollo.\n\nPodrás exportar todos los datos de '{business.Name}' en formatos Excel, PDF y CSV.", 
            "OK");
    }

    private async Task DeleteBusinessAsync(BusinessModel business)
    {
        try
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Confirmar Eliminación",
                $"¿Está seguro de que desea eliminar el negocio '{business.Name}'?\n\n" +
                $"Esta acción eliminará:\n" +
                $"• Todas las EDS asociadas\n" +
                $"• Todos los registros de ventas\n" +
                $"• Todos los datos relacionados\n\n" +
                $"?? ESTA ACCIÓN NO SE PUEDE DESHACER",
                "Eliminar",
                "Cancelar");

            if (confirm)
            {
                // For now, show that this is in development
                await Application.Current.MainPage.DisplayAlert("Eliminación en Desarrollo", 
                    "La funcionalidad de eliminación está en desarrollo.\n\n" +
                    "Esta función incluirá validaciones adicionales y respaldos de seguridad antes de permitir la eliminación.", 
                    "OK");

                // TODO: Implement actual deletion when API is ready
                // await _businessService.DeleteBusinessAsync(business.IdBusiness);
                // await LoadBusinessesAsync(); // Refresh the list
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error al intentar eliminar el negocio:\n\n{ex.Message}", "OK");
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}