using APP.Eds.Models.Court;
using APP.Eds.Services.Court;
using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;

namespace APP.Eds.Components.PopUp;

public partial class AddCourtTypeOfCollection : Popup
{
    private readonly CourtService courtService;
    private System.Timers.Timer _updateTimer;

    decimal _remaining;
    public decimal Remaining
    {
        get => _remaining;
        set { if (_remaining != value) { _remaining = value; OnPropertyChanged(); } }
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

    public AddCourtTypeOfCollection(CourtService courtService)
    {
        InitializeComponent();
        this.courtService = courtService;

        // Encadenar al servicio para los bindings (RemainingToPay en el encabezado)
        BindingContext = courtService;

        _updateTimer = new System.Timers.Timer(1500);
        _updateTimer.Elapsed += OnUpdateTimerElapsed;
        _updateTimer.AutoReset = false;

        InitializePaymentOptionsAsync();
    }

    private async void InitializePaymentOptionsAsync()
    {
        try
        {
            if (courtService.TypeOfCollectionList == null || !courtService.TypeOfCollectionList.Any())
            {
                await courtService.GetAllEdsData();
                await Task.Delay(500);
            }

            if (courtService.TypeOfCollectionList == null || !courtService.TypeOfCollectionList.Any())
            {
                await CustomAlert.ShowWarningAsync(
                    "No se pudieron cargar los métodos de pago disponibles.\n\n" +
                    "Posibles causas: conexión, servidor o autenticación.",
                    "Datos No Disponibles");
                return;
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                PaymentOptions.Clear();
                foreach (var t in courtService.TypeOfCollectionList)
                {
                    var opt = new PaymentOption { Type = t, IsSelected = false, Amount = 0m, IsPreviouslyPaid = false };
                    opt.PropertyChanged += PaymentOption_PropertyChanged;
                    PaymentOptions.Add(opt);
                }

                // Mostrar pagos previos marcados y bloqueados
                RestorePreviousSelections();

                RecalcRemaining();
            });
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync(
                $"Error al inicializar los métodos de pago:\n\n{ex.Message}",
                "Error de Inicialización");
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

    private void RecalcRemaining()
    {
        try
        {
            // Solo lo que el usuario está agregando AHORA (excluye pagos ya registrados)
            var addedThisSession = PaymentOptions
                .Where(p => p.IsSelected && !p.IsPreviouslyPaid)
                .Sum(p => p.Amount);

            var baseTotal = (decimal)courtService.RemainingToPay; // pendiente actual del servicio
            Remaining = Math.Max(0, baseTotal - addedThisSession);
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
