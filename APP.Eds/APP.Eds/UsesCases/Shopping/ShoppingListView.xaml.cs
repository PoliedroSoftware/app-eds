using APP.Eds.Services.Shopping;
using APP.Eds.Models.ShoppingProduct;
using System.Collections.ObjectModel;
using System.Linq;
using APP.Eds.Models.Shopping;

namespace APP.Eds.UsesCases.Shopping;

public partial class ShoppingListView : ContentPage
{
    private ShoppingService _shoppingService;
    private ObservableCollection<ShoppingResponseViewModel> _filteredShoppingList;
    private int _currentPage = 1;
    private const int _pageSize = 5;
    private string _currentSearchText = "";
    private ProviderModel _selectedProviderFilter;
    private CategoryModel _selectedCategoryFilter;

    public ShoppingListView()
    {
        InitializeComponent();
        _shoppingService = ShoppingService.Instance;
        BindingContext = _shoppingService;
        _filteredShoppingList = new ObservableCollection<ShoppingResponseViewModel>();
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
            
            await _shoppingService.GetAllShoppingAsync(_currentPage, _pageSize);
            
            ApplyFilters();
            UpdateStatistics();
            UpdateEmptyState();
            UpdatePaginationButtons();
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

    private void ApplyFilters()
    {
        _filteredShoppingList.Clear();
        
        var items = _shoppingService.ShoppingList.AsEnumerable();
        
        // Filter by invoice search text
        if (!string.IsNullOrWhiteSpace(_currentSearchText))
        {
            items = items.Where(s => 
                s.Invoice?.Contains(_currentSearchText, StringComparison.OrdinalIgnoreCase) == true);
        }
        
        // Filter by provider
        if (_selectedProviderFilter != null)
        {
            items = items.Where(s => s.IdProvider == _selectedProviderFilter.IdProvider);
        }
        
        // Filter by category
        if (_selectedCategoryFilter != null)
        {
            items = items.Where(s => s.IdCategory == _selectedCategoryFilter.IdCategory);
        }
        
        // Add filtered items to collection
        foreach (var item in items.OrderByDescending(s => s.Date))
        {
            _filteredShoppingList.Add(new ShoppingResponseViewModel(item));
        }
        
        ShoppingCollectionView.ItemsSource = _filteredShoppingList;
        
        // Show/hide clear filters button
        UpdateClearFiltersButtonVisibility();
    }

    private void UpdateClearFiltersButtonVisibility()
    {
        bool hasFilters = !string.IsNullOrWhiteSpace(_currentSearchText) || 
                         _selectedProviderFilter != null || 
                         _selectedCategoryFilter != null;
        
        if (ClearFiltersButton != null)
        {
            ClearFiltersButton.IsVisible = hasFilters;
        }
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
            
            System.Diagnostics.Debug.WriteLine($"📊 Estadísticas del listado:");
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

    private void UpdatePaginationButtons()
    {
        // Update pagination info label
        if (PageInfoLabel != null)
        {
            var totalItems = _filteredShoppingList.Count;
            PageInfoLabel.Text = $"Página {_currentPage} ({totalItems} registros)";
        }

        // Disable previous button on first page
        if (PreviousPageButton != null)
        {
            PreviousPageButton.IsEnabled = _currentPage > 1;
        }

        // Disable next button if we received less than pageSize items
        if (NextPageButton != null)
        {
            NextPageButton.IsEnabled = _filteredShoppingList.Count >= _pageSize;
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        _currentSearchText = e.NewTextValue;
        ApplyFilters();
        UpdateEmptyState();
    }

    private void OnProviderFilterChanged(object sender, EventArgs e)
    {
        if (ProviderFilterPicker.SelectedItem is ProviderModel selectedProvider)
        {
            _selectedProviderFilter = selectedProvider;
        }
        else
        {
            _selectedProviderFilter = null;
        }
        
        ApplyFilters();
        UpdateEmptyState();
    }

    private void OnCategoryFilterChanged(object sender, EventArgs e)
    {
        if (CategoryFilterPicker.SelectedItem is CategoryModel selectedCategory)
        {
            _selectedCategoryFilter = selectedCategory;
        }
        else
        {
            _selectedCategoryFilter = null;
        }
        
        ApplyFilters();
        UpdateEmptyState();
    }

    private void OnClearFiltersClicked(object sender, EventArgs e)
    {
        // Clear all filters
        _currentSearchText = "";
        _selectedProviderFilter = null;
        _selectedCategoryFilter = null;
        
        // Reset UI controls
        if (SearchEntry != null)
            SearchEntry.Text = string.Empty;
        
        if (ProviderFilterPicker != null)
            ProviderFilterPicker.SelectedIndex = -1;
        
        if (CategoryFilterPicker != null)
            CategoryFilterPicker.SelectedIndex = -1;
        
        // Reapply filters (which will now show all)
        ApplyFilters();
        UpdateEmptyState();
    }

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        _currentPage = 1; // Reset to first page
        await LoadShoppingData();
        await DisplayAlert("Actualizado", "El listado de compras se actualizo correctamente", "OK");
    }

    private async void OnPreviousPageClicked(object sender, EventArgs e)
    {
        if (_currentPage > 1)
        {
            _currentPage--;
            await LoadShoppingData();
        }
    }

    private async void OnNextPageClicked(object sender, EventArgs e)
    {
        _currentPage++;
        await LoadShoppingData();
    }
}

// ViewModel wrapper to add calculated properties
public class ShoppingResponseViewModel : ShoppingResponse
{
    public string TotalGallonsText { get; set; }
    
    public ShoppingResponseViewModel(ShoppingResponse response)
    {
        // Copy all properties
        IdShopping = response.IdShopping;
        Invoice = response.Invoice;
        Date = response.Date;
        IdProvider = response.IdProvider;
        IdCategory = response.IdCategory;
        Amount = response.Amount;
        Provider = response.Provider;
        Category = response.Category;
        ShoppingProducts = response.ShoppingProducts;
        
        // Calculate total gallons
        var totalGallons = ShoppingProducts?.Sum(p => p.Quantity) ?? 0;
        TotalGallonsText = $"{totalGallons:N2} gal";
    }
}
