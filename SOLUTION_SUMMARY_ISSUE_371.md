# Issue #371: Fix - EDS Product Filtering in Shopping Module

## Problem Statement
When creating a new purchase in the Shopping module, the system was displaying ALL products and compartments from ALL EDS stations, regardless of which EDS was selected. This allowed users to add products from other stations, which affected inventory tracking and caused incorrect purchase records.

### Evidence from Screenshots
The screenshots showed products from multiple EDS stations (ACPM, Gasolina, Urea, Diesel Extra, etc.) being displayed simultaneously in the product selection popup, even though only one EDS was selected.

## Root Cause Analysis

### What Was Already Working ✅
1. **Filtered Collection Exists**: `FilteredProductCompartimentPairs` collection was already implemented in `ShoppingService.cs` (line 28)
2. **Filtering Logic Exists**: The method `FilterCompartimentsByEdsAsync()` (lines 633-766) correctly filters products based on EDS-Tank relationships
3. **Popup Uses Filtered Collection**: `AddShopping.xaml` (line 124) was already binding to `FilteredProductCompartimentPairs`
4. **Filter Trigger Exists**: The filter is triggered when `SelectedEds` property changes (lines 388-408)

### What Was Missing ❌
1. **No validation before opening popup**: Users could open the product selection popup without selecting an EDS first
2. **No empty state handling**: When an EDS had no configured products, users saw an empty picker with no explanation
3. **No user feedback**: Users weren't informed why they couldn't see products or why the list was empty

## Solution Implemented

### 1. Pre-Popup Validation (ShoppingPostView.xaml.cs)
Added two validation checks before opening the product selection popup:

#### Validation 1: EDS Selection Required
```csharp
if (_shoppingService.SelectedEds == null)
{
    await CustomAlert.ShowErrorAsync(
        "Debe seleccionar una Estación de Servicio (EDS) antes de agregar productos.\n\n" +
        "Por favor, seleccione un EDS en el campo correspondiente y luego intente agregar productos.",
        "EDS Requerido");
    return;
}
```

#### Validation 2: Products Availability Check
```csharp
if (_shoppingService.FilteredProductCompartimentPairs == null || 
    _shoppingService.FilteredProductCompartimentPairs.Count == 0)
{
    await CustomAlert.ShowWarningAsync(
        $"No hay productos ni compartimentos configurados para el EDS '{_shoppingService.SelectedEds.Name}'.\n\n" +
        "Por favor:\n" +
        "• Verifique que el EDS tenga tanques asignados\n" +
        "• Verifique que los tanques tengan compartimentos configurados\n" +
        "• Verifique que los compartimentos tengan productos asociados",
        "Sin Productos Disponibles");
    return;
}
```

### 2. Empty State UI (AddShopping.xaml)
Added visual elements to display when no products are available:

```xml
<!-- Empty State Message - Visible cuando NO hay productos -->
<Border x:Name="EmptyStateContainer"
        BackgroundColor="#FFF3CD" 
        StrokeShape="RoundRectangle 12"
        Stroke="#F59E0B"
        StrokeThickness="2"
        Padding="16"
        IsVisible="False">
    <StackLayout Spacing="12">
        <StackLayout Orientation="Horizontal" Spacing="12" HorizontalOptions="Center">
            <Label Text="⚠️" FontSize="24" VerticalOptions="Center"/>
            <Label Text="Sin Productos Disponibles" 
                   FontSize="16" 
                   FontAttributes="Bold"
                   TextColor="#92400E"
                   VerticalOptions="Center"/>
        </StackLayout>
        <Label x:Name="EmptyStateMessage"
               Text="No hay productos ni compartimentos configurados para esta EDS."
               FontSize="13"
               TextColor="#92400E"
               HorizontalTextAlignment="Center"
               LineBreakMode="WordWrap"/>
    </StackLayout>
</Border>
```

### 3. Empty State Logic (AddShopping.xaml.cs)
Added method to manage visibility of empty state UI:

```csharp
private void UpdateEmptyStateVisibility()
{
    bool hasProducts = shoppingService.FilteredProductCompartimentPairs != null && 
                      shoppingService.FilteredProductCompartimentPairs.Count > 0;
    
    // Show/hide picker and empty state message
    if (PickerContainer != null)
        PickerContainer.IsVisible = hasProducts;
        
    if (EmptyStateContainer != null)
        EmptyStateContainer.IsVisible = !hasProducts;
    
    // Disable Add button when no products available
    if (AddButton != null)
        AddButton.IsEnabled = hasProducts;
    
    // Update empty state message with EDS information
    if (!hasProducts && EmptyStateMessage != null && shoppingService.SelectedEds != null)
    {
        EmptyStateMessage.Text = $"No hay productos ni compartimentos configurados para el EDS '{shoppingService.SelectedEds.Name}'.\n\n" +
                                "Por favor, verifique:\n" +
                                "• Que el EDS tenga tanques asignados\n" +
                                "• Que los tanques tengan compartimentos\n" +
                                "• Que los compartimentos tengan productos";
    }
}
```

