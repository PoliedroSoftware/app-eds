using APP.Eds.Services.ProductType;
using APP.Eds.Components.PopUp;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace APP.Eds.UsesCases.ProductType;

public partial class ProductTypePostView : ContentPage, INotifyPropertyChanged
{
    private ProductTypeService _productTypeService;
    
    public ProductTypePostView()
    {
        InitializeComponent();
        _productTypeService = new ProductTypeService();
        BindingContext = _productTypeService;
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
            if (string.IsNullOrWhiteSpace(_productTypeService.Description))
            {
                await CustomAlert.ShowErrorAsync("La descripción del tipo de producto es obligatoria para el registro", "Descripción Requerida");
                return;
            }

            if (_productTypeService.Description.Length < 3)
            {
                await CustomAlert.ShowErrorAsync("La descripción debe tener al menos 3 caracteres para ser válida", "Descripción Muy Corta");
                return;
            }

            if (_productTypeService.Description.Length > 100)
            {
                await CustomAlert.ShowErrorAsync("La descripción no puede exceder 100 caracteres", "Descripción Muy Larga");
                return;
            }

            // Clean up description
            string originalDescription = _productTypeService.Description;
            _productTypeService.Description = _productTypeService.Description.Trim();
            
            if (originalDescription != _productTypeService.Description)
            {
                await CustomAlert.ShowInfoAsync("Los espacios extra al inicio y final han sido removidos automáticamente", "Descripción Limpiada");
            }

            LoadingOverlay.ShowLoading();
            await _productTypeService.SaveProductTypeDataAsync();
            
            // Clear form after successful save
            Description = string.Empty;
            
            await CustomAlert.ShowSuccessAsync($"El tipo de producto '{_productTypeService.Description}' ha sido creado exitosamente", "Tipo Creado");
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error al guardar el tipo de producto:\n\n{ex.Message}", "Error del Sistema");
        }
        finally
        {
            LoadingOverlay.HideLoading();
            
            // Re-enable button
            if (button != null)
            {
                button.IsEnabled = true;
                button.Text = "?? Crear Tipo de Producto";
            }
        }
    }

    public string Description
    {
        get => _productTypeService.Description;
        set
        {
            _productTypeService.Description = value;
            OnPropertyChanged();
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}