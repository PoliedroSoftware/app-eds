using APP.Eds.Components.PopUp;
using APP.Eds.Models.Court;
using APP.Eds.Models.Dispensers;
using APP.Eds.Models.Eds;
using APP.Eds.Services.Court;
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
    private CourtService _service;
    public string UserRole { get; set; } = string.Empty;

    // === NUEVO: control de visibilidad / edición de secciones ===
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
    // ============================================================

    // Propiedad para el elemento activo del menú
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
        // Inicialización manual si InitializeComponent no está disponible
        try
        {
            InitializeComponent();
        }
        catch
        {
            // Fallback manual initialization
            Title = "Cierre De Turno";
            BackgroundColor = Color.FromArgb("#F8F9FA");
            Content = CreateContent();
        }

        _service = CourtService.Instance;
        _service.DateStarttime = DateTime.Today;

        // Mantén el BindingContext en el servicio (no romper Court.* bindings)
        BindingContext = _service;

        // Estado inicial: visible y editable
        SeccionesVisibles = true;
        PuedeEditar = true;

        // Configurar DatePicker después de la inicialización
        ConfigureDatePickerAsync();

        Task.Run(async () => await _service.LoadTranslationsAsync());

        UserRole = Preferences.Get("userRole", string.Empty);

        string configJson = Preferences.Get("userConfig", "{}");
        var config = JsonSerializer.Deserialize<Dictionary<string, bool>>(configJson);

        if (config != null)
        {
            ApplyConfig(config);
        }

        // Establecer el elemento activo inicial
        ActiveNavItem = "Document";
    }

    private async void ConfigureDatePickerAsync()
    {
        await Task.Run(async () =>
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                try
                {
                    var picker = this.FindByName<DatePicker>("datePicker");
                    if (picker != null)
                    {
                        picker.MinimumDate = new DateTime(1900, 1, 1);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error setting datePicker MinimumDate: {ex.Message}");
                }
            });
        });
    }

    private View CreateContent()
    {
        // Crear contenido básico si no se puede usar InitializeComponent
        return new StackLayout
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
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            // Manejo seguro de LoadingOverlay
            var loadingOverlay = this.FindByName<LoadingView.LoadingView>("LoadingOverlay");
            try { loadingOverlay?.ShowLoading(); } catch { }

            // Manejo seguro de MainContent
            var mainContent = this.FindByName<ScrollView>("MainContent");
            try { if (mainContent != null) mainContent.IsVisible = false; } catch { }

            // Manejo seguro de Business visibility
            var businessBorder = this.FindByName<Border>("Business");
            try
            {
                if (businessBorder != null)
                    businessBorder.IsVisible = (UserRole is "Admin");
            }
            catch { }

            await _service.LoadTranslationsAsync();

            // Animar la entrada del menú
            await AnimateBottomNavEntry();
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error al cargar datos:\n\n{ex.Message}", "Error de Carga");
        }
        finally
        {
            // Manejo seguro de LoadingOverlay y MainContent
            var loadingOverlay = this.FindByName<LoadingView.LoadingView>("LoadingOverlay");
            var mainContent = this.FindByName<ScrollView>("MainContent");
            try { loadingOverlay?.HideLoading(); } catch { }
            try { if (mainContent != null) mainContent.IsVisible = true; } catch { }
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
    }

    private void ApplyConfig(Dictionary<string, bool> config)
    {
        foreach (var kvp in config)
        {
            var element = this.FindByName<VisualElement>(kvp.Key);
            if (element != null)
            {
                element.IsVisible = kvp.Value;
            }
        }
    }

    /// <summary>
    /// Métodos específicos para cada botón del menú inferior
    /// </summary>
    private async void OnDocumentTapped(object sender, EventArgs e)
    {
        await HandleNavTap("Document");
    }

    private async void OnExpenseTapped(object sender, EventArgs e)
    {
        await HandleNavTap("Expense");
    }

    private async void OnInfoTapped(object sender, EventArgs e)
    {
        await HandleNavTap("Info");
    }

    private async void OnHistoryTapped(object sender, EventArgs e)
    {
        await HandleNavTap("History");
    }

    /// <summary>
    /// Maneja los taps en el menú de navegación inferior (método centralizado)
    /// </summary>
    private async Task HandleNavTap(string navItem)
    {
        try
        {
            if (string.IsNullOrEmpty(navItem))
            {
                Debug.WriteLine("NavItem is null or empty");
                return;
            }

            // 🚫 bloqueo de navegación/edición tras enviar
            if (!PuedeEditar)
            {
                await CustomAlert.ShowErrorAsync("El corte ya fue enviado. Edición bloqueada.", "Corte cerrado");
                return;
            }

            Debug.WriteLine($"Navigation tap: {navItem}");

            ActiveNavItem = navItem;

            // Animar el tap
            await AnimateNavItemTap(navItem);

            // Ejecutar la acción correspondiente
            await ExecuteNavAction(navItem);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in navigation tap for {navItem}: {ex.Message}");
            await CustomAlert.ShowErrorAsync("No se pudo procesar la acción del menú", "Error de Navegación");
        }
    }

    /// <summary>
    /// Maneja los taps en el menú de navegación inferior (método original mantenido por compatibilidad)
    /// </summary>
    private async void OnBottomNavTapped(object sender, EventArgs e)
    {
        try
        {
            string navItem = null;

            if (sender is TapGestureRecognizer tapGesture && tapGesture.CommandParameter is string commandParam)
            {
                navItem = commandParam;
            }
            else if (sender is View viewElement && viewElement.GestureRecognizers.FirstOrDefault() is TapGestureRecognizer gesture && gesture.CommandParameter is string param)
            {
                navItem = param;
            }
            else if (e is TappedEventArgs tappedArgs && tappedArgs.Parameter is string tappedParam)
            {
                navItem = tappedParam;
            }

            if (string.IsNullOrEmpty(navItem))
            {
                Debug.WriteLine("No se pudo determinar el elemento de navegación");
                return;
            }

            await HandleNavTap(navItem);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in bottom nav tap: {ex.Message}");
            await CustomAlert.ShowErrorAsync("No se pudo procesar la acción del menú", "Error de Navegación");
        }
    }

    /// <summary>
    /// Anima la entrada del menú inferior con efectos mejorados
    /// </summary>
    private async Task AnimateBottomNavEntry()
    {
        try
        {
            var slidingBackground = this.FindByName<Border>("SlidingBackground");
            var bottomNavBorder = slidingBackground?.Parent?.Parent as Border;

            if (bottomNavBorder != null)
            {
                // Inicializar posición fuera de pantalla
                bottomNavBorder.TranslationY = 120;
                bottomNavBorder.Opacity = 0;

                // Animar entrada del menú completo
                await Task.WhenAll(
                    bottomNavBorder.TranslateTo(0, 0, 700, Easing.SpringOut),
                    bottomNavBorder.FadeTo(1, 400, Easing.CubicOut)
                );
            }

            // Animar el indicador deslizante después de un breve delay
            if (slidingBackground != null)
            {
                await Task.Delay(200);
                slidingBackground.Opacity = 0;
                await slidingBackground.FadeTo(0.15, 300, Easing.CubicOut);

                // Inicializar posición del indicador
                await AnimateToActiveItem(ActiveNavItem);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error animating bottom nav entry: {ex.Message}");
        }
    }

    /// <summary>
    /// Anima el indicador deslizante hacia el elemento activo
    /// </summary>
    private async Task AnimateToActiveItem(string activeItem)
    {
        try
        {
            var slidingBackground = this.FindByName<Border>("SlidingBackground");
            if (slidingBackground == null) return;

            // Calcular la posición del indicador basada en el elemento activo
            double targetX = activeItem switch
            {
                "Document" => 0,
                "Expense" => 1,
                "Info" => 2,
                "History" => 3,
                _ => 0
            };

            // Obtener el ancho de la pantalla y calcular la posición
            var screenWidth = Application.Current?.MainPage?.Width ?? 400;
            var itemWidth = screenWidth / 4;
            var indicatorPosition = (targetX * itemWidth) + (itemWidth / 2) - 28; // Centrar el círculo

            // Animar el background deslizante con efecto suave
            await Task.WhenAll(
                slidingBackground.TranslateTo(indicatorPosition, 0, 350, Easing.CubicOut),
                slidingBackground.ScaleTo(1.1, 200, Easing.SpringOut)
            );

            await slidingBackground.ScaleTo(1.0, 150, Easing.SpringIn);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error animating sliding indicator: {ex.Message}");
        }
    }

    /// <summary>
    /// Anima el elemento del menú cuando se toca con efectos mejorados
    /// </summary>
    private async Task AnimateNavItemTap(string navItem)
    {
        try
        {
            var itemGrid = this.FindByName<Grid>($"{navItem}Button");
            var itemCircle = this.FindByName<Ellipse>($"{navItem}Circle");

            if (itemGrid == null) return;

            // Animación de pulso con escalado del círculo
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

    /// <summary>
    /// Ejecuta la acción correspondiente al elemento del menú
    /// </summary>
    private async Task ExecuteNavAction(string navItem)
    {
        try
        {
            switch (navItem)
            {
                case "Document":
                    if (!PuedeEditar) { await CustomAlert.ShowErrorAsync("El corte ya fue enviado.", "Corte cerrado"); return; }
                    await OpenDocumentPopUp();
                    break;
                case "Expense":
                    if (!PuedeEditar) { await CustomAlert.ShowErrorAsync("El corte ya fue enviado.", "Corte cerrado"); return; }
                    await OpenExpenditurePopUp();
                    break;
                case "Info":
                    if (!PuedeEditar) { await CustomAlert.ShowErrorAsync("El corte ya fue enviado.", "Corte cerrado"); return; }
                    await OpenAdditionalInfoPopUp();
                    break;
                case "History":
                    await OnShowCourtListClicked();
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error executing nav action: {ex.Message}");
            await CustomAlert.ShowErrorAsync("No se pudo ejecutar la acción solicitada", "Error de Navegación");
        }
    }

    private async Task OpenDocumentPopUp()
    {
        if (!PuedeEditar) { await CustomAlert.ShowErrorAsync("El corte ya fue enviado.", "Corte cerrado"); return; }
        try
        {
            var popup = new AddDocuemt(_service);
            await ShowPopupSafelyAsync<object>(popup);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error opening document popup: {ex.Message}");
            await CustomAlert.ShowErrorAsync("No se pudo abrir el formulario de documentos", "Error de Interfaz");
        }
    }

    private async Task OpenExpenditurePopUp()
    {
        if (!PuedeEditar) { await CustomAlert.ShowErrorAsync("El corte ya fue enviado.", "Corte cerrado"); return; }
        try
        {
            var popup = new AddCourtExpenditure(_service);
            await ShowPopupSafelyAsync<object>(popup);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error opening expenditure popup: {ex.Message}");
            await CustomAlert.ShowErrorAsync("No se pudo abrir el formulario de gastos", "Error de Interfaz");
        }
    }

    private async Task OpenAdditionalInfoPopUp()
    {
        if (!PuedeEditar) { await CustomAlert.ShowErrorAsync("El corte ya fue enviado.", "Corte cerrado"); return; }
        try
        {
            var popup = new AddInfo(_service);
            await ShowPopupSafelyAsync<object>(popup);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error opening info popup: {ex.Message}");
            await CustomAlert.ShowErrorAsync("No se pudo abrir el formulario de información adicional", "Error de Interfaz");
        }
    }

    private async Task OnShowCourtListClicked()
    {
        try
        {
            await Navigation.PushAsync(new CourtListView());
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error navigating to court list: {ex.Message}");
            await CustomAlert.ShowErrorAsync("No se pudo abrir la lista de cierres de turno", "Error de Navegación");
        }
    }

    // Métodos existentes adaptados
    private async void OpenDispenserPopUp(object sender, EventArgs e)
    {
        if (!PuedeEditar) { await CustomAlert.ShowErrorAsync("El corte ya fue enviado.", "Corte cerrado"); return; }
        try
        {
            var popup = new AddDispenser(_service);
            await ShowPopupSafelyAsync<object>(popup);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error opening dispenser popup: {ex.Message}");
            await CustomAlert.ShowErrorAsync("No se pudo abrir el formulario del dispensador", "Error de Interfaz");
        }
    }

    private async void OpenTypeOfCollectionPopUp(object sender, EventArgs e)
    {
        if (!PuedeEditar) { await CustomAlert.ShowErrorAsync("El corte ya fue enviado.", "Corte cerrado"); return; }
        try
        {
            var popup = new AddCourtTypeOfCollection(_service);
            await ShowPopupSafelyAsync<object>(popup);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error opening collection popup: {ex.Message}");
            await CustomAlert.ShowErrorAsync("No se pudo abrir el formulario de tipos de cobro", "Error de Interfaz");
        }
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (button != null)
        {
            button.IsEnabled = false;
        }

        try
        {
            if (BindingContext is CourtService vm)
            {
                double totalAmount = vm.GetTotalAmount();
                double totalTypeOfCollection = vm.GetTotalTypeOfCollection();
                double totalExpenditures = vm.GetTotalExpenditure();

                if (UserRole == "Admin")
                {
                    if (vm.SelectedBusiness is null)
                    {
                        await CustomAlert.ShowErrorAsync("Debe seleccionar un negocio para continuar con el cierre", "Negocio Requerido");
                        return;
                    }
                    if (vm.SelectedEds is null)
                    {
                        await CustomAlert.ShowErrorAsync("Debe seleccionar una estación de servicio (EDS) para continuar", "EDS Requerida");
                        return;
                    }
                    if (vm.SelectedIslander is null)
                    {
                        await CustomAlert.ShowErrorAsync("Debe seleccionar un islero responsable para continuar", "Islero Requerido");
                        return;
                    }
                }

                if (vm.CourtDispensers == null || !vm.CourtDispensers.Any())
                {
                    await CustomAlert.ShowErrorAsync("Debe agregar al menos un dispensador al cierre de turno", "Dispensadores Requeridos");
                    return;
                }

                if (vm.CourtTypeOfCollections == null || !vm.CourtTypeOfCollections.Any())
                {
                    await CustomAlert.ShowErrorAsync("No puede cerrar turno sin registrar al menos un método de pago. Por favor agregue uno antes de continuar.", "Medios de pago requeridos");
                    return;
                }

                double cash = totalTypeOfCollection - totalExpenditures;
                const double epsilon = 1e-6;
                if (cash < epsilon)
                {
                    await CustomAlert.ShowErrorAsync($"El total de efectivo no puede ser negativo.\n\nTotal recaudo: ${totalTypeOfCollection:F2}\nTotal gastos: ${totalExpenditures:F2}", "Error en Cálculos");
                    return;
                }

                try
                {
                    var loadingOverlay = this.FindByName<LoadingView.LoadingView>("LoadingOverlay");
                    try { loadingOverlay?.ShowLoading(); } catch { }
                    await vm.SendCourtDataAsync();
                }
                finally
                {
                    var loadingOverlay = this.FindByName<LoadingView.LoadingView>("LoadingOverlay");
                    try { loadingOverlay?.HideLoading(); } catch { }
                }

                if (vm.LastSendWasSuccessful)
                {
                    // 🔴 Oculta secciones y bloquea edición tras envío
                    OcultarSeccionesCierre();

                    // (si necesitas refrescar datos generales)
                    CourtService.ResetInstanceFields();
                    _service = CourtService.Instance;
                    BindingContext = _service;

                    await _service.GetAllEdsData();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in Button_Clicked: {ex.Message}");
            await CustomAlert.ShowErrorAsync($"Ocurrió un error al enviar los datos del cierre:\n\n{ex.Message}", "Error del Sistema");
        }
        finally
        {
            // Re-habilitar el botón después de completar la operación
            if (button != null)
            {
                button.IsEnabled = true;
            }
        }
    }

    // === NUEVO: método que oculta secciones y bloquea edición ===
    private void OcultarSeccionesCierre()
    {
        // Bloquear edición
        PuedeEditar = false;

        // Ocultar usando binding (si tu XAML usa x:Reference CortePage)
        SeccionesVisibles = false;

        // Respaldo: ocultar por nombre si existen estos contenedores
        this.FindByName<VisualElement>("SectionHoses")?.SetValue(VisualElement.IsVisibleProperty, false);     // Ventas por mangueras
        this.FindByName<VisualElement>("SectionPayments")?.SetValue(VisualElement.IsVisibleProperty, false);  // Formas de pago
        this.FindByName<VisualElement>("SectionExpenses")?.SetValue(VisualElement.IsVisibleProperty, false);  // Gastos
    }
    // ============================================================

    private void OnBusinessSelected(object sender, EventArgs e)
    {
        var picker = sender as Picker;
        if (picker == null) return;

        Debug.WriteLine($"Tipo de SelectedItem: {picker.SelectedItem?.GetType()}");

        Dispatcher.Dispatch(() =>
        {
            try
            {
                if (picker.SelectedItem is BusinessModel selectedBusiness)
                {
                    int businessId = selectedBusiness.IdBusiness;
                    Debug.WriteLine($"Negocio seleccionado: {selectedBusiness.Name}, ID: {businessId}");
                    _service.LoadEdsByBusiness(businessId);
                    _service.IslanderSelectList.Clear();
                    _service.IsBusinessSelected = true;
                }
                else
                {
                    Debug.WriteLine("El SelectedItem no es del tipo esperado o es null");
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
        var picker = sender as Picker;
        if (picker == null) return;

        Debug.WriteLine($"Tipo de SelectedItem: {picker.SelectedItem?.GetType()}");

        Dispatcher.Dispatch(() =>
        {
            try
            {
                if (picker.SelectedItem is EdsCourtModel selectedEds)
                {
                    int edsId = selectedEds.IdEds;
                    Debug.WriteLine($"Eds seleccionado: {selectedEds.Name}, ID: {edsId}");
                    _service.LoadIslandersByEds(edsId);
                    _service.IsEdsSelected = true;
                }
                else
                {
                    Debug.WriteLine("El SelectedItem no es del tipo esperado o es null");
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
        var picker = sender as Picker;
        if (picker == null) return;

        Debug.WriteLine($"Tipo de SelectedItem: {picker.SelectedItem?.GetType()}");

        Dispatcher.Dispatch(() =>
        {
            try
            {
                if (picker.SelectedItem is IslanderResponse selectedIslander)
                {
                    int islanderId = selectedIslander.IdIslander;
                    Debug.WriteLine($"Islander seleccionado: {selectedIslander.Name}, ID: {islanderId}");
                    _service.LoadEdsByBusiness(islanderId);
                }
                else
                {
                    Debug.WriteLine("El SelectedItem no es del tipo esperado o es null");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnIslanderSelected: {ex.Message}");
            }
        });
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private static async Task<T> ShowPopupSafelyAsync<T>(Popup popup) where T : class
    {
        try
        {
            var result = await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    return await Application.Current.MainPage.ShowPopupAsync(popup) as T;
                }
                catch (ObjectDisposedException ex)
                {
                    Debug.WriteLine($"Popup was disposed during show: {ex.Message}");
                    return null;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error showing popup: {ex.Message}");
                    return null;
                }
            });

            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in ShowPopupSafelyAsync: {ex.Message}");
            return null;
        }
    }
}
