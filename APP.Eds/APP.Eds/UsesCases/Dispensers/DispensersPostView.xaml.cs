using APP.Eds.Services.Dispensers;
using APP.Eds.UsesCases.DispenserType;
using APP.Eds.Components.PopUp;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace APP.Eds.UsesCases.Dispensers;

public partial class DispensersPostView : ContentPage, INotifyPropertyChanged
{
    private DispensersService _dispensersService;

    public DispensersPostView()
    {
        InitializeComponent();
        _dispensersService = new DispensersService();
        BindingContext = _dispensersService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            LoadingOverlay?.ShowLoading();
            await _dispensersService.InitializeAsync();
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error al cargar los datos iniciales:\n\n{ex.Message}", "Error de Inicialización");
        }
        finally
        {
            LoadingOverlay?.HideLoading();
        }
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (button != null)
        {
            button.IsEnabled = false;
        }

        try
        {
            // Enhanced validation with professional alerts
            if (string.IsNullOrWhiteSpace(_dispensersService.Code))
            {
                await CustomAlert.ShowErrorAsync("El código del dispensador es obligatorio para el registro", "Código Requerido");
                return;
            }

            if (_dispensersService.Number <= 0)
            {
                await CustomAlert.ShowErrorAsync("Debe ingresar un número de dispensador válido (mayor que 0)", "Número Inválido");
                return;
            }

            if (_dispensersService.SelectedDispenserType == null)
            {
                await CustomAlert.ShowErrorAsync("Debe seleccionar el tipo de dispensador correspondiente", "Tipo Requerido");
                return;
            }

            if (_dispensersService.SelectedEds == null)
            {
                await CustomAlert.ShowErrorAsync("Debe seleccionar la estación de servicio (EDS) donde se ubicará el dispensador", "EDS Requerida");
                return;
            }

            if (_dispensersService.SelectedIsland == null)
            {
                await CustomAlert.ShowErrorAsync("Debe seleccionar la isla de combustible correspondiente", "Isla Requerida");
                return;
            }

            if (_dispensersService.HoseNumber <= 0)
            {
                await CustomAlert.ShowErrorAsync("Debe ingresar un número válido de mangueras (mayor que 0)", "Número de Mangueras Inválido");
                return;
            }

            if (string.IsNullOrWhiteSpace(_dispensersService.SelectedStatus))
            {
                await CustomAlert.ShowErrorAsync("Debe seleccionar el estado operativo del dispensador", "Estado Requerido");
                return;
            }

            LoadingOverlay?.ShowLoading();
            
            await _dispensersService.SaveDispensersDataAsync();
            
            // Clear form after successful save
            ClearForm();
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error al guardar el dispensador:\n\n{ex.Message}", "Error del Sistema");
        }
        finally
        {
            LoadingOverlay?.HideLoading();
            if (button != null)
            {
                button.IsEnabled = true;
            }
        }
    }

    private void OnAddDispenserTypeClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new DispenserTypePostView());
    }

    private void ClearForm()
    {
        if (_dispensersService != null)
        {
            _dispensersService.Code = string.Empty;
            _dispensersService.Number = 0;
            _dispensersService.SelectedDispenserType = null;
            _dispensersService.SelectedEds = null;
            _dispensersService.SelectedIsland = null;
            _dispensersService.HoseNumber = 0;
            _dispensersService.SelectedStatus = null;
            _dispensersService.IsActive = true; // Default to active
            _dispensersService.RequiresMaintenance = false;
            _dispensersService.DispenserNotes = string.Empty;
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}