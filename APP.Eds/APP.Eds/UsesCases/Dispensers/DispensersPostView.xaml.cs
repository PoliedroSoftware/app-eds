using APP.Eds.Services.Dispensers;
using APP.Eds.UsesCases.DispenserType;
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
            await DisplayAlert("Error", $"Error al cargar datos: {ex.Message}", "OK");
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
            // Enhanced validation
            if (string.IsNullOrWhiteSpace(_dispensersService.Code))
            {
                await DisplayAlert("Error", "Por favor, ingrese el codigo del dispensador", "OK");
                return;
            }

            if (_dispensersService.Number <= 0)
            {
                await DisplayAlert("Error", "Por favor, ingrese un numero de dispensador valido mayor a 0", "OK");
                return;
            }

            if (_dispensersService.SelectedDispenserType == null)
            {
                await DisplayAlert("Error", "Por favor, seleccione el tipo de dispensador", "OK");
                return;
            }

            if (_dispensersService.SelectedEds == null)
            {
                await DisplayAlert("Error", "Por favor, seleccione la estacion de servicio (EDS)", "OK");
                return;
            }

            if (_dispensersService.SelectedIsland == null)
            {
                await DisplayAlert("Error", "Por favor, seleccione la isla de combustible", "OK");
                return;
            }

            if (_dispensersService.HoseNumber <= 0)
            {
                await DisplayAlert("Error", "Por favor, ingrese un numero de mangueras valido mayor a 0", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(_dispensersService.SelectedStatus))
            {
                await DisplayAlert("Error", "Por favor, seleccione el estado operativo del dispensador", "OK");
                return;
            }

            LoadingOverlay?.ShowLoading();
            
            await _dispensersService.SaveDispensersDataAsync();
            
            // Clear form after successful save
            ClearForm();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al guardar: {ex.Message}", "OK");
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