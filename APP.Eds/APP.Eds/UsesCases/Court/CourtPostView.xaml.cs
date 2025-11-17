using APP.Eds.Components.PopUp;
using APP.Eds.Models.Court;
using APP.Eds.Models.Dispensers;
using APP.Eds.Models.Eds;
using APP.Eds.Services.Court;
using APP.Eds.Services.RegisterShift;
using APP.Eds.UsesCases.Court.APP.Eds.Models.Business;
using APP.Eds.UsesCases.LoadingView;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Storage;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace APP.Eds.UsesCases.Court;

public partial class CourtPostView : ContentPage, INotifyPropertyChanged
{
    public RegisterShiftUserService _registerShiftUserService;
    public RegisterShiftUserService RegisterShiftView { get; private set; }
    public RegisterShiftAdminService _registerShiftAdminService;

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

    private bool _isregisterShiftChecked;
    public bool IsRegisterShiftChecked
    {
               get => _isregisterShiftChecked;
        set
        {
            if (_isregisterShiftChecked != value)
            {
                _isregisterShiftChecked = value;
                OnPropertyChanged(nameof(IsRegisterShiftChecked));
            }
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
        _registerShiftUserService = RegisterShiftUserService.Instance;
        RegisterShiftView = _registerShiftUserService;
        _registerShiftAdminService = RegisterShiftAdminService.Instance;

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
            await _registerShiftUserService.LoadIslanderAsync();
            RegisterShiftView.RefreshIslanderCommand.Execute(null);
            if (mainContent != null) mainContent.IsVisible = false;

            // Mostrar/ocultar tarjeta Business por rol (Admin la ve)
            var businessBorder = this.FindByName<Border>("Business");
            if (businessBorder != null)
                businessBorder.IsVisible = (UserRole == "Admin");

            var edsUser = this.FindByName<Border>("EdsUser");
            if (edsUser != null)
                edsUser.IsVisible = (UserRole == "User");

            await _service.LoadTranslationsAsync();
            await AnimateBottomNavEntry();
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error al cargar datos:\n\n{ex.Message}", "Error de Carga");
            Debug.WriteLine($"Error inicializando RegisterShiftUserService: {ex.Message}");
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
        // Prevent double-click by disabling the button immediately
        var button = sender as Button;
        if (button != null && !button.IsEnabled) return;
        if (button != null) button.IsEnabled = false;

        try
        {
            if (!PuedeEditar)
            {
                await CustomAlert.ShowErrorAsync("El corte ya fue enviado.", "Corte cerrado");
                return;
            }

            await ShowPopupSafelyAsync<object>(new AddDispenser(_service));
            // Refrescar SOLO ventas por mangueras
            await RefreshSectionsAsync(refreshDispensers: true, refreshPayments: false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error opening dispenser popup: {ex.Message}");
            await CustomAlert.ShowErrorAsync("No se pudo abrir el formulario del dispensador", "Error de Interfaz");
        }
        finally
        {
            // Re-enable the button after a short delay to prevent rapid consecutive clicks
            if (button != null)
            {
                await Task.Delay(500); // 500ms delay before re-enabling
                button.IsEnabled = true;
            }
        }
    }

    private async void OpenTypeOfCollectionPopUp(object sender, EventArgs e)
    {
        // Prevent double-click by disabling the button immediately
        var button = sender as Button;
        if (button != null && !button.IsEnabled) return;
        if (button != null) button.IsEnabled = false;

        try
        {
            if (!PuedeEditar)
            {
                await CustomAlert.ShowErrorAsync("El corte ya fue enviado.", "Corte cerrado");
                return;
            }

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
        finally
        {
            // Re-enable the button after a short delay to prevent rapid consecutive clicks
            if (button != null)
            {
                await Task.Delay(500); // 500ms delay before re-enabling
                button.IsEnabled = true;
            }
        }
    }

    private void OnEdsUserSelected (object sender, EventArgs e)
    {
        if (sender is not Picker picker) return;
        if (UserRole == "User" && picker.SelectedItem is EdsResponse selected)
        {
            _registerShiftUserService.SelectedUserEds = selected;
        }
    }

    // --- Envío del corte
    private async void Button_Clicked(object sender, EventArgs e)
    {
        var btn = sender as Button;
        // una sola variable -> sin CS0136
        if (btn != null) btn.IsEnabled = false;

        try
        {
            if (BindingContext is not CourtService vm) return;

            // Servicio RegisterShift para User
            var check = this.FindByName<CheckBox>("RegisterShiftCheck");
            bool registerShift = check?.IsChecked ?? false;

            if (string.Equals(UserRole, "User", StringComparison.OrdinalIgnoreCase) && registerShift)
            {
                _registerShiftUserService.DateStart = vm.DateStarttime;
                _registerShiftUserService.DateEnd = vm.DateEndtime; // o (vm.Endtime < vm.Starttime ? vm.DateStarttime.AddDays(1) : vm.DateStarttime)
                _registerShiftUserService.StartTime = vm.Starttime;
                _registerShiftUserService.EndTime = vm.Endtime;

                var overlayRegisterShift = this.FindByName<LoadingView.LoadingView>("LoadingOverlay");
                try { overlayRegisterShift?.ShowLoading(); } catch { }

                await _registerShiftUserService.SaveRegisterShiftAsync();

                try { overlayRegisterShift?.HideLoading(); } catch { }
                return; // no continuar con el flujo normal
            }
            // Servicio para Admin
            if (string.Equals(UserRole, "Admin", StringComparison.OrdinalIgnoreCase) && registerShift)
            {
                _registerShiftAdminService.IdEds = vm.IdEds;
                _registerShiftAdminService.IdBusiness = vm.IdBusiness;
                _registerShiftAdminService.IdIslander = vm.IdIslander;
                _registerShiftAdminService.DateStart = vm.DateStarttime;
                _registerShiftAdminService.DateEnd = vm.DateEndtime; 
                _registerShiftAdminService.StartTime = vm.Starttime;
                _registerShiftAdminService.EndTime = vm.Endtime;

                if (vm.IdEds <= 0 || vm.IdBusiness <= 0 || vm.IdIslander <= 0)
                {
                    await CustomAlert.ShowErrorAsync(
                        "Debe Seleccionar los tres Campos iniciales Negocio, EDS e Ilsero.",
                        "Campos Vacios");
                    return;
                }

                var overlayRegisterShift = this.FindByName<LoadingView.LoadingView>("LoadingOverlay");
                try { overlayRegisterShift?.ShowLoading(); } catch { }

                await _registerShiftAdminService.SaveRegisterShiftAsync();

                try { overlayRegisterShift?.HideLoading(); } catch { }
                return; // no continuar con el flujo normal
            }


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

            // 4) Validar tamaño de archivos adjuntos antes de enviar
            if (vm.CourtDocuments != null && vm.CourtDocuments.Any())
            {
                const long MAX_FILE_SIZE = 25 * 1024 * 1024; // 5 MB por archivo
                const long MAX_TOTAL_SIZE = 25 * 1024 * 1024; // 10 MB total
                long totalDocumentsSize = 0;

                foreach (var doc in vm.CourtDocuments)
                {
                    try
                    {
                        // Calcular tamaño aproximado del archivo desde Base64
                        var base64Length = doc.Descripcion?.Length ?? 0;
                        if (base64Length == 0) continue;

                        int padding = doc.Descripcion.EndsWith("==") ? 2 : doc.Descripcion.EndsWith("=") ? 1 : 0;
                        long fileSize = (long)((base64Length * 3) / 4) - padding;
                        totalDocumentsSize += fileSize;

                        // Validar tamaño individual
                        if (fileSize > MAX_FILE_SIZE)
                        {
                            await CustomAlert.ShowErrorAsync(
                                $"⚠️ Archivo Demasiado Grande\n\n" +
                                $"El archivo '{doc.DocumentName}' excede el límite permitido.\n\n" +
                                $"• Tamaño del archivo: {fileSize / (1024.0 * 1024.0):0.##} MB\n" +
                                $"• Límite por archivo: {MAX_FILE_SIZE / (1024.0 * 1024.0):0.##} MB\n\n" +
                                $"Por favor, elimine este archivo o cargue una versión más pequeña antes de enviar el cierre.",
                                "Validación de Archivos");
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error validating document size: {ex.Message}");
                    }
                }

                // Validar tamaño total
                if (totalDocumentsSize > MAX_TOTAL_SIZE)
                {
                    await CustomAlert.ShowErrorAsync(
                        $"⚠️ Tamaño Total de Archivos Excedido\n\n" +
                        $"El tamaño total de los archivos adjuntos supera el límite permitido por el servidor.\n\n" +
                        $"• Tamaño total: {totalDocumentsSize / (1024.0 * 1024.0):0.##} MB\n" +
                        $"• Límite máximo: {MAX_TOTAL_SIZE / (1024.0 * 1024.0):0.##} MB\n" +
                        $"• Archivos adjuntos: {vm.CourtDocuments.Count}\n\n" +
                        $"💡 Para continuar:\n" +
                        $"• Elimine algunos documentos adjuntos\n" +
                        $"• Comprima las imágenes o archivos PDF\n" +
                        $"• Divida los documentos en múltiples cierres\n\n" +
                        $"Use el botón 'Eliminar Todos' en la sección de comprobantes para limpiar los adjuntos.",
                        "Validación de Tamaño");
                    return;
                }
            }

            // --- Envío ---
            var overlay = this.FindByName<LoadingView.LoadingView>("LoadingOverlay");
            try { overlay?.ShowLoading(); } catch { }

            await vm.SendCourtDataAsync();

            try { overlay?.HideLoading(); } catch { }

            if (vm.LastSendWasSuccessful)
            {
                // ✅ Mensaje de éxito profesional y detallado
                var successMessage = BuildSuccessMessage(totalAmount, totalTypeOfCollection, totalExpenditures);
                await CustomAlert.ShowSuccessAsync(successMessage, "✅ Corte Enviado Exitosamente");

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
    /// Construye un mensaje de éxito profesional y detallado para el envío del corte
    /// </summary>
    private string BuildSuccessMessage(double totalAmount, double totalTypeOfCollection, double totalExpenditures)
    {
        var message = new System.Text.StringBuilder();
        
        message.AppendLine("El corte de turno ha sido registrado correctamente en el sistema.");
        message.AppendLine();
        message.AppendLine("📊 RESUMEN DEL CIERRE:");
        message.AppendLine();
        
        // Información de ventas
        if (_service.CourtDispensers?.Any() == true)
        {
            message.AppendLine($"⛽ Ventas por Mangueras: {_service.CourtDispensers.Count} registro(s)");
            message.AppendLine($"   • Total en dinero: ${totalAmount:N2}");
            message.AppendLine($"   • Total en galones: {_service.GetTotalGallons():N2}");
            message.AppendLine();
        }
        
        // Métodos de pago
        if (_service.CourtTypeOfCollections?.Any() == true)
        {
            message.AppendLine($"💳 Métodos de Pago: {_service.CourtTypeOfCollections.Count} método(s)");
            message.AppendLine($"   • Total recaudado: ${totalTypeOfCollection:N2}");
            
            // Detallar métodos de pago
            foreach (var payment in _service.CourtTypeOfCollections)
            {
                message.AppendLine($"   • {payment.TypeOfCollectionName}: ${payment.Amount:N2}");
            }
            message.AppendLine();
        }
        
        // Gastos
        if (_service.CourtExpenditures?.Any() == true)
        {
            message.AppendLine($"💸 Gastos Registrados: {_service.CourtExpenditures.Count} gasto(s)");
            message.AppendLine($"   • Total de gastos: ${totalExpenditures:N2}");
            message.AppendLine();
        }
        
        // Documentos adjuntos
        if (_service.CourtDocuments?.Any() == true)
        {
            message.AppendLine($"📎 Documentos Adjuntos: {_service.CourtDocuments.Count} archivo(s)");
            message.AppendLine();
        }
        
        // Efectivo en caja
        double cash = totalTypeOfCollection - totalExpenditures;
        message.AppendLine("💰 EFECTIVO FINAL:");
        message.AppendLine($"   ${cash:N2}");
        message.AppendLine();
        
        // Validación de cuadratura
        var tolerance = 0.01;
        if (Math.Abs(totalAmount - totalTypeOfCollection) <= tolerance)
        {
            message.AppendLine("✅ VALIDACIÓN: Cuadratura exitosa");
            message.AppendLine("   Los métodos de pago coinciden con las ventas registradas.");
        }
        else
        {
            var difference = totalAmount - totalTypeOfCollection;
            message.AppendLine($"⚠️ VALIDACIÓN: Diferencia de ${Math.Abs(difference):N2}");
            message.AppendLine(difference > 0 
                ? "   (Ventas mayores a métodos de pago)" 
                : "   (Métodos de pago mayores a ventas)");
        }
        
        return message.ToString();
    }


    /// <summary>
    /// Rehabilita botones y secciones para un NUEVO cierre de turno,
    /// reseteando el servicio y restableciendo las banderas de edición/visibilidad.
    /// </summary>
    /// 
    // ✅ CORREGIDO: Lógica diferente para Admin vs Usuario normal
    public bool AccionesHabilitadas => PuedeEditar && CanAccessFunctionality;

    // ✅ CORREGIDO: Los botones se habilitan cuando hay EDS O Islander seleccionado
    public bool CanAccessFunctionality
    {
        get
        {
            if (UserRole == "Admin")
            {
                // ✅ CORREGIDO: Admin solo necesita seleccionar Negocio Y (EDS O Islander)
                return IsBusinessSelected && (_service.IsEdsSelected || _service.SelectedIslander != null);
            }
            else
            {
                // Usuarios normales siempre pueden acceder
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

                    // ✅ CORREGIDO: Sincronizar ambas propiedades
                    IsBusinessSelected = true;  // La propiedad local de la página
                    
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

                    // ✅ CORREGIDO: Notificar cambio para habilitar botones
                    OnPropertyChanged(nameof(CanAccessFunctionality));
                    OnPropertyChanged(nameof(AccionesHabilitadas));

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
                    // ✅ CORREGIDO: Notificar cambio para habilitar botones
                    OnPropertyChanged(nameof(CanAccessFunctionality));
                    OnPropertyChanged(nameof(AccionesHabilitadas));

                    await ShowOperationalSectionsAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnIslanderSelected: {ex.Message}");
            }
        });
    }

    private async void OnRegisterShiftCheckChanged(object sender, CheckedChangedEventArgs e)
    {
        _isregisterShiftChecked = e.Value;
        if (!e.Value) return;
        //if (!_isregisterShiftChecked || !string.Equals(UserRole, "User", StringComparison.OrdinalIgnoreCase))
        //    return;

        var sendButton = this.FindByName<Button>("SendData");
        if (sendButton != null)
        {
            sendButton.Text = _isregisterShiftChecked ? "Registrar Turno" : "Enviar";
        }
        else if (sendButton == null)
        {
            sendButton.Text = _isregisterShiftChecked ? "Enviar Datos" : "Envair";
        }
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

    private async void OnRemoveAllDocumentsClicked(object sender, EventArgs e)
    {
        try
        {
            if (_service.CourtDocuments == null || !_service.CourtDocuments.Any())
                return;

            _service.CourtDocuments.Clear();

            // Alinear el modelo Court
            if (_service.Court != null)
                _service.Court.CourtDocuments = _service.CourtDocuments.ToList();

            // Ocultar la sección si quedó vacía
            _service.VisibleDocuments = false;

            await CustomAlert.ShowSuccessAsync("Todos los comprobantes fueron eliminados.", "Comprobantes");
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"No se pudieron eliminar los documentos:\n\n{ex.Message}", "Error");
        }
    }
}
