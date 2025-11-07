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
            
            TotalCountLabel.Text = totalCount.ToString();
            TotalAmountLabel.Text = $"$ {totalAmount:N2}";
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
                await DisplayAlert(
                    $"Compra: {shopping.Invoice}",
                    $"Fecha: {shopping.Date:dd/MM/yyyy}\n" +
                    $"Proveedor ID: {shopping.IdProvider}\n" +
                    $"Categoria ID: {shopping.IdCategory}\n" +
                    $"Monto: $ {shopping.Amount:N2}",
                    "OK");
            }
        }
    }
}