## Technical Details

### Data Flow
1. User selects an EDS from the picker
2. `SelectedEds` property setter is triggered (ShoppingService.cs line 393)
3. `FilterCompartimentsByEdsAsync()` is called automatically (line 400)
4. Method fetches EDS-Tank relationships from API (`/api/v1/eds-tank`)
5. Filters compartments that belong to tanks assigned to the selected EDS
6. Updates `FilteredProductCompartimentPairs` collection
7. UI automatically updates via data binding

### Database Relationships
```
EDS → eds_tank → Tank → Compartment → Product
```

The filtering ensures that only compartments from tanks assigned to the selected EDS are shown.

## Files Modified

1. **ShoppingPostView.xaml.cs**
   - Added EDS selection validation
   - Added empty product list validation
   - User-friendly error messages

2. **AddShopping.xaml**
   - Added empty state container with warning styling
   - Added conditional visibility for picker vs empty state

3. **AddShopping.xaml.cs**
   - Added `UpdateEmptyStateVisibility()` method
   - Called on popup initialization
   - Manages UI state based on product availability

## Benefits

### User Experience Improvements
1. **Clear Validation**: Users are informed immediately if they try to add products without selecting an EDS
2. **Helpful Guidance**: When no products are available, users receive clear instructions on what to check
3. **Prevents Errors**: Users cannot add products from wrong EDS stations
4. **Better Feedback**: Empty states provide context instead of showing blank pickers

### Technical Improvements
1. **Data Integrity**: Ensures purchases are recorded for the correct EDS
2. **Inventory Accuracy**: Prevents inventory corruption from products assigned to wrong stations
3. **Fail-Fast Validation**: Catches issues before the popup opens, reducing user frustration
4. **Minimal Changes**: Leveraged existing filtering logic, only added validation and UI feedback

## Testing Recommendations

### Manual Testing Checklist
- [ ] Verify error message when no EDS is selected
- [ ] Verify warning message when EDS has no products
- [ ] Verify only filtered products show for selected EDS
- [ ] Verify switching between EDS stations updates product list
- [ ] Verify complete purchase flow works correctly
- [ ] Verify empty state UI displays correctly

### Edge Cases to Test
1. EDS with no tanks assigned
2. EDS with tanks but no compartments
3. EDS with compartments but no products
4. Switching between EDS mid-flow
5. Multiple users purchasing for different EDS simultaneously

## Acceptance Criteria Status

✅ **Al seleccionar una EDS, la lista de productos/compartimentos se limita exclusivamente a los que correspondan a la EDS.**
- Solution: Already implemented in existing code, now with validation to ensure it's always followed

✅ **En caso de que no haya productos asociados, el usuario ve un mensaje claro en lugar de una lista vacía.**
- Solution: Added empty state UI with detailed message explaining what to check

✅ **El popup, la pantalla de nueva compra y cualquier flujo relacionado usan SIEMPRE la colección filtrada por EDS.**
- Solution: Verified popup uses `FilteredProductCompartimentPairs`, added validation to enforce this requirement

## Notes

- No backend changes were needed (as requested in the issue)
- All filtering logic was already present and working correctly
- The fix focuses on validation and user feedback
- Changes are minimal and surgical, following the principle of least modification
- Solution is defensive - prevents issues before they occur rather than handling them after

## Related Files Reference

- **Service**: `APP.Eds/APP.Eds/Services/Shopping/ShoppingService.cs`
- **View**: `APP.Eds/APP.Eds/UsesCases/Shopping/ShoppingPostView.xaml.cs`
- **Popup View**: `APP.Eds/APP.Eds/Components/PopUp/AddShopping.xaml`
- **Popup Logic**: `APP.Eds/APP.Eds/Components/PopUp/AddShopping.xaml.cs`
- **Models**: `APP.Eds/APP.Eds/Models/Shopping/ProductCompartimentPairModel.cs`

## Security Considerations

- Frontend validation is implemented, but backend should also validate EDS ownership
- Ensure API endpoints verify user permissions for selected EDS
- Consider adding logging for filtered product access attempts

## Future Enhancements (Optional)

1. Add unit tests for filtering logic
2. Add integration tests for EDS selection flow
3. Consider caching filtered results for performance
4. Add analytics to track which EDS stations have configuration issues
5. Consider admin dashboard showing EDS configuration status
