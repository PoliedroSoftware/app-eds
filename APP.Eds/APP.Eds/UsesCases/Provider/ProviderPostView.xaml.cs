using System.Xml.Linq;
using APP.Eds.Services.Provider;
using APP.Eds.Components.PopUp;

namespace APP.Eds.UsesCases.Provider;

public partial class ProviderPostView : ContentPage
{
    private ProviderService _providerService;
    
    public ProviderPostView()
    {
        InitializeComponent();
        _providerService = new ProviderService();
        BindingContext = _providerService;
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        try
        {
            // Disable button to prevent multiple submissions
            if (sender is Button button)
            {
                button.IsEnabled = false;
                button.Text = "Guardando...";
            }

            // Enhanced validation with professional alerts
            if (string.IsNullOrWhiteSpace(_providerService.Name))
            {
                await CustomAlert.ShowErrorAsync("El nombre del proveedor es obligatorio para el registro", "Nombre Requerido");
                return;
            }

            if (_providerService.Name.Length < 3)
            {
                await CustomAlert.ShowErrorAsync("El nombre del proveedor debe tener al menos 3 caracteres para ser v�lido", "Nombre Muy Corto");
                return;
            }

            if (_providerService.Name.Length > 100)
            {
                await CustomAlert.ShowErrorAsync("El nombre del proveedor no puede exceder los 100 caracteres", "Nombre Muy Largo");
                return;
            }

            LoadingOverlay.ShowLoading();
            await _providerService.SaveProviderDataAsync();

            // Refresh the provider list after successful save
            await _providerService.GetProvidersAsync();

            // Clear form field after successful submission
            Name = string.Empty;
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error al guardar el proveedor:\n\n{ex.Message}", "Error del Sistema");
        }
        finally
        {
            LoadingOverlay.HideLoading();

            // Re-enable button
            if (sender is Button button)
            {
                button.IsEnabled = true;
                button.Text = "?? Guardar Proveedor";
            }
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            LoadingOverlay.ShowLoading();
            await _providerService.GetProvidersAsync();
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error al cargar la lista de proveedores:\n\n{ex.Message}", "Error de Carga");
        }
        finally
        {
            LoadingOverlay.HideLoading();
        }
    }

    public string Name
    {
        get => _providerService.Name;
        set
        {
            _providerService.Name = value;
            OnPropertyChanged();
        }
    }
}