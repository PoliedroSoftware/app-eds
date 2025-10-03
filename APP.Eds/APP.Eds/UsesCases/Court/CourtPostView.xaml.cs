using APP.Eds.Components.PopUp;
using APP.Eds.Models.Court;
using APP.Eds.Models.Dispensers;
using APP.Eds.Models.Eds;
using APP.Eds.Services.Court;
using APP.Eds.UsesCases.Court.APP.Eds.Models.Business;
using APP.Eds.UsesCases.LoadingView;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.Shapes;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace APP.Eds.UsesCases.Court;

public partial class CourtPostView : ContentPage, INotifyPropertyChanged
{
    private async Task ShowOperationalSectionsAsync()
    {
        // 🔥 MÉTODO SIMPLIFICADO: Ya no se necesita manipular manualmente la visibilidad
        // porque ahora está controlada por las propiedades ShouldShowDispensersSection y ShouldShowPaymentMethodsSection
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            // Solo refrescar las secciones si es necesario
            await RefreshSectionsAsync(refreshDispensers: true, refreshPayments: true);
        });
    }
    private CourtService _service;
    public string UserRole { get; set; } = string.Empty;

    // --- Estado de edición/visibilidad controlado por la página (x:Reference CortePage)
    private bool _seccionesVisibles = true;
    public bool SeccionesVisibles
    {
        get => _seccionesVisibles;
        set
        {
            if (_seccionesVisibles == value) return;
            _seccionesVisibles = value;
            OnPropertyChanged(nameof(SeccionesVisibles));
        }
    }

    private bool _puedeEditar = true;
    public bool PuedeEditar
    {
        get => _puedeEditar;
        set
        {
            if (_puedeEditar == value) return;
            _puedeEditar = value;
            OnPropertyChanged(nameof(PuedeEditar));
        }
    }
    // ---------------------------------------------------------------------------

    // Menú inferior: elemento activo (solo visual/animación)
    private string _activeNavItem = "Document";
    public string ActiveNavItem
    {
        get => _activeNavItem;
        set
        {
            _activeNavItem = value;
            OnPropertyChanged();
            _ = AnimateToActiveItem(value);
        }
    }

    public CourtPostView()
    {
        try { InitializeComponent(); }
        catch
        {
            Title = "Cierre De Turno";
            BackgroundColor = Color.FromArgb("#F8F9FA");
            Content = CreateContent();
        }

        _service = CourtService.Instance;
        _service.DateStarttime = DateTime.Today;

        // El BindingContext sigue siendo el servicio (todas las bindings Court.* funcionan)
        BindingContext = _service;

        // Estado inicial para un flujo nuevo
        SetEditingState(canEdit: true, showSections: true);

        ConfigureDatePickerAsync();
        _ = _service.LoadTranslationsAsync();

        UserRole = Preferences.Get("userRole", string.Empty);

        // Aplica configuración persistida sobre el SERVICIO (no sobre controles)
        string configJson = Preferences.Get("userConfig", "{}");
        var config = JsonSerializer.Deserialize<Dictionary<string, bool>>(configJson);
        if (config is not null) ApplyConfigToService(config);

        ActiveNavItem = "Document";
    }

    private async void ConfigureDatePickerAsync()
    {
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            try
            {
                var picker = this.FindByName<DatePicker>("datePicker");
                if (picker != null)
                    picker.MinimumDate = new DateTime(1900, 1, 1);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error setting datePicker MinimumDate: {ex.Message}");
            }
        });
    }

    private View CreateContent() =>
        new StackLayout
        {
            Children =
            {
                new Label
                {
                    Text = "🏪 Cierre De Turno",
                    FontSize = 24,
                    FontAttributes = FontAttributes.Bold,
                    HorizontalOptions = LayoutOptions.Center,
                    TextColor = Colors.Purple
                }
            }
        };

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Siempre que se abre esta página se asume un nuevo flujo editable
        SetEditingState(canEdit: true, showSections: true);

        var loadingOverlay = this.FindByName<LoadingView.LoadingView>("LoadingOverlay");
        var mainContent = this.FindByName<ScrollView>("MainContent");
        try
        {
            loadingOverlay?.ShowLoading();
            if (mainContent != null) mainContent.IsVisible = false;

            // Mostrar/ocultar tarjeta Business por rol (Admin la ve)
            var businessBorder = this.FindByName<Border>("Business");
            if (businessBorder != null)
                businessBorder.IsVisible = (UserRole == "Admin");

            await _service.LoadTranslationsAsync();
            await AnimateBottomNavEntry();
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error al cargar datos:\n\n{ex.Message}", "Error de Carga");
        }
        finally
        {
            try { loadingOverlay?.HideLoading(); } catch { }
            if (mainContent != null) mainContent.IsVisible = true;
        }
    }

    // Aplica banderas de visibilidad guardadas al SERVICE (no a la UI)
    private void ApplyConfigToService(Dictionary<string, bool> config)
    {
        if (config.TryGetValue("VisibleDispenser", out bool v1)) _service.VisibleDispenser = v1;
        if (config.TryGetValue("VisibleReceipts", out bool v2)) _service.VisibleReceipts = v2;
        if (config.TryGetValue("VisibleDocuments", out bool v3)) _service.VisibleDocuments = v3;
        if (config.TryGetValue("VisibleExpenses", out bool v4)) _service.VisibleExpenses = v4;
    }

    // Helper: establece edición/visibilidad y notifica a XAML
    private void SetEditingState(bool canEdit, bool showSections)
    {
        PuedeEditar = canEdit;
        SeccionesVisibles = showSections;
    }

    // === Navegación inferior ===
    private async void OnDocumentTapped(object sender, EventArgs e) => await HandleNavTap("Document");
    private async void OnExpenseTapped(object sender, EventArgs e) => await HandleNavTap("Expense");
    private async void OnInfoTapped(object sender, EventArgs e) => await HandleNavTap("Info");
    private async void OnHistoryTapped(object sender, EventArgs e) => await HandleNavTap("History");

    private async Task HandleNavTap(string navItem)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(navItem)) return;

            if (!PuedeEditar && navItem is "Document" or "Expense" or "Info")
            {
                await CustomAlert.ShowErrorAsync("El corte ya fue enviado. Edición bloqueada.", "Corte cerrado");
                return;
            }

            ActiveNavItem = navItem;
            await AnimateNavItemTap(navItem);
            await ExecuteNavAction(navItem);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in navigation tap for {navItem}: {ex.Message}");
            await CustomAlert.ShowErrorAsync("No se pudo procesar la acción del menú", "Error de Navegación");
        }
    }

    private async Task AnimateBottomNavEntry()
    {
        try
        {
            var bottomNavBorder = this.FindByName<Border>("BottomNavContainer");

            if (bottomNavBorder != null)
            {
                bottomNavBorder.TranslationY = 120;
                bottomNavBorder.Opacity = 0;
                await Task.WhenAll(
                    bottomNavBorder.TranslateTo(0, 0, 700, Easing.SpringOut),
                    bottomNavBorder.FadeTo(1, 400, Easing.CubicOut)
                );
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error animating bottom nav entry: {ex.Message}");
        }
    }

    private async Task AnimateToActiveItem(string activeItem)
    {
        try
        {
            Debug.WriteLine($"Animating to active item: {activeItem}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error animating sliding indicator: {ex.Message}");
        }
    }

    private async Task AnimateNavItemTap(string navItem)
    {
        try
        {
            var itemGrid = this.FindByName<Grid>($"{navItem}Button");
            var itemCircle = this.FindByName<Ellipse>($"{navItem}Circle");
            if (itemGrid == null) return;

            await Task.WhenAll(
                itemGrid.ScaleTo(0.9, 80, Easing.CubicOut),
                itemCircle?.ScaleTo(0.85, 80, Easing.CubicOut) ?? Task.CompletedTask
            );
            await Task.WhenAll(
                itemGrid.ScaleTo(1.05, 120, Easing.SpringOut),
                itemCircle?.ScaleTo(1.2, 120, Easing.SpringOut) ?? Task.CompletedTask
            );
            await Task.WhenAll(
                itemGrid.ScaleTo(1, 100, Easing.CubicOut),
                itemCircle?.ScaleTo(1, 100, Easing.CubicOut) ?? Task.CompletedTask
            );
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error animating nav item tap: {ex.Message}");
        }
    }

    private async Task ExecuteNavAction(string navItem)
    {
        try
        {
            switch (navItem)
            {
                case "Document": await OpenDocumentPopUp(); break;
                case "Expense": await OpenExpenditurePopUp(); break;
                case "Info": await OpenAdditionalInfoPopUp(); break;
                case "History": await OnShowCourtListClicked(); break;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error executing nav action: {ex.Message}");
            await CustomAlert.ShowErrorAsync("No se pudo ejecutar la acción solicitada", "Error de Navegación");
        }
    }

    // ====== REFRESCO FINO DE SECCIONES (sin recargar toda la página) ======
    // ====== REFRESCO FINO DE SECCIONES (sin recargar toda la página) ======
    private async Task RefreshSectionsAsync(bool refreshDispensers, bool refreshPayments)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            // 1) Ventas por mangueras
            if (refreshDispensers)
            {
                var cvDisp = this.FindByName<CollectionView>("CourtDispensers");
                if (cvDisp != null)
                {
                    // Truco: quitar ItemsSource y togglear visibilidad para forzar render
                    var wasVisible = cvDisp.IsVisible;

                    cvDisp.ItemsSource = null;
                    cvDisp.IsVisible = false;
                    await Task.Yield();            // cede un frame al UI thread
                    cvDisp.ItemsSource = _service.CourtDispensers; // ideal: ObservableCollection<> 
                    cvDisp.IsVisible = wasVisible || true;
                }
            }

            // 2) Formas de pago
            if (refreshPayments)
            {
                var cvPays = this.FindByName<CollectionView>("CourtTypeOfCollections");
                if (cvPays != null)
                {
                    var wasVisible = cvPays.IsVisible;

                    cvPays.ItemsSource = null;
                    cvPays.IsVisible = false;
                    await Task.Yield();
                    cvPays.ItemsSource = _service.CourtTypeOfCollections; // ideal: ObservableCollection<> 
                    cvPays.IsVisible = wasVisible || true;
                }
            }

            // 3) (Opcional) recalcular tarjetas/resúmenes si el servicio no lanza PropertyChanged
            try
            {
                _service.TotalAmount = _service.GetTotalAmount();
                _service.TotalTypeOfCollection = _service.GetTotalTypeOfCollection();
                _service.TotalExpenditure = _service.GetTotalExpenditure();
                _service.TotalSales = _service.TotalAmount; // o tu fórmula real
            }
            catch { /* si alguna propiedad no existe, el rebind ya refresca la UI */ }
        });
    }

    // ======================================================================

    // --- Popups
    private async Task OpenDocumentPopUp()
    {
        if (!PuedeEditar) { await CustomAlert.ShowErrorAsync("El corte ya fue enviado.", "Corte cerrado"); return; }
        try { await ShowPopupSafelyAsync<object>(new AddDocuemt(_service)); }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error opening document popup: {ex.Message}");
            await CustomAlert.ShowErrorAsync("No se pudo abrir el formulario de documentos", "Error de Interfaz");
        }
    }

    private async Task OpenExpenditurePopUp()
    {
        if (!PuedeEditar) { await CustomAlert.ShowErrorAsync("El corte ya fue enviado.", "Corte cerrado"); return; }
        try { await ShowPopupSafelyAsync<object>(new AddCourtExpenditure(_service)); }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error opening expenditure popup: {ex.Message}");
            await CustomAlert.ShowErrorAsync("No se pudo abrir el formulario de gastos", "Error de Interfaz");
        }
    }

    private async Task OpenAdditionalInfoPopUp()
    {
        if (!PuedeEditar) { await CustomAlert.ShowErrorAsync("El corte ya fue enviado.", "Corte cerrado"); return; }
        try { await ShowPopupSafelyAsync<object>(new AddInfo(_service)); }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error opening info popup: {ex.Message}");
            await CustomAlert.ShowErrorAsync("No se pudo abrir el formulario de información adicional", "Error de Interfaz");
        }
    }

    private async Task OnShowCourtListClicked()
    {
        try { await Navigation.PushAsync(new CourtListView()); }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error navigating to court list: {ex.Message}");
            await CustomAlert.ShowErrorAsync("No se pudo abrir la lista de cierres de turno", "Error de Navegación");
        }
    }

    // --- Botones obligatorios
    private async void OpenDispenserPopUp(object sender, EventArgs e)
    {
        if (!PuedeEditar) { await CustomAlert.ShowErrorAsync("El corte ya fue enviado.", "Corte cerrado"); return; }
        try
        {
            await ShowPopupSafelyAsync<object>(new AddDispenser(_service));
            // Refrescar SOLO ventas por mangueras
            await RefreshSectionsAsync(refreshDispensers: true, refreshPayments: false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error opening dispenser popup: {ex.Message}");
            await CustomAlert.ShowErrorAsync("No se pudo abrir el formulario del dispensador", "Error de Interfaz");
        }
    }

    private async void OpenTypeOfCollectionPopUp(object sender, EventArgs e)
    {
        if (!PuedeEditar) 
        { 
            await CustomAlert.ShowErrorAsync("El corte ya fue enviado.", "Corte cerrado"); 
            return; 
        }

        try
        {
            // **✨ NUEVA VALIDACIÓN: Verificar que haya al menos una venta antes de agregar formas de pago**
            double totalSales = _service.GetTotalAmount();
            
            if (totalSales <= 0)
            {
                await CustomAlert.ShowWarningAsync(
                    "📋 Sin Ventas Registradas\n\n" +
                    "Debe agregar al menos una venta antes de configurar métodos de pago.\n\n" +
                    "• Agregue dispensadores con ventas primero\n" +
                    "• Luego configure los métodos de pago correspondientes\n\n" +
                    "Esto asegura que los métodos de pago coincidan con las ventas realizadas.",
                    "Ventas Requeridas");
                return;
            }

            // Verificar si hay dispensadores agregados (validación adicional más específica)
            if (_service.CourtDispensers == null || !_service.CourtDispensers.Any())
            {
                await CustomAlert.ShowWarningAsync(
                    "🚫 Dispensadores Requeridos\n\n" +
                    "No se han registrado dispensadores con ventas.\n\n" +
                    $"• Total de ventas actual: ${totalSales:N2}\n" +
                    "• Dispensadores registrados: 0\n\n" +
                    "Por favor, agregue al menos un dispensador con ventas antes de configurar métodos de pago.",
                    "Agregar Dispensadores Primero");
                return;
            }

            // Si hay ventas, proceder normalmente con el popup
            await ShowPopupSafelyAsync<object>(new AddCourtTypeOfCollection(_service));
            // Refrescar SOLO formas de pago
            await RefreshSectionsAsync(refreshDispensers: false, refreshPayments: true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error opening collection popup: {ex.Message}");
            await CustomAlert.ShowErrorAsync("No se pudo abrir el formulario de tipos de cobro", "Error de Interfaz");
        }
    }

    // --- Envío del corte
    private async void Button_Clicked(object sender, EventArgs e)
    {
        var btn = sender as Button;                // una sola variable -> sin CS0136
        if (btn != null) btn.IsEnabled = false;

        try
        {
            if (BindingContext is not CourtService vm) return;

            // --- Reglas para Administrador (datos maestros) ---
            if (UserRole == "Admin")
            {
                if (vm.SelectedBusiness is null)
                {
                    await CustomAlert.ShowErrorAsync(
                        "Debe seleccionar un negocio para continuar con el cierre.",
                        "Negocio requerido");
                    return;
                }
                if (vm.SelectedEds is null)
                {
                    await CustomAlert.ShowErrorAsync(
                        "Debe seleccionar una EDS para continuar.",
                        "EDS requerida");
                    return;
                }
                if (vm.SelectedIslander is null)
                {
                    await CustomAlert.ShowErrorAsync(
                        "Debe seleccionar un islero responsable.",
                        "Islero requerido");
                    return;
                }
            }

            // --- Totales actuales ---
            double totalAmount = vm.GetTotalAmount();                  // ventas (dinero)
            double totalTypeOfCollection = vm.GetTotalTypeOfCollection(); // formas de pago
            double totalExpenditures = vm.GetTotalExpenditure();       // gastos
            double cash = totalTypeOfCollection - totalExpenditures;

            // 1) Debe existir al menos un método de pago
            if (vm.CourtTypeOfCollections == null || !vm.CourtTypeOfCollections.Any())
            {
                await CustomAlert.ShowErrorAsync(
                    "Debe registrar al menos un método de pago antes de enviar.",
                    "Medios de pago requeridos");
                return;
            }

            // 2) Si no hay ventas/dispensadores, NO bloqueamos el envío; solo log informativo
            if (vm.CourtDispensers == null || !vm.CourtDispensers.Any())
            {
                Debug.WriteLine("Aviso: enviando corte sin ventas registradas (dispensadores vacíos).");
            }

            // 3) Efectivo no puede ser negativo
            const double epsilon = 1e-6;
            if (cash < -epsilon)
            {
                await CustomAlert.ShowErrorAsync(
                    $"El total de efectivo no puede ser negativo.\n\n" +
                    $"Recaudo: ${totalTypeOfCollection:F2}\n" +
                    $"Gastos : ${totalExpenditures:F2}",
                    "Error en cálculos");
                return;
            }

            // --- Envío ---
            var overlay = this.FindByName<LoadingView.LoadingView>("LoadingOverlay");
            try { overlay?.ShowLoading(); } catch { }

            await vm.SendCourtDataAsync();

            try { overlay?.HideLoading(); } catch { }

            if (vm.LastSendWasSuccessful)
            {
                // Evitar doble submit inmediatamente
                OcultarSeccionesCierre();

                // Preparar NUEVO flujo de cierre (rehabilita y muestra todo)
                await ResetForNewCloseAsync();
            }
            else
            {
                await CustomAlert.ShowErrorAsync(
                    "No se pudo completar el envío del corte.",
                    "Envío fallido");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in Button_Clicked: {ex.Message}");
            await CustomAlert.ShowErrorAsync(
                $"Ocurrió un error al enviar los datos del cierre:\n\n{ex.Message}",
                "Error del sistema");
        }
        finally
        {
            if (btn != null) btn.IsEnabled = true; // re-habilita el MISMO botón
        }
    }

    /// <summary>
    /// Rehabilita botones y secciones para un NUEVO cierre de turno,
    /// reseteando el servicio y restableciendo las banderas de edición/visibilidad.
    /// </summary>
    /// 
    // ✅ CORREGIDO: Lógica diferente para Admin vs Usuario normal
    public bool AccionesHabilitadas => PuedeEditar && CanAccessFunctionality;

    // ✅ NUEVA PROPIEDAD: Determina si el usuario puede acceder a la funcionalidad
    public bool CanAccessFunctionality
    {
        get
        {
            if (UserRole == "Admin")
            {
                // Administradores necesitan seleccionar negocio
                return IsBusinessSelected;
            }
            else
            {
                // Usuarios normales pueden acceder siempre (se asume que ya tienen asignada su EDS)
                return true;
            }
        }
    }

    private async Task ResetForNewCloseAsync()
    {
        var prevBusiness = _service.SelectedBusiness;
        var prevEds = _service.SelectedEds;
        var prevIslander = _service.SelectedIslander;

        CourtService.ResetInstanceFields();
        _service = CourtService.Instance;
        BindingContext = _service;

        if (prevBusiness != null)
        {
            _service.SelectedBusiness = prevBusiness;
            IsBusinessSelected = true; 
        }

        if (prevEds != null) _service.SelectedEds = prevEds;
        if (prevIslander != null) _service.SelectedIslander = prevIslander;

        SetEditingState(canEdit: true, showSections: true);

        OnPropertyChanged(nameof(AccionesHabilitadas));
        OnPropertyChanged(nameof(CanAccessFunctionality));

        try { await _service.GetAllEdsData(); } catch { }
    }


    // Oculta secciones tras envío y bloquea edición (para evitar doble click)
    private void OcultarSeccionesCierre()
    {
        SetEditingState(canEdit: false, showSections: false);

        // 🔥 YA NO ES NECESARIO: La visibilidad ahora se controla automáticamente por las propiedades del servicio
        // Las secciones se ocultarán automáticamente cuando las colecciones estén vacías después del reset
    }

    // --- Pickers
    private void OnBusinessSelected(object sender, EventArgs e)
    {
        if (sender is not Picker picker) return;

        Dispatcher.Dispatch(async () =>
        {
            try
            {
                if (picker.SelectedItem is BusinessModel selectedBusiness)
                {
                    _service.LoadEdsByBusiness(selectedBusiness.IdBusiness);
                    _service.IslanderSelectList.Clear();
                    _service.IsBusinessSelected = true;

                    IsBusinessSelected = true;
                    OnPropertyChanged(nameof(AccionesHabilitadas));
                    OnPropertyChanged(nameof(CanAccessFunctionality));

                    await ShowOperationalSectionsAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnBusinessSelected: {ex.Message}");
            }
        });
    }


    private void OnEdsSelected(object sender, EventArgs e)
    {
        if (sender is not Picker picker) return;

        Dispatcher.Dispatch(async () =>
        {
            try
            {
                if (picker.SelectedItem is EdsCourtModel selectedEds)
                {
                    _service.LoadIslandersByEds(selectedEds.IdEds);
                    _service.IsEdsSelected = true;

                    // 👇 Mostrar secciones al elegir EDS
                    await ShowOperationalSectionsAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnEdsSelected: {ex.Message}");
            }
        });
    }

    private void OnIslanderSelected(object sender, EventArgs e)
    {
        if (sender is not Picker picker) return;

        Dispatcher.Dispatch(async () =>
        {
            try
            {
                if (picker.SelectedItem is IslanderResponse selectedIslander)
                {
                    // (si necesitas cargar algo extra, hazlo aquí)

                    // 👇 Mostrar secciones al elegir Islero
                    await ShowOperationalSectionsAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnIslanderSelected: {ex.Message}");
            }
        });
    }


    // INotifyPropertyChanged local para x:Reference CortePage
    public new event PropertyChangedEventHandler? PropertyChanged;
    protected new virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    // Mostrar popups con seguridad
    private static async Task<T> ShowPopupSafelyAsync<T>(Popup popup) where T : class
    {
        try
        {
            var result = await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try { return await Application.Current.MainPage.ShowPopupAsync(popup) as T; }
                catch (ObjectDisposedException ex) { Debug.WriteLine($"Popup disposed: {ex.Message}"); return null; }
                catch (Exception ex) { Debug.WriteLine($"Show popup error: {ex.Message}"); return null; }
            });

            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in ShowPopupSafelyAsync: {ex.Message}");
            return null;
        }
    }


        private BusinessDto _selectedBusiness;
        public BusinessDto SelectedBusiness
        {
            get => _selectedBusiness;
            set
            {
                if (_selectedBusiness != value)
                {
                    _selectedBusiness = value;
                    OnPropertyChanged(nameof(SelectedBusiness));
                    IsBusinessSelected = _selectedBusiness != null;
                }
            }
        }

        private bool _isBusinessSelected;
        public bool IsBusinessSelected
        {
            get => _isBusinessSelected;
            set
            {
                if (_isBusinessSelected != value)
                {
                    _isBusinessSelected = value;
                    OnPropertyChanged(nameof(IsBusinessSelected));
                    OnPropertyChanged(nameof(AccionesHabilitadas));
                    OnPropertyChanged(nameof(CanAccessFunctionality));
                }
            }
        }
}
