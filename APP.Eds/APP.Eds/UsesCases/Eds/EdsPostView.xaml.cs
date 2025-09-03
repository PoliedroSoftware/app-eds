using System.Net;
using APP.Eds.Services.Eds;
using APP.Eds.Controls;
using APP.Eds.Components.PopUp;
using APP.Eds.UsesCases.LoadingView; // Added for LoadingOverlay

namespace APP.Eds.UsesCases.Eds;

public partial class EdsPostView : ContentPage
{
    private EdsService _edsService;
    
    public EdsPostView()
    {
        InitializeComponent();
        _edsService = new EdsService();
        BindingContext = _edsService;
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        if (BindingContext is EdsService vm)
        {
            try
            {
                // Disable button to prevent multiple submissions
                if (sender is HoverButton hoverButton)
                {
                    hoverButton.IsEnabled = false;
                }

                // Enhanced validation with professional alerts
                if (string.IsNullOrWhiteSpace(_edsService.Name))
                {
                    await CustomAlert.ShowErrorAsync("El nombre de la estaci�n de servicio es obligatorio", "Nombre Requerido");
                    return;
                }

                if (string.IsNullOrWhiteSpace(_edsService.Nit))
                {
                    await CustomAlert.ShowErrorAsync("El NIT de la estaci�n es obligatorio para identificaci�n fiscal", "NIT Requerido");
                    return;
                }

                if (string.IsNullOrWhiteSpace(_edsService.Sicom))
                {
                    await CustomAlert.ShowErrorAsync("El c�digo SICOM es obligatorio para el registro ante autoridades", "SICOM Requerido");
                    return;
                }

                if (string.IsNullOrWhiteSpace(_edsService.Address))
                {
                    await CustomAlert.ShowErrorAsync("La direcci�n f�sica de la estaci�n es obligatoria", "Direcci�n Requerida");
                    return;
                }

                if (_edsService.SelectedBusiness is null)
                {
                    await CustomAlert.ShowErrorAsync("Debe seleccionar el negocio al cual pertenece esta estaci�n", "Negocio Requerido");
                    return;
                }

                // Additional validation
                if (_edsService.Name.Length < 3)
                {
                    await CustomAlert.ShowErrorAsync("El nombre debe tener al menos 3 caracteres", "Nombre Muy Corto");
                    return;
                }

                if (_edsService.Nit.Length < 8)
                {
                    await CustomAlert.ShowErrorAsync("El NIT debe tener al menos 8 caracteres", "NIT Inv�lido");
                    return;
                }

                if (_edsService.Sicom.Length < 4)
                {
                    await CustomAlert.ShowErrorAsync("El c�digo SICOM debe tener al menos 4 caracteres", "SICOM Inv�lido");
                    return;
                }

                if (_edsService.Address.Length < 10)
                {
                    await CustomAlert.ShowErrorAsync("La direcci�n debe ser m�s espec�fica (m�nimo 10 caracteres)", "Direcci�n Muy Corta");
                    return;
                }

                LoadingOverlay.IsVisible = true;
                var selectedId = vm.SelectedBusiness.IdBusiness;
                await vm.SaveEdsDataAsync();
                
                await CustomAlert.ShowSuccessAsync(
                    $"Estaci�n de servicio registrada exitosamente:\n\n" +
                    $"� Nombre: {_edsService.Name}\n" +
                    $"� NIT: {_edsService.Nit}\n" +
                    $"� SICOM: {_edsService.Sicom}\n" +
                    $"� Direcci�n: {_edsService.Address}\n" +
                    $"� Negocio: {_edsService.SelectedBusiness.Name}",
                    "EDS Registrada");
            }
            catch (Exception ex)
            {
                await CustomAlert.ShowErrorAsync($"Error al registrar la estaci�n de servicio:\n\n{ex.Message}", "Error del Sistema");
            }
            finally
            {
                LoadingOverlay.IsVisible = false;

                // Clear form fields after successful submission
                _edsService.Name = string.Empty;
                _edsService.Nit = string.Empty;
                _edsService.Address = string.Empty;
                _edsService.Sicom = string.Empty;
                _edsService.SelectedBusiness = null;

                // Re-enable button
                if (sender is HoverButton hoverButton)
                {
                    hoverButton.IsEnabled = true;
                }
            }  
        }
    }
}