using APP.Eds.Services.DispenserType;
using APP.Eds.Components.PopUp;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace APP.Eds.UsesCases.DispenserType;

public partial class DispenserTypePostView : ContentPage, INotifyPropertyChanged
{
    private DispenserTypeService _dispenserTypeService;
    
    public DispenserTypePostView()
    {
        InitializeComponent();
        _dispenserTypeService = new DispenserTypeService();
        BindingContext = _dispenserTypeService;
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        var button = sender as Button;
        try
        {
            // Disable button to prevent multiple submissions
            if (button != null)
            {
                button.IsEnabled = false;
                button.Text = "Guardando...";
            }

            // Enhanced validation with professional alerts
            if (string.IsNullOrWhiteSpace(_dispenserTypeService.Description))
            {
                await CustomAlert.ShowErrorAsync("La descripción del tipo de dispensador es obligatoria para el registro", "Descripción Requerida");
                return;
            }

            // Additional validation for minimum length
            if (_dispenserTypeService.Description.Length < 3)
            {
                await CustomAlert.ShowErrorAsync("La descripción debe tener al menos 3 caracteres para ser válida", "Descripción Muy Corta");
                return;
            }

            if (_dispenserTypeService.Description.Length > 100)
            {
                await CustomAlert.ShowErrorAsync("La descripción no puede exceder 100 caracteres", "Descripción Muy Larga");
                return;
            }

            // Check for special characters or inappropriate content
            if (_dispenserTypeService.Description.Trim() != _dispenserTypeService.Description)
            {
                await CustomAlert.ShowWarningAsync("La descripción contiene espacios al inicio o final que serán removidos automáticamente", "Espacios Detectados");
                _dispenserTypeService.Description = _dispenserTypeService.Description.Trim();
            }

            LoadingOverlay.ShowLoading();
            await _dispenserTypeService.SaveDispenserTypeDataAsync();
            
            // Clear form after successful save
            Description = string.Empty;
            
            await CustomAlert.ShowSuccessAsync($"El tipo de dispensador '{_dispenserTypeService.Description}' ha sido creado exitosamente", "Tipo Creado");
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error al guardar el tipo de dispensador:\n\n{ex.Message}", "Error del Sistema");
        }
        finally
        {
            LoadingOverlay.HideLoading();
            
            // Re-enable and restore button
            if (button != null)
            {
                button.IsEnabled = true;
                button.Text = "?? Crear Tipo de Dispensador";
            }
        }
    }

    public string Description
    {
        get => _dispenserTypeService.Description;
        set
        {
            _dispenserTypeService.Description = value;
            OnPropertyChanged();
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}