using APP.Eds.Services.ProductCompartiment;
using APP.Eds.Components.PopUp;

namespace APP.Eds.UsesCases.ProductCompartiment;

public partial class ProductCompartimentPostView : ContentPage
{
    private ProductCompartimentService _productCompartimentService;

    public ProductCompartimentPostView()
    {
        InitializeComponent();
        _productCompartimentService = new ProductCompartimentService();
        BindingContext = _productCompartimentService;
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        try
        {
            // Disable button to prevent multiple submissions
            if (sender is Button button)
            {
                button.IsEnabled = false;
                button.Text = "Asignando...";
            }

            // Enhanced validation with professional alerts
            if (_productCompartimentService.SelectProduct == null)
            {
                await CustomAlert.ShowErrorAsync("Debe seleccionar un producto para asignar al compartimento", "Producto Requerido");
                return;
            }

            if (_productCompartimentService.SelectCompartiment == null)
            {
                await CustomAlert.ShowErrorAsync("Debe seleccionar un compartimento para asignar el producto", "Compartimento Requerido");
                return;
            }

            if (_productCompartimentService.Stock < 0)
            {
                await CustomAlert.ShowErrorAsync("El stock inicial no puede ser negativo", "Stock Inválido");
                return;
            }

            // Optional: Check if stock exceeds compartment capacity (if available)
            if (_productCompartimentService.Stock > 50000) // reasonable limit
            {
                bool confirm = await CustomAlert.ShowConfirmAsync(
                    $"El stock inicial ({_productCompartimentService.Stock:F2} L) parece muy alto.\n\n¿Confirma que este valor es correcto?",
                    "Stock Elevado",
                    "Confirmar",
                    "Revisar");
                
                if (!confirm) return;
            }

            string productName = _productCompartimentService.SelectProduct.Name ?? "N/A";
            int compartmentNumber = _productCompartimentService.SelectCompartiment.Number;
            
            bool confirmAssignment = await CustomAlert.ShowConfirmAsync(
                $"¿Confirma la asignación del producto '{productName}' al compartimento #{compartmentNumber}?\n\n" +
                $"Stock inicial: {_productCompartimentService.Stock:F2} L\n\n" +
                $"Esta asignación establecerá la relación entre el producto y el compartimento.",
                "Confirmar Asignación",
                "Asignar",
                "Cancelar");
                
            if (!confirmAssignment) return;

            LoadingOverlay.ShowLoading();
            await _productCompartimentService.SaveProductCompartimentDataAsync();
            
            await CustomAlert.ShowSuccessAsync(
                $"Asignación completada exitosamente:\n\n" +
                $"• Producto: {productName}\n" +
                $"• Compartimento: #{compartmentNumber}\n" +
                $"• Stock Inicial: {_productCompartimentService.Stock:F2} L\n\n" +
                $"El producto ahora está disponible en este compartimento.",
                "Producto Asignado");
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error al asignar el producto al compartimento:\n\n{ex.Message}", "Error del Sistema");
        }
        finally
        {
            LoadingOverlay.HideLoading();

            // Clear selections
            _productCompartimentService.SelectProduct = null;
            _productCompartimentService.SelectCompartiment = null;
            Stock = 0;
            
            // Re-enable button
            if (sender is Button button)
            {
                button.IsEnabled = true;
                button.Text = "?? Asignar Producto";
            }
        }
    }

    public double Stock
    {
        get => _productCompartimentService.Stock;
        set
        {
            _productCompartimentService.Stock = value;
            OnPropertyChanged();
        }
    }
}
