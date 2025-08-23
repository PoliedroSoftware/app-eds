using System.Xml.Linq;
using APP.Eds.Services.Provider;

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

            // Validate required fields
            if (string.IsNullOrWhiteSpace(_providerService.Name))
            {
                await DisplayAlert("Error", "Por favor ingrese el nombre del proveedor", "OK");
                return;
            }

            if (_providerService.Name.Length < 3)
            {
                await DisplayAlert("Error", "El nombre del proveedor debe tener al menos 3 caracteres", "OK");
                return;
            }

            LoadingOverlay.ShowLoading();
            await _providerService.SaveProviderDataAsync();

            // Refresh the provider list after successful save
            await _providerService.GetProvidersAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al guardar el proveedor: {ex.Message}", "OK");
        }
        finally
        {
            LoadingOverlay.HideLoading();

            // Clear form field after successful submission
            Name = string.Empty;

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
            await DisplayAlert("Error", $"Error cargando proveedores: {ex.Message}", "OK");
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