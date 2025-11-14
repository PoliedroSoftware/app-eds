using APP.Eds.Components.PopUp;
using APP.Eds.Models.Product;
using APP.Eds.Services.Product;
using APP.Eds.UsesCases.ProductType;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;


namespace APP.Eds.UsesCases.Product;

public class EditablePendingProduct : INotifyPropertyChanged
{
    public EditablePendingProduct()
    {
        // Inicializar los tipos específicos
        AvailableProductTypes.Clear();
        AvailableProductTypes.Add(new SpecificProductType(1, "Corriente", 1, "⛽")); // Gasolina Corriente
        AvailableProductTypes.Add(new SpecificProductType(2, "Extra", 1, "✨"));     // Gasolina Extra
        AvailableProductTypes.Add(new SpecificProductType(3, "ACPM", 2, "🚛"));     // ACPM
        AddSampleData();
    }
    public int IdProduct { get; set; }
    public int IdProductType { get; set; }
    public double SellPrice { get; set; }
    public double PurchasePrice { get; set; }
    public int Stock { get; set; }
    public int IdEds { get; set; }
    public bool IsFormValid => !string.IsNullOrWhiteSpace(Name) && IdProductType > 0;
    public string SelectedEdsName { get; set; }

    public ObservableCollection<SpecificProductType> AvailableProductTypes { get; set; } = [];
    public ObservableCollection<SpecificProductType> FilteredProductTypes { get; set; } = [];
    public ObservableCollection<ProductTypeModelResponse> ProductTypeList { get; set; } = [];
    public ObservableCollection<EnhancedProductTypeItem> EnhancedProductTypeList { get; set; } = [];
    public bool IsProductTypeSelectionVisible => SelectedProductOption?.Id == 1; // Gasolina
    public bool IsAcpmSelected => SelectedProductOption?.Id == 2; // ACPM

