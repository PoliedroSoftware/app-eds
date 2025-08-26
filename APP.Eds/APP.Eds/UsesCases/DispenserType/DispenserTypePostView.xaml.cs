using APP.Eds.Services.DispenserType;
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

            // Validate required field
            if (string.IsNullOrWhiteSpace(_dispenserTypeService.Description))
            {
                await DisplayAlert("Error", "Por favor, ingrese la descripción del tipo de dispensador", "OK");
                return;
            }

            // Additional validation for minimum length
            if (_dispenserTypeService.Description.Length < 3)
            {
                await DisplayAlert("Error", "La descripción debe tener al menos 3 caracteres", "OK");
                return;
            }

            LoadingOverlay.ShowLoading();
            await _dispenserTypeService.SaveDispenserTypeDataAsync();
            
            // Clear form after successful save
            Description = string.Empty;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al guardar: {ex.Message}", "OK");
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