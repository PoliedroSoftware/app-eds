using APP.Eds.Services.Shopping;
using APP.Eds.Models.ShoppingProduct;
using System.Collections.ObjectModel;
using System.Linq;

namespace APP.Eds.UsesCases.Shopping;

public partial class ShoppingListView : ContentPage
{
    private ShoppingService _shoppingService;
    private ObservableCollection<ShoppingResponse> _filteredShoppingList;

    public ShoppingListView()
    {
        InitializeComponent();
        _shoppingService = ShoppingService.Instance;
        BindingContext = _shoppingService;
        _filteredShoppingList = new ObservableCollection<ShoppingResponse>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadShoppingData();
    }

    private async Task LoadShoppingData()
    {
        try
        {
            LoadingOverlay?.ShowLoading();
            
            await _shoppingService.GetAllShoppingAsync();
            
            UpdateFilteredList();
            UpdateStatistics();
            UpdateEmptyState();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al cargar compras: {ex.Message}", "OK");
        }
        finally
        {
            LoadingOverlay?.HideLoading();
        }
    }

    private void UpdateFilteredList(string searchText = "")
    {
        _filteredShoppingList.Clear();
        
        var items = string.IsNullOrWhiteSpace(searchText)
            ? _shoppingService.ShoppingList
            : _shoppingService.ShoppingList.Where(s => 
                s.Invoice?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true);
        
        foreach (var item in items.OrderByDescending(s => s.Date))
        {
            _filteredShoppingList.Add(item);
        }
        
        ShoppingCollectionView.ItemsSource = _filteredShoppingList;
    }

    private void UpdateStatistics()
    {
        if (_shoppingService.ShoppingList != null && _shoppingService.ShoppingList.Any())
        {
            var totalCount = _shoppingService.ShoppingList.Count;
            var totalAmount = _shoppingService.ShoppingList.Sum(s => s.Amount);
            
            // Calcular total de productos
            var totalProducts = _shoppingService.ShoppingList
                .Sum(s => s.ShoppingProducts?.Count ?? 0);
            
            // Calcular total de galones
            var totalGallons = _shoppingService.ShoppingList
                .Sum(s => s.ShoppingProducts?.Sum(p => p.Quantity) ?? 0);
            
            TotalCountLabel.Text = totalCount.ToString();
            TotalAmountLabel.Text = $"$ {totalAmount:N2}";
            
            System.Diagnostics.Debug.WriteLine($"?? Estadísticas del listado:");
            System.Diagnostics.Debug.WriteLine($"   - Total compras: {totalCount}");
            System.Diagnostics.Debug.WriteLine($"   - Total productos: {totalProducts}");
            System.Diagnostics.Debug.WriteLine($"   - Total galones: {totalGallons:N2}");
            System.Diagnostics.Debug.WriteLine($"   - Monto total: $ {totalAmount:N2}");
        }
        else
        {
            TotalCountLabel.Text = "0";
            TotalAmountLabel.Text = "$ 0.00";
        }
    }

    private void UpdateEmptyState()
    {
        bool isEmpty = _filteredShoppingList == null || _filteredShoppingList.Count == 0;
        EmptyStateView.IsVisible = isEmpty;
        ShoppingCollectionView.IsVisible = !isEmpty;
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateFilteredList(e.NewTextValue);
        UpdateEmptyState();
    }

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        await LoadShoppingData();
        await DisplayAlert("Actualizado", "El listado de compras se actualizo correctamente", "OK");
    }

    private async void OnShoppingItemTapped(object sender, EventArgs e)
    {
        if (sender is Border border && border.GestureRecognizers[0] is TapGestureRecognizer tapGesture)
        {
            if (tapGesture.CommandParameter is ShoppingResponse shopping)
            {
                try
                {
                    // Construir mensaje con información de productos
                    var message = $"?? Fecha: {shopping.Date:dd/MM/yyyy}\n" +
                                 $"?? Proveedor ID: {shopping.IdProvider}\n" +
                                 $"?? Categoria ID: {shopping.IdCategory}\n" +
                                 $"?? Monto Total: $ {shopping.Amount:N2}\n\n";

                    if (shopping.ShoppingProducts != null && shopping.ShoppingProducts.Any())
                    {
                        message += $"?? PRODUCTOS ({shopping.ShoppingProducts.Count}):\n";
                        message += new string('?', 40) + "\n";

                        var totalQuantity = 0.0;
                        var totalPurchaseValue = 0.0;

                        foreach (var product in shopping.ShoppingProducts)
                        {
                            message += $"\n??? Producto ID: {product.IdProduct}\n";
                            message += $"   ?? Cantidad: {product.Quantity:N2} gal\n";
                            message += $"   ?? P. Compra: $ {product.PurchasePrice:N2}\n";
                            message += $"   ?? P. Venta: $ {product.SellPrice:N2}\n";
                            message += $"   ?? Subtotal: $ {product.TotalPrice:N2}\n";

                            totalQuantity += product.Quantity;
                            totalPurchaseValue += product.TotalPrice;
                        }

                        message += "\n" + new string('?', 40) + "\n";
                        message += $"?? RESUMEN:\n";
                        message += $"   • Total Galones: {totalQuantity:N2} gal\n";
                        message += $"   • Total Compra: $ {totalPurchaseValue:N2}";
                    }
                    else
                    {
                        message += "?? No hay productos registrados para esta compra.";
                    }

                    await DisplayAlert(
                        $"?? Compra: {shopping.Invoice}",
                        message,
                        "Cerrar");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Error al mostrar detalles: {ex.Message}", "OK");
                }
            }
        }
    }
}
