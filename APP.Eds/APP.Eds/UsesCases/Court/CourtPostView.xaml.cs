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
    
    // Variables para rastrear popups activos
    private Popup _activePopup;
    private readonly SemaphoreSlim _popupSemaphore = new(1, 1);
    
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
        BindingContext = _service;
        
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
            await DisplayAlert("Error", $"Error al cargar datos: {ex.Message}", "OK");
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
        // Cerrar cualquier popup activo cuando la página desaparezca
        CloseActivePopupSafely();
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
    /// Maneja los taps en el menú de navegación inferior
    /// </summary>
    private async void OnBottomNavTapped(object sender, EventArgs e)
    {
        if (sender is TapGestureRecognizer tapGesture && tapGesture.CommandParameter is string navItem)
        {
            try
            {
                // Actualizar elemento activo (esto desencadena las animaciones)
                ActiveNavItem = navItem;
                
                // Animar el tap
                await AnimateNavItemTap(navItem);
                
                // Ejecutar la acción correspondiente
                await ExecuteNavAction(navItem);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in bottom nav tap: {ex.Message}");
            }
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
                    await OpenDocumentPopUp();
                    break;
                case "Expense":
                    await OpenExpenditurePopUp();
                    break;
                case "Info":
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
            await DisplayAlert("Error", "No se pudo ejecutar la acción", "OK");
        }
    }

    /// <summary>
    /// Método seguro para mostrar popups que evita múltiples instancias simultáneas
    /// </summary>
    private async Task<bool> ShowPopupSafelyAsync(Popup popup)
    {
        if (!await _popupSemaphore.WaitAsync(100)) // Timeout de 100ms
        {
            popup?.Close(); // Cerrar el popup si no se puede mostrar
            return false;
        }

        try
        {
            // Cerrar popup activo si existe
            CloseActivePopupSafely();

            // Configurar el nuevo popup
            _activePopup = popup;

            // Mostrar el popup de forma segura
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    await this.ShowPopupAsync(popup);
                }
                catch (ObjectDisposedException)
                {
                    Debug.WriteLine("Popup was already disposed");
                    _activePopup = null;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error showing popup: {ex.Message}");
                    _activePopup = null;
                }
            });

            return true;
        }
        finally
        {
            _popupSemaphore.Release();
        }
    }

    /// <summary>
    /// Cierra el popup activo de forma segura
    /// </summary>
    private void CloseActivePopupSafely()
    {
        if (_activePopup != null)
        {
            try
            {
                _activePopup.Close();
            }
            catch (ObjectDisposedException)
            {
                // El popup ya fue dispuesto, esto es normal
                Debug.WriteLine("Popup was already disposed when trying to close");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error closing popup: {ex.Message}");
            }
            finally
            {
                _activePopup = null;
            }
        }
    }

    private async Task OpenDocumentPopUp()
    {
        try
        {
            var popup = new AddDocuemt(_service);
            await ShowPopupSafelyAsync(popup);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error opening document popup: {ex.Message}");
            await DisplayAlert("Error", "No se pudo abrir el popup de documentos", "OK");
        }
    }

    private async Task OpenExpenditurePopUp()
    {
        try
        {
            var popup = new AddCourtExpenditure(_service);
            await ShowPopupSafelyAsync(popup);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error opening expenditure popup: {ex.Message}");
            await DisplayAlert("Error", "No se pudo abrir el popup de gastos", "OK");
        }
    }

    private async Task OpenAdditionalInfoPopUp()
    {
        try
        {
            var popup = new AddInfo(_service);
            await ShowPopupSafelyAsync(popup);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error opening info popup: {ex.Message}");
            await DisplayAlert("Error", "No se pudo abrir el popup de información", "OK");
        }
    }

    private async Task OnShowCourtListClicked()
    {
        try
        {
            // Cerrar cualquier popup antes de navegar
            CloseActivePopupSafely();
            await Navigation.PushAsync(new CourtListView());
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error navigating to court list: {ex.Message}");
            await DisplayAlert("Error", "No se pudo abrir la lista de cierres", "OK");
        }
    }

    // Métodos existentes adaptados
    private async void OpenDispenserPopUp(object sender, EventArgs e)
    {
        try
        {
            var popup = new AddDispenser(_service);
            await ShowPopupSafelyAsync(popup);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error opening dispenser popup: {ex.Message}");
            await DisplayAlert("Error", "No se pudo abrir el popup del dispensador", "OK");
        }
    }

    private async void OpenTypeOfCollectionPopUp(object sender, EventArgs e)
    {
        try
        {
            var popup = new AddCourtTypeOfCollection(_service);
            await ShowPopupSafelyAsync(popup);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error opening collection popup: {ex.Message}");
            await DisplayAlert("Error", "No se pudo abrir el popup de tipos de cobro", "OK");
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
                        await DisplayAlert("Error", "Por favor, seleccione un Negocio", "OK");
                        return;
                    }
                    if (vm.SelectedEds is null)
                    {
                        await DisplayAlert("Error", "Por favor, seleccione un EDS", "OK");
                        return;
                    }
                    if (vm.SelectedIslander is null)
                    {
                        await DisplayAlert("Error", "Por favor, seleccione un Islero", "OK");
                        return;
                    }
                }

                if (vm.CourtDispensers == null || !vm.CourtDispensers.Any())
                {
                    await DisplayAlert("Error", "Debe agregar al menos un dispensador", "OK");
                    return;
                }

                if (vm.CourtTypeOfCollections == null || !vm.CourtTypeOfCollections.Any())
                {
                    await DisplayAlert("Error", "Debe agregar al menos un tipo recuado", "OK");
                    return;
                }

                double cash = totalTypeOfCollection - totalExpenditures;
                const double epsilon = 1e-6;
                if (cash < -epsilon)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", $"El total de efectivo no puede ser negativo", "OK");
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
            await DisplayAlert("Error", "Ocurrió un error al enviar los datos", "OK");
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
}