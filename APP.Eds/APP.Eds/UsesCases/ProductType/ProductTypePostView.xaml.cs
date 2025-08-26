using APP.Eds.Services.ProductType;
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
            }

            // Validate required field
            if (string.IsNullOrWhiteSpace(_productTypeService.Description))
            {
                await DisplayAlert("Error", "Por favor, ingrese la descripción del tipo de producto", "OK");
                return;
            }

            LoadingOverlay.ShowLoading();
            await _productTypeService.SaveProductTypeDataAsync();
            
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
            
            // Re-enable button
            if (button != null)
            {
                button.IsEnabled = true;
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