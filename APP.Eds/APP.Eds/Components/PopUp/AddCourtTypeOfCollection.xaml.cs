using APP.Eds.Models.Court;
using APP.Eds.Services.Court;
using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace APP.Eds.Components.PopUp;

public partial class AddCourtTypeOfCollection : Popup, INotifyPropertyChanged
{
    private readonly CourtService courtService;
    private System.Timers.Timer _updateTimer;

    // ✅ NUEVO: Propiedades para controlar el estado de carga
    private bool _isLoading = true; // Empieza en true para mostrar loading al abrir
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (_isLoading != value)
            {
                _isLoading = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsContentVisible)); // Notifica cambio en visibilidad del contenido
            }
        }
    }

    private string _loadingMessage = "Cargando métodos de pago...";
    public string LoadingMessage
    {
        get => _loadingMessage;
        set
        {
            if (_loadingMessage != value)
            {
                _loadingMessage = value;
                OnPropertyChanged();
            }
        }
    }

    // Propiedad calculada para mostrar/ocultar el contenido
    public bool IsContentVisible => !IsLoading;

    // ✅ MODIFICADO: Mostrar el monto RESTANTE (que disminuye al agregar métodos)
    public double TotalDelDia => (double)Remaining;

    decimal _remaining;
    public decimal Remaining
    {
        get => _remaining;
        set 
        { 
            if (_remaining != value) 
            { 
                _remaining = value; 
                OnPropertyChanged();
                // ✅ IMPORTANTE: Notificar TotalDelDia cuando Remaining cambia
                OnPropertyChanged(nameof(TotalDelDia));
            } 
        }
    }

    public class PaymentOption : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public required TypeOfCollectionCourtModel Type { get; init; }

        // NUEVO: indica si este método fue pagado previamente (solo lectura en el popup)
        public bool IsPreviouslyPaid { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                PropertyChanged?.Invoke(this, new(nameof(IsSelected)));
            }
        }

        decimal _amount;
        public decimal Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                PropertyChanged?.Invoke(this, new(nameof(Amount)));
            }
        }

        string _notes = string.Empty;
        public string Notes
        {
            get => _notes;
            set
            {
                _notes = value;
                PropertyChanged?.Invoke(this, new(nameof(Notes)));
            }
        }
    }

    public ObservableCollection<PaymentOption> PaymentOptions { get; } = new();

    // ✅ NUEVO: Implementación explícita de INotifyPropertyChanged
    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public AddCourtTypeOfCollection(CourtService courtService)
    {
        InitializeComponent();
        this.courtService = courtService;

        // ✅ IMPORTANTE: Establecer BindingContext a esta instancia para que las propiedades IsLoading funcionen
        BindingContext = this;

        // ✅ NUEVO: Inicializar Remaining con el total pendiente
        Remaining = (decimal)courtService.RemainingToPay;

        _updateTimer = new System.Timers.Timer(1500);
        _updateTimer.Elapsed += OnUpdateTimerElapsed;
        _updateTimer.AutoReset = false;

        // ✅ NUEVO: Iniciar carga de forma asíncrona sin await (fire and forget es OK aquí)
        _ = InitializePaymentOptionsAsync();
    }

    private async Task InitializePaymentOptionsAsync()
    {
        try
        {
            // ✅ NUEVO: Mostrar loading al inicio
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                IsLoading = true;
                LoadingMessage = "Cargando métodos de pago...";
            });

            System.Diagnostics.Debug.WriteLine("AddCourtTypeOfCollection: Starting InitializePaymentOptionsAsync");
            
            // ✅ FIX: Try dedicated method first, then fallback to full data load
            try
            {
                System.Diagnostics.Debug.WriteLine("AddCourtTypeOfCollection: Attempting to load TypeOfCollection data directly");
                
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    LoadingMessage = "Cargando...";
                });
                
                await courtService.LoadTypeOfCollectionDataAsync();
                System.Diagnostics.Debug.WriteLine($"AddCourtTypeOfCollection: Direct load successful. Count = {courtService.TypeOfCollectionList?.Count ?? 0}");
            }
            catch (Exception directLoadEx)
            {
                System.Diagnostics.Debug.WriteLine($"AddCourtTypeOfCollection: Direct load failed: {directLoadEx.Message}. Trying full data load...");
                
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    LoadingMessage = "Reintentando carga de datos...";
                });
                
                // Fallback to full data load
                await courtService.GetAllEdsData();
                await Task.Delay(500); // Reduced delay since we're showing loading
                
                System.Diagnostics.Debug.WriteLine($"AddCourtTypeOfCollection: After GetAllEdsData, TypeOfCollectionList count = {courtService.TypeOfCollectionList?.Count ?? 0}");
            }

            // ✅ FIX: Better error message with specific guidance
            if (courtService.TypeOfCollectionList == null || !courtService.TypeOfCollectionList.Any())
            {
                System.Diagnostics.Debug.WriteLine("AddCourtTypeOfCollection: TypeOfCollectionList is still empty after loading attempts");
                
                // ✅ NUEVO: Ocultar loading antes de mostrar error
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    IsLoading = false;
                });
                
                await CustomAlert.ShowWarningAsync(
                    "⚠️ No se pudieron cargar los métodos de pago disponibles.\n\n" +
                    "Causas posibles:\n" +
                    "• Problemas de conexión con el servidor\n" +
                    "• El token de autenticación ha expirado\n" +
                    "• No hay métodos de pago configurados en el sistema\n\n" +
                    "Soluciones:\n" +
                    "1. Verifique su conexión a Internet\n" +
                    "2. Intente cerrar sesión y volver a iniciar\n" +
                    "3. Contacte al administrador del sistema\n\n" +
                    $"API URL: {Services.Config.Configuration.BaseUrl}/api/v1/type-of-collection",
                    "Datos No Disponibles");
                    
                Close(); // ✅ FIX: Close the popup if data couldn't be loaded
                return;
            }

            System.Diagnostics.Debug.WriteLine($"AddCourtTypeOfCollection: Successfully loaded {courtService.TypeOfCollectionList.Count} payment methods");

            // ✅ NUEVO: Actualizar mensaje de loading antes de procesar datos
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                LoadingMessage = "Preparando métodos de pago...";
            });

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                PaymentOptions.Clear();
                
                foreach (var t in courtService.TypeOfCollectionList)
                {
                    try
                    {
                        var opt = new PaymentOption 
                        { 
                            Type = t, 
                            IsSelected = false, 
                            Amount = 0m, 
                            IsPreviouslyPaid = false 
                        };
                        opt.PropertyChanged += PaymentOption_PropertyChanged;
                        PaymentOptions.Add(opt);
                        
                        System.Diagnostics.Debug.WriteLine($"AddCourtTypeOfCollection: Added payment option '{t.Description}'");
                    }
                    catch (Exception optEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"AddCourtTypeOfCollection: Error adding payment option '{t?.Description ?? "null"}': {optEx.Message}");
                    }
                }

                System.Diagnostics.Debug.WriteLine($"AddCourtTypeOfCollection: Total PaymentOptions added: {PaymentOptions.Count}");

                // Mostrar pagos previos marcados y bloqueados
                RestorePreviousSelections();

                RecalcRemaining();
                
                // ✅ NUEVO: Ocultar loading cuando todo esté listo
                IsLoading = false;
                System.Diagnostics.Debug.WriteLine("AddCourtTypeOfCollection: Loading complete, UI ready");
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AddCourtTypeOfCollection: Critical error in InitializePaymentOptionsAsync: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"AddCourtTypeOfCollection: Stack trace: {ex.StackTrace}");
            
            // ✅ NUEVO: Ocultar loading antes de mostrar error
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                IsLoading = false;
            });
            
            await CustomAlert.ShowErrorAsync(
                $"❌ Error crítico al inicializar los métodos de pago:\n\n{ex.Message}\n\n" +
                "Por favor, cierre esta ventana e intente nuevamente. Si el problema persiste, " +
                "reinicie la aplicación o contacte al soporte técnico.",
                "Error de Inicialización");
                
            Close(); // ✅ FIX: Close popup on critical error
        }
    }

    // Marca pagos ya registrados como seleccionados, con monto y en solo lectura
    private void RestorePreviousSelections()
    {
        try
        {
            var previous = courtService.CourtTypeOfCollections?.ToList();
            if (previous == null || previous.Count == 0)
                return;

            foreach (var prev in previous)
            {
                var option = PaymentOptions.FirstOrDefault(p =>
                    string.Equals(p.Type.Description, prev.TypeOfCollectionName, StringComparison.OrdinalIgnoreCase));

                if (option != null)
                {
                    option.IsSelected = true;
                    option.IsPreviouslyPaid = true;              // Bloquear edición
                    option.Amount = (decimal)prev.Amount;        // Mostrar el monto pagado
                    option.Notes = prev.Description ?? string.Empty;
                }
            }

            RecalcRemaining();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error restoring previous selections: {ex.Message}");
        }
    }

    private void PaymentOption_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PaymentOption.IsSelected))
        {
            RecalcRemaining();
        }
        else if (e.PropertyName == nameof(PaymentOption.Amount))
        {
            // Antes: StartUpdateTimer();
            RecalcRemaining(); // recalcula al instante
        }
    }

    private void StartUpdateTimer()
    {
        try
        {
            _updateTimer?.Stop();
            _updateTimer?.Start();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error starting update timer: {ex.Message}");
        }
    }

    private void OnUpdateTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        try
        {
            Application.Current?.Dispatcher.Dispatch(() => RecalcRemaining());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in update timer elapsed: {ex.Message}");
        }
    }

    private void RecalcRemaining()
    {
        try
        {
            // Solo lo que el usuario está agregando AHORA (excluye pagos ya registrados)
            var addedThisSession = PaymentOptions
                .Where(p => p.IsSelected && !p.IsPreviouslyPaid)
                .Sum(p => p.Amount);

            var baseTotal = (decimal)courtService.RemainingToPay; // pendiente actual
            Remaining = Math.Max(0, baseTotal - addedThisSession);
            
            // ✅ Ya no es necesario notificar TotalDelDia aquí porque se notifica automáticamente al cambiar Remaining
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error calculating remaining: {ex.Message}");
        }
    }

    private void OnCloseTapped(object sender, EventArgs e)
    {
        try
        {
            _updateTimer?.Stop();
            _updateTimer?.Dispose();
            Close();
        }
        catch { }
    }

    private void Amount_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            if (sender is Entry entry)
            {
                var newText = e.NewTextValue ?? string.Empty;
                var oldText = e.OldTextValue ?? string.Empty;

                if (oldText == "0.00" && !string.IsNullOrEmpty(newText) && newText != "0" && newText != "0.0")
                {
                    entry.Text = newText.Replace("0.00", "");
                    return;
                }

                if (!string.IsNullOrWhiteSpace(newText) &&
                    !decimal.TryParse(newText, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
                {
                    entry.Text = oldText;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in Amount_TextChanged: {ex.Message}");
        }
        finally
        {
            RecalcRemaining(); // recalcula al escribir
        }
    }

    private void Amount_Focused(object sender, FocusEventArgs e)
    {
        try
        {
            if (sender is Entry entry && entry.Text == "0.00")
            {
                entry.Text = string.Empty;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in Amount_Focused: {ex.Message}");
        }
    }

    private void Amount_Unfocused(object sender, FocusEventArgs e)
    {
        try
        {
            if (sender is Entry entry)
            {
                if (string.IsNullOrWhiteSpace(entry.Text))
                {
                    entry.Text = "0.00";
                }
                else if (decimal.TryParse(entry.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal value))
                {
                    entry.Text = value.ToString("F2");
                }

                StartUpdateTimer();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in Amount_Unfocused: {ex.Message}");
        }
    }

    private void Clear_All(object sender, EventArgs e)
    {
        try
        {
            _updateTimer?.Stop();

            foreach (var p in PaymentOptions)
            {
                if (!p.IsPreviouslyPaid) // No limpiar pagos ya registrados
                {
                    p.IsSelected = false;
                    p.Amount = 0m;
                    p.Notes = string.Empty;
                }
            }

            RecalcRemaining();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error clearing payment options: {ex.Message}");
        }
    }

    private async void Add_Selected(object sender, EventArgs e)
    {
        try
        {
            if (sender is Button button)
            {
                button.IsEnabled = false;
                button.Text = "Agregando...";
            }

            // Solo los nuevos de esta sesión
            var selected = PaymentOptions
                .Where(p => p.IsSelected && !p.IsPreviouslyPaid)
                .ToList();

            if (selected.Count == 0)
            {
                if (courtService.RemainingToPay <= 0)
                {
                    await CustomAlert.ShowInfoAsync("No hay saldo pendiente por pagar.", "Sin Pendiente");
                }
                else
                {
                    await CustomAlert.ShowErrorAsync("Seleccione al menos un método de pago nuevo.", "Selección Requerida");
                }
                return;
            }

            foreach (var p in selected)
            {
                if (p.Amount <= 0m)
                {
                    await CustomAlert.ShowErrorAsync($"El monto para '{p.Type.Description}' debe ser mayor a 0.", "Monto Inválido");
                    return;
                }
            }

            // Validar contra el pendiente actual
            decimal pendingBefore = (decimal)courtService.RemainingToPay;
            decimal dataNew = selected.Sum(p => p.Amount);
            decimal diff = pendingBefore - dataNew;

            if (Math.Abs(diff) > 0.009m) // tolerancia centavos
            {
                await CustomAlert.ShowErrorAsync(
                    $"El total que intenta registrar no coincide con el pendiente.\n\n" +
                    $"• Pendiente actual: {pendingBefore:C2}\n" +
                    $"• A registrar ahora: {dataNew:C2}\n" +
                    $"• Diferencia: {diff:C2}",
                    "Total No Coincide");
                return;
            }

            // Registrar los nuevos métodos
            foreach (var p in selected)
            {
                courtService.SelectedTypeOfCollection = p.Type;
                courtService.CourtTypeOfCollectionAmount = (double)p.Amount;
                courtService.CourtTypeOfCollectionDescription = p.Notes;

                await courtService.AddCourtTypeOfCollectionFromPopup();
            }

            await CustomAlert.ShowSuccessAsync(
                $"Se agregaron {selected.Count} método(s) por {dataNew:C2}.",
                "Métodos de Pago Agregados");

            // IMPORTANTE: NO resetear TotalSales; el pendiente se actualiza dentro del servicio
            // courtService.TotalSales = 0;  // ← eliminar esta línea si existía

            await CloseAsync();
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error al agregar métodos de pago:\n\n{ex.Message}", "Error del Sistema");
        }
        finally
        {
            if (sender is Button button)
            {
                button.IsEnabled = true;
                button.Text = courtService.Add ?? "Agregar";
            }
        }
    }

    private void PreselectDefaultOption()
    {
        try
        {
            var pending = (decimal)courtService.RemainingToPay;

            PaymentOption target = null;
            var last = courtService.CourtTypeOfCollections?.LastOrDefault();
            if (last != null)
            {
                target = PaymentOptions.FirstOrDefault(p =>
                    string.Equals(p.Type.Description, last.TypeOfCollectionName, StringComparison.OrdinalIgnoreCase));
            }

            if (target == null)
                target = PaymentOptions.FirstOrDefault(p =>
                    p.Type.Description?.Contains("Efectivo", StringComparison.OrdinalIgnoreCase) == true);

            target ??= PaymentOptions.FirstOrDefault();

            if (target != null && !target.IsPreviouslyPaid)
            {
                target.IsSelected = true;
                target.Amount = pending;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error preselecting default payment option: {ex.Message}");
        }
    }

    private async Task CloseAsync()
    {
        try
        {
            _updateTimer?.Stop();
            _updateTimer?.Dispose();
            await Task.Delay(100);
            Close();
        }
        catch { }
    }
}