    private string _name;
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(IsFormValid)); // Notificar cambio en validez del formulario
        }
    }



    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    private SpecificProductType _selectedSpecificProductType;


    public SpecificProductType SelectedSpecificProductType
    {
        get => _selectedSpecificProductType;
        set
        {
            _selectedSpecificProductType = value;
            OnPropertyChanged(nameof(SelectedSpecificProductType));

            if (SelectedProductOption == null) return;

            if (SelectedProductOption.Id == 2) // ACPM
            {
                Name = "ACPM";
                UpdateProductTypeFromSelection();
                return;
            }

            if (SelectedProductOption.Id == 3) // Urea
            {
                Name = "Urea";
                UpdateProductTypeFromSelection();
                return;
            }

            // Gasolina con subtipo
            if (_selectedSpecificProductType != null)
            {
                Name = $"Gasolina {_selectedSpecificProductType.Description}".Trim();
                UpdateProductTypeFromSelection();
            }
        }
    }
    private void UpdateProductTypeFromSelection()
    {
        if (SelectedProductOption == null) return;

        // Gasolina con subtipo
        if (SelectedProductOption.Id == 1 && SelectedSpecificProductType != null)
        {
            var id = GetBackendTypeIdForGasoline(SelectedSpecificProductType.Description);
            if (id.HasValue)
            {
                IdProductType = id.Value;
                return;
            }
        }

        // ACPM sin subtipo
        if (SelectedProductOption.Id == 2)
        {
            var id = GetBackendTypeIdForAcpm();
            if (id.HasValue)
            {
                IdProductType = id.Value;
                return;
            }
        }

        // Urea sin subtipo
        if (SelectedProductOption.Id == 3)
        {
            var id = GetBackendTypeIdForUrea();
            if (id.HasValue)
            {
                IdProductType = id.Value;
                return;
            }
        }

        // Fallback: no tocar IdProductType si no hay match
    }

    // Método para actualizar el tipo de producto basado en la selección   
    private int? GetBackendTypeIdByPredicate(Func<string, bool> predicate)
    {
        var match = EnhancedProductTypeList.FirstOrDefault(pt =>
            predicate((pt.Description ?? string.Empty).ToLowerInvariant()));
        return match?.IdProductType;
    }

    private int? GetBackendTypeIdForGasoline(string subtype) // "corriente" o "extra"
    {
        var sub = (subtype ?? "").Trim().ToLowerInvariant();
        return GetBackendTypeIdByPredicate(desc =>
            desc.Contains("gasolina") && desc.Contains(sub));
    }

    private int? GetBackendTypeIdForAcpm()
    {
        return GetBackendTypeIdByPredicate(desc => desc.Contains("acpm") || desc.Contains("diesel"));
    }

    private int? GetBackendTypeIdForUrea()
    {
        return GetBackendTypeIdByPredicate(desc => desc.Contains("urea"));
    }
    private void OnProductOptionChanged()
    {
        if (SelectedProductOption == null)
        {
            FilteredProductTypes.Clear();
            SelectedSpecificProductType = null;
            Name = string.Empty;
        }
        else
        {
            // Filtrar tipos de producto según la opción seleccionada
            FilteredProductTypes.Clear();
            var filteredTypes = AvailableProductTypes.Where(pt => pt.ParentProductId == SelectedProductOption.Id);

            foreach (var type in filteredTypes)
            {
                FilteredProductTypes.Add(type);
            }

            // Si es ACPM, seleccionar automáticamente
            if (SelectedProductOption.Id == 2) // ACPM
            {
                SelectedSpecificProductType = null;
                Name = "ACPM";
                UpdateProductTypeFromSelection();
            }
            else if (SelectedProductOption.Id == 3)
            {
                SelectedSpecificProductType = null;
                Name = "Urea";
                UpdateProductTypeFromSelection();
            }
            else
            {
                SelectedSpecificProductType = null;
                Name = string.Empty;
            }
        }

        // Notificar cambios en las propiedades de visibilidad
        OnPropertyChanged(nameof(FilteredProductTypes));
        OnPropertyChanged(nameof(IsProductTypeSelectionVisible));
        OnPropertyChanged(nameof(IsAcpmSelected));
    }

    private void AddSampleData()
    {
        try
        {
            var sampleTypes = new List<ProductTypeModelResponse>
            {
                new ProductTypeModelResponse { IdProductType = 1, Description = "Gasolina Corriente" },
                new ProductTypeModelResponse { IdProductType = 2, Description = "Gasolina Extra" },
                new ProductTypeModelResponse { IdProductType = 3, Description = "ACPM" },
                new ProductTypeModelResponse { IdProductType = 5, Description = "Urea" },
                new ProductTypeModelResponse { IdProductType = 4, Description = "Lubricantes" }
            };
            UpdateProductTypeList(sampleTypes);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error adding sample data: {ex.Message}");
        }
    }


    private void UpdateProductTypeList(IEnumerable<ProductTypeModelResponse> data)
    {
        try
        {
            ProductTypeList.Clear();
            EnhancedProductTypeList.Clear();

            if (data != null)
            {
                foreach (var item in data)
                {
                    ProductTypeList.Add(item);
                    EnhancedProductTypeList.Add(new EnhancedProductTypeItem(item));
                }
            }

            // Notify that collections have changed
            OnPropertyChanged(nameof(ProductTypeList));
            OnPropertyChanged(nameof(EnhancedProductTypeList));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating product type list: {ex.Message}");
        }
    }

    private ProductOption _selectedProductOption;
    public ProductOption SelectedProductOption
    {
        get => _selectedProductOption;
        set
        {
            _selectedProductOption = value;
            OnPropertyChanged(nameof(SelectedProductOption));
            OnProductOptionChanged();
        }
    }
}

public partial class ProductPostView : ContentPage, INotifyPropertyChanged
{
    private ProductService _productService;

    public ICommand EditProductCommand { get; }
    public ICommand DeleteProductCommand { get; }

    public ProductPostView()
	{
		InitializeComponent();
        _productService = new ProductService();
        BindingContext = _productService;
        
        // These commands are kept for future use when ProductList is implemented
        DeleteProductCommand = new Command<object>(OnDeleteProduct);
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        bool wasSuccessful = false;

        try
        {
            // Disable button to prevent multiple submissions
            if (button != null)
            {
                button.IsEnabled = false;
                button.Text = "Registrando...";
            }

            // Validar campos requeridos específicos del nuevo sistema
            if (_productService.SelectedProductOption == null)
            {
                await CustomAlert.ShowErrorAsync("Por favor seleccione el tipo de combustible (Gasolina o ACPM)", "Tipo de Combustible Requerido");
                return;
            }

            // Si es Gasolina, debe seleccionar el tipo específico
            if (_productService.SelectedProductOption.Id == 1 && _productService.SelectedSpecificProductType == null)
            {
                await CustomAlert.ShowErrorAsync("Por favor seleccione el tipo de gasolina (Corriente o Extra)", "Tipo de Gasolina Requerido");
                return;
            }

            // Validar que el nombre se haya generado correctamente
            if (string.IsNullOrWhiteSpace(_productService.Name))
            {
                await CustomAlert.ShowErrorAsync("El nombre del producto no se generó correctamente. Por favor revise su selección.", "Error en Nombre del Producto");
                return;
            }

            // Validar que los valores numéricos no sean negativos
            if (_productService.PurchasePrice < 0)
            {
                await CustomAlert.ShowErrorAsync("El precio de compra no puede ser negativo", "Precio de Compra Inválido");
                return;
            }

            if (_productService.SellPrice < 0)
            {
                await CustomAlert.ShowErrorAsync("El precio de venta no puede ser negativo", "Precio de Venta Inválido");
                return;
            }

            if (_productService.Stock < 0)
            {
                await CustomAlert.ShowErrorAsync("El stock no puede ser negativo", "Stock Inválido");
                return;
            }

            // Validación de lógica de negocio para precios
            if (_productService.SellPrice > 0 && _productService.PurchasePrice > 0 && 
                _productService.SellPrice <= _productService.PurchasePrice)
            {
                bool confirm = await CustomAlert.ShowConfirmAsync(
                    $"El precio de venta (${_productService.SellPrice:F2}) es menor o igual al precio de compra (${_productService.PurchasePrice:F2}).\n\n" +
                    $"Esto podría resultar en pérdidas. ¿Desea continuar de todas formas?",
                    "Advertencia de Rentabilidad",
                    "Continuar",
                    "Revisar Precios");
                
                if (!confirm) return;
            }

            // Validación específica de rangos de precios para combustibles
            if (_productService.SellPrice > 0)
            {
                // Rangos aproximados para Colombia (pueden ajustarse según el mercado)
                bool isGasoline = _productService.SelectedProductOption.Id == 1;
                bool isAcpm = _productService.SelectedProductOption.Id == 2;

                if (isGasoline && (_productService.SellPrice < 8000 || _productService.SellPrice > 20000))
                {
                    bool confirm = await CustomAlert.ShowConfirmAsync(
                        $"El precio de venta para gasolina (${_productService.SellPrice:F2}) está fuera del rango típico (8.000 - 20.000 COP).\n\n" +
                        $"¿Confirma que este precio es correcto?",
                        "Precio Atípico para Gasolina",
                        "Confirmar",
                        "Revisar");
                    
                    if (!confirm) return;
                }

                if (isAcpm && (_productService.SellPrice < 7000 || _productService.SellPrice > 18000))
                {
                    bool confirm = await CustomAlert.ShowConfirmAsync(
                        $"El precio de venta para ACPM (${_productService.SellPrice:F2}) está fuera del rango típico (7.000 - 18.000 COP).\n\n" +
                        $"¿Confirma que este precio es correcto?",
                        "Precio Atípico para ACPM",
                        "Confirmar",
                        "Revisar");
                    
                    if (!confirm) return;
                }
            }

            // Mostrar confirmación antes de guardar
            var productType = _productService.SelectedProductOption.Name;
            var productSubtype = _productService.SelectedSpecificProductType?.Description ?? "";
            var fullProductName = _productService.Name;

            var confirmationMessage = $"¿Confirma el registro del siguiente producto?\n\n" +
                                    $"• Tipo: {productType}\n" +
                                    $"• Subtipo: {productSubtype}\n" +
                                    $"• Nombre completo: {fullProductName}\n";

            if (_productService.PurchasePrice > 0)
                confirmationMessage += $"• Precio de compra: ${_productService.PurchasePrice:F2}\n";

            if (_productService.SellPrice > 0)
                confirmationMessage += $"• Precio de venta: ${_productService.SellPrice:F2}\n";

            if (_productService.Stock > 0)
                confirmationMessage += $"• Stock inicial: {_productService.Stock:N0} galones\n";

            bool finalConfirm = await CustomAlert.ShowConfirmAsync(
                confirmationMessage,
                "Confirmar Registro",
                "Registrar",
                "Cancelar");

            if (!finalConfirm) return;

            LoadingOverlay.ShowLoading();
            await _productService.SaveProductDataAsync();
            wasSuccessful = true;

            // El formulario se limpia automáticamente en el servicio después de un guardado exitoso
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Ocurrió un error inesperado al registrar el producto:\n\n{ex.Message}", "Error del Sistema");
        }
        finally
        {
            LoadingOverlay.HideLoading();

            // Re-enable and restore button
            if (button != null)
            {
                button.IsEnabled = true;
                button.Text = "?? Registrar Producto";
            }
        }
    }

