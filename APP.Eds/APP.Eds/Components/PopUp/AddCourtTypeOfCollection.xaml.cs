using APP.Eds.Services.Court;
using CommunityToolkit.Maui.Views;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using APP.Eds.Models.Court;

namespace APP.Eds.Components.PopUp;

public partial class AddCourtTypeOfCollection : Popup
{
    private readonly CourtService courtService;

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
        
        // Set BindingContext for proper data binding
        BindingContext = courtService;

        InitializePaymentOptions();
    }

    private void InitializePaymentOptions()
    {
        try
        {
            if (courtService.TypeOfCollectionList is not null)
            {
                foreach (var t in courtService.TypeOfCollectionList)
                {
                    var opt = new PaymentOption { Type = t, IsSelected = false, Amount = 0m };
                    opt.PropertyChanged += PaymentOption_PropertyChanged;
                    PaymentOptions.Add(opt);
                }

                RecalcRemaining();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing payment options: {ex.Message}");
        }
    }

    private void PaymentOption_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PaymentOption.Amount) || e.PropertyName == nameof(PaymentOption.IsSelected))
        {
            RecalcRemaining();
        }
    }

    private void RecalcRemaining()
    {
        try
        {
            var added = PaymentOptions.Where(p => p.IsSelected).Sum(p => p.Amount);
            var baseTotal = (decimal)courtService.TotalSales;
            Remaining = baseTotal - added;
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
            Close();
        }
        catch (ObjectDisposedException ex)
        {
            System.Diagnostics.Debug.WriteLine($"AddCourtTypeOfCollection popup was already disposed during close: {ex.Message}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error closing AddCourtTypeOfCollection popup: {ex.Message}");
        }
    }

    private void Amount_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            if (sender is Entry entry)
            {
                var newText = e.NewTextValue ?? string.Empty;
                if (string.IsNullOrWhiteSpace(newText)) return;

                if (!decimal.TryParse(newText, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
                    entry.Text = e.OldTextValue;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in Amount_TextChanged: {ex.Message}");
        }
    }

    private void Clear_All(object sender, EventArgs e)
    {
        try
        {
            foreach (var p in PaymentOptions)
            {
                p.IsSelected = false;
                p.Amount = 0m;
                p.Notes = string.Empty;
            }
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
            // Disable button to prevent multiple submissions
            if (sender is Button button)
            {
                button.IsEnabled = false;
                button.Text = "Agregando...";
            }

            var selected = PaymentOptions.Where(p => p.IsSelected).ToList();
            
            if (selected.Count == 0)
            {
                await Application.Current.MainPage.DisplayAlert("Validación", 
                    "Debe seleccionar al menos un método de pago.", "OK");
                return;
            }

            // Validate amounts
            foreach (var p in selected)
            {
                if (p.Amount <= 0m)
                {
                    await Application.Current.MainPage.DisplayAlert("Validación",
                        $"El monto para '{p.Type.Description}' debe ser mayor a 0.", "OK");
                    return;
                }
            }

            // Check if total matches
            decimal totalSalesDay = (decimal)courtService.TotalSales;
            decimal addedNow = (decimal)(courtService.CourtTypeOfCollections?.Sum(p => p.Amount) ?? 0d);
            decimal dataNew = selected.Sum(p => p.Amount);
            decimal amountNew = totalSalesDay - addedNow - dataNew;

            if (Math.Abs(amountNew) > 0.01m) // Allow for small rounding differences
            {
                bool confirm = await Application.Current.MainPage.DisplayAlert("Confirmación",
                    $"El total del día no coincide exactamente (diferencia: ${amountNew:F2}).\n\n¿Desea continuar de todas formas?", 
                    "Continuar", "Revisar");
                
                if (!confirm) return;
            }

            // Add selected payment methods
            foreach (var p in selected)
            {
                courtService.SelectedTypeOfCollection = p.Type;
                courtService.CourtTypeOfCollectionAmount = (double)p.Amount;
                courtService.CourtTypeOfCollectionDescription = p.Notes;

                await courtService.AddCourtTypeOfCollectionFromPopup();
            }

            await Application.Current.MainPage.DisplayAlert("Éxito", 
                $"Se agregaron {selected.Count} método(s) de pago correctamente.", "OK");

            await CloseAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error adding selected payment methods: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Error", 
                $"Error al agregar métodos de pago:\n\n{ex.Message}", "OK");
        }
        finally
        {
            // Re-enable button
            if (sender is Button button)
            {
                button.IsEnabled = true;
                button.Text = courtService.Add ?? "Agregar";
            }
        }
    }

    private async Task CloseAsync()
    {
        try
        {
            await Task.Delay(100); // Small delay for smooth animation
            Close();
        }
        catch (ObjectDisposedException ex)
        {
            System.Diagnostics.Debug.WriteLine($"AddCourtTypeOfCollection popup was already disposed during close: {ex.Message}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error closing popup: {ex.Message}");
        }
    }
}
