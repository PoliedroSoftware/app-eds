using APP.Eds.Services.Business;
using APP.Eds.Components.PopUp;

namespace APP.Eds.UsesCases.Business;

public partial class BusinessPostView : ContentPage
{
    private BusinessService _businessService;

    public BusinessPostView()
    {
        InitializeComponent();
        _businessService = new BusinessService();
        BindingContext = _businessService;
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
            if (string.IsNullOrWhiteSpace(_businessService.Name))
            {
                await CustomAlert.ShowErrorAsync("El nombre del negocio es obligatorio para el registro", "Nombre Requerido");
                return;
            }

            if (_businessService.Name.Length < 3)
            {
                await CustomAlert.ShowErrorAsync("El nombre del negocio debe tener al menos 3 caracteres", "Nombre Muy Corto");
                return;
            }

            if (_businessService.Name.Length > 100)
            {
                await CustomAlert.ShowErrorAsync("El nombre del negocio no puede exceder 100 caracteres", "Nombre Muy Largo");
                return;
            }

            // Clean up name
            string originalName = _businessService.Name;
            _businessService.Name = _businessService.Name.Trim();
            
            if (originalName != _businessService.Name)
            {
                await CustomAlert.ShowInfoAsync("Los espacios extra han sido removidos automaticamente del nombre", "Nombre Limpiado");
            }

            LoadingOverlay.ShowLoading();
            await _businessService.SaveBusinessDataAsync();
            
            // Show success message with options
            var action = await Application.Current.MainPage.DisplayActionSheet(
                $"El negocio '{_businessService.Name}' ha sido registrado exitosamente. ¿Que desea hacer ahora?",
                null,
                null,
                "Ver todos los negocios",
                "Crear otro negocio",
                "Continuar configuracion");

            switch (action)
            {
                case "Ver todos los negocios":
                    // Navigate back to business list
                    await Navigation.PopAsync();
                    break;
                case "Crear otro negocio":
                    // Clear form and stay on same page
                    Name = string.Empty;
                    break;
                case "Continuar configuracion":
                    // Navigate to EDS creation or wizard
                    await Navigation.PopAsync(); // Go back to list for now
                    break;
            }
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error al guardar el negocio:\n\n{ex.Message}", "Error del Sistema");
        }
        finally
        {
            LoadingOverlay.HideLoading();
            
            // Re-enable button
            if (sender is Button button)
            {
                button.IsEnabled = true;
                button.Text = "Registrar Negocio";
            }
        }
    }

    private async void OnViewListClicked(object sender, EventArgs e)
    {
        try
        {
            // Navigate to business list
            if (Application.Current?.MainPage is NavigationPage navPage)
            {
                await navPage.PushAsync(new BusinessListView());
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", 
                $"No se pudo abrir el listado de negocios: {ex.Message}", "OK");
        }
    }

    private void OnClearFormClicked(object sender, EventArgs e)
    {
        try
        {
            // Clear the form
            Name = string.Empty;
            
            // Show confirmation
            Application.Current.MainPage.DisplayAlert("Formulario Limpio", 
                "El formulario ha sido limpiado. Puede ingresar un nuevo negocio.", "OK");
        }
        catch (Exception ex)
        {
            Application.Current.MainPage.DisplayAlert("Error", 
                $"Error al limpiar el formulario: {ex.Message}", "OK");
        }
    }

    public string Name
    {
        get => _businessService.Name;
        set
        {
            _businessService.Name = value;
            OnPropertyChanged();
        }
    }
}

