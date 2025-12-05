# Manual Testing Guide for Issue #371: EDS Product Filtering

## Issue Summary
When creating a new purchase in the Shopping module, the system was listing ALL products and compartments from ALL EDS stations instead of filtering by the selected EDS. This allowed adding products from other stations, affecting inventory and causing incorrect purchase records.

## Solution Implemented
- Added validation to ensure an EDS is selected before opening the product selection popup
- Added empty state message when the selected EDS has no configured products/compartments
- The popup already uses `FilteredProductCompartimentPairs` collection (line 124 of AddShopping.xaml)
- The filtering logic already exists and triggers when an EDS is selected (ShoppingService.cs lines 633-766)

## Test Scenarios

### Test 1: Attempt to add product without selecting EDS
**Steps:**
1. Navigate to Shopping → Nueva Compra
2. Fill in Invoice number, Date, Provider, and Category
3. **DO NOT** select an EDS
4. Click "📦 Agregar producto" button

**Expected Result:**
- An error alert should appear with the message:
  ```
  EDS Requerido
  
  Debe seleccionar una Estación de Servicio (EDS) antes de agregar productos.
  
  Por favor, seleccione un EDS en el campo correspondiente y luego intente agregar productos.
  ```
- The popup should NOT open
- User should be able to continue and select an EDS

**Status:** ⬜ Not Tested | ✅ Pass | ❌ Fail

---

### Test 2: Select EDS with configured products
**Steps:**
1. Navigate to Shopping → Nueva Compra
2. Fill in Invoice number and Date
3. Select an EDS from the "Estación de Servicio (EDS)" picker (e.g., "Eds prueba" or "La paz")
4. Select a Provider and Category
5. Click "📦 Agregar producto" button

**Expected Result:**
- The "Nuevo Producto" popup should open
- The "Producto y Compartimento" picker should show ONLY products/compartments belonging to the selected EDS
- Products from other EDS stations should NOT be visible
- The picker should be enabled and functional
- The "🚀 Agregar a la Compra" button should be enabled

**Verification:**
- Cross-check that products listed match the selected EDS
- Verify tank assignments in the database match what's shown

**Status:** ⬜ Not Tested | ✅ Pass | ❌ Fail

---

### Test 3: Select EDS with no configured products/compartments
**Steps:**
1. Navigate to Shopping → Nueva Compra  
2. Select an EDS that has no tanks assigned OR no compartments configured
3. Fill in Provider and Category
4. Click "📦 Agregar producto" button

**Expected Result:**
- A warning alert should appear with the message:
  ```
  Sin Productos Disponibles
  
  No hay productos ni compartimentos configurados para el EDS '[EDS Name]'.
  
  Por favor:
  • Verifique que el EDS tenga tanques asignados
  • Verifique que los tanques tengan compartimentos configurados
  • Verifique que los compartimentos tengan productos asociados
  ```
- The popup should NOT open
- User should be able to go back and select a different EDS

**Status:** ⬜ Not Tested | ✅ Pass | ❌ Fail

---

### Test 4: Switch between different EDS stations
**Steps:**
1. Navigate to Shopping → Nueva Compra
2. Select EDS "Eds prueba"
3. Click "📦 Agregar producto" and note which products are shown
4. Close the popup (click X button)
5. Change the EDS selection to "La paz"
6. Click "📦 Agregar producto" again

**Expected Result:**
- The products shown in the picker should be DIFFERENT for each EDS
- Only products belonging to "La paz" should be visible in step 6
- Products from "Eds prueba" should NOT be visible when "La paz" is selected

**Status:** ⬜ Not Tested | ✅ Pass | ❌ Fail

---

### Test 5: Complete purchase flow with filtered products
**Steps:**
1. Navigate to Shopping → Nueva Compra
2. Fill in all required fields including EDS selection
3. Click "📦 Agregar producto"
4. Select a product from the filtered list
5. Enter Precio Compra, Precio Venta, and Cantidad
6. Click "🚀 Agregar a la Compra"
7. Verify the product appears in "Productos Agregados" section
8. Click "✅ Finalizar compra"

**Expected Result:**
- Product should be added successfully
- The purchase should be saved to the database
- The inventory for the selected EDS should be updated correctly
- No products from other EDS stations should be affected

**Status:** ⬜ Not Tested | ✅ Pass | ❌ Fail

---

### Test 6: Empty state UI in popup
**Precondition:** An EDS with no products/compartments configured
**Steps:**
1. Temporarily bypass validation to open popup (for testing purposes only)
2. Or configure an EDS with tanks but no compartments

**Expected Result:**
- The picker container should be hidden
- An empty state message should be visible with:
  - ⚠️ icon
  - "Sin Productos Disponibles" heading
  - Detailed message explaining the issue
  - Yellow/orange styling (#FFF3CD background, #F59E0B border)
- The "🚀 Agregar a la Compra" button should be disabled

**Status:** ⬜ Not Tested | ✅ Pass | ❌ Fail

---

## Acceptance Criteria (from Issue #371)

- [ ] Al seleccionar una EDS, la lista de productos/compartimentos se limita exclusivamente a los que correspondan a la EDS.
- [ ] En caso de que no haya productos asociados, el usuario ve un mensaje claro en lugar de una lista vacía.
- [ ] El popup, la pantalla de nueva compra y cualquier flujo relacionado usan SIEMPRE la colección filtrada por EDS.

## Code Verification Checklist

- [x] `AddShopping.xaml` line 124: Picker uses `FilteredProductCompartimentPairs` (not the unfiltered collection)
- [x] `ShoppingService.cs` lines 388-408: Filter triggers when `SelectedEds` changes
- [x] `ShoppingService.cs` lines 633-766: Filter logic implemented correctly
- [x] `ShoppingPostView.xaml.cs`: Added EDS validation before opening popup
- [x] `AddShopping.xaml`: Added empty state UI elements
- [x] `AddShopping.xaml.cs`: Added empty state visibility logic

## Notes for QA
- The filtering is done based on the `eds_tank` relationship in the database
- An EDS must have tanks assigned, and those tanks must have compartments with products
- The filter is applied automatically when an EDS is selected
- The unfiltered collection (`ProductCompartimentPairs`) is never exposed to the UI for product selection

## Screenshots to Capture
1. Error message when no EDS is selected
2. Warning message when EDS has no products
3. Popup showing only filtered products for selected EDS
4. Empty state UI in popup (if applicable)
5. Successful purchase with correct EDS filtering

## Regression Testing
- Verify that existing shopping functionality still works
- Verify that changing EDS updates the product list correctly
- Verify that invoice validation still works
- Verify that product addition and removal still works correctly