    private async void OnAddProductTypeClicked(object sender, EventArgs e)
    {
        try
        {
            await Navigation.PushAsync(new ProductTypePostView());
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"No se pudo abrir la página de tipos de producto:\n\n{ex.Message}", "Error de Navegación");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Refresh product types when returning from ProductType creation
        try
        {
            await _productService.RefreshProductTypesAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error refreshing product types: {ex.Message}");
            // Don't show error to user as this is not critical
        }
    }

    private async void OnDeleteProduct(object obj)
    {
        // This method is kept for future use when ProductList with ProductModelResponse is implemented
        if (obj is ProductModelResponse product)
        {
            bool confirm = await CustomAlert.ShowConfirmAsync(
                $"¿Está seguro de que desea eliminar el producto '{product.Name}'?\n\nEsta acción no se puede deshacer.", 
                "Confirmar Eliminación", 
                "Eliminar", 
                "Cancelar");

            if (confirm)
            {
                try
                {
                    LoadingOverlay.ShowLoading();
                    bool deleted = await _productService.DeleteProductAsync(product.IdProduct);
                    if (deleted)
                    {
                        await CustomAlert.ShowSuccessAsync($"El producto '{product.Name}' ha sido eliminado correctamente", "Producto Eliminado");
                    }
                }
                catch (Exception ex)
                {
                    await CustomAlert.ShowErrorAsync($"Error al eliminar el producto:\n\n{ex.Message}", "Error de Eliminación");
                }
                finally
                {
                    LoadingOverlay.HideLoading();
                }
            }
        }
    }

    // Propiedades para binding (mantenidas para compatibilidad)
    public string Name
    {
        get => _productService.Name;
        set
        {
            _productService.Name = value;
            OnPropertyChanged();
        }
    }

    public double PurchasePrice
    {
        get => _productService.PurchasePrice;
        set
        {
            _productService.PurchasePrice = value;
            OnPropertyChanged();
        }
    }

    public double SellPrice
    {
        get => _productService.SellPrice;
        set
        {
            _productService.SellPrice = value;
            OnPropertyChanged();
        }
    }

    public int Stock
    {
        get => _productService.Stock;
        set
        {
            _productService.Stock = value;
            OnPropertyChanged();
        }
    }

    // Propiedades adicionales para mejorar la UX del stock
    public string StockUnit => _productService.StockUnit;
    public string StockPlaceholder => _productService.StockPlaceholder;
    public string StockLabel => _productService.StockLabel;
    public string StockHint => _productService.StockHint;

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}