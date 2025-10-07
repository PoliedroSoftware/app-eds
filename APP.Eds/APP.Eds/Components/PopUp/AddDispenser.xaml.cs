using APP.Eds.Services.Court;
using APP.Eds.Components.PopUp;
using CommunityToolkit.Maui.Views;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Globalization;

namespace APP.Eds.Components.PopUp;

public partial class AddDispenser : Popup, INotifyPropertyChanged
{
    private readonly CourtService courtService;
    private bool isGallonsEditable = false;
    private bool canEditPrice = false;

    // >>> VALIDATION: record last valid price to rollback on invalid edits
    private double _lastValidSellPrice = 0d;

    private bool _isDisposed = false;
    private readonly object _disposeLock = new object();

    public CourtService CourtService => courtService;

    // Propiedades que se sincronizan con CourtService
    public double AccumulatedAmount
    {
        get => courtService.AccumulatedAmount;
        set
        {
            courtService.AccumulatedAmount = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(AmountDifferenceResult));
            OnPropertyChanged(nameof(FutureStock));
        }
    }

    public double AccumulatedGallons
    {
        get => courtService.AccumulatedGallons;
        set
        {
            courtService.AccumulatedGallons = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(GallonsDifferenceResult));
            OnPropertyChanged(nameof(FutureStock));
        }
    }

    public double LastAccumulatedAmount
    {
        get => courtService.LastAccumulatedAmount;
        set
        {
            courtService.LastAccumulatedAmount = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(AmountDifferenceResult));
            OnPropertyChanged(nameof(FutureStock));
        }
    }

    public double LastAccumulatedGallons
    {
        get => courtService.LastAccumulatedGallons;
        set
        {
            courtService.LastAccumulatedGallons = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(GallonsDifferenceResult));
            OnPropertyChanged(nameof(FutureStock));
        }
    }

    public Models.Court.HoseCourtModel SelectedHose
    {
        get => courtService.SelectedHose;
        set
        {
            courtService.SelectedHose = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FutureStock));
        }
    }

    public double AmountDifferenceResult => courtService.AmountDifferenceResult;
    public double GallonsDifferenceResult => courtService.GallonsDifferenceResult;

    // Propiedad para calcular el stock futuro
    public decimal FutureStock
    {
        get
        {
            if (SelectedHose?.ProductEntity?.Stock == null)
                return 0;

            var currentStock = SelectedHose.ProductEntity.Stock;
            var gallonsSold = (decimal)GallonsDifferenceResult;
            
            return Math.Max(0, currentStock - gallonsSold);
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public AddDispenser(CourtService courtService)
    {
        InitializeComponent();
        this.courtService = courtService;

        if (SecondEntry != null)
        {
            SecondEntry.IsEnabled = isGallonsEditable;
        }

        CheckUserRole();

        // Configure local binding context
        BindingContext = this;
    }

    private void CheckUserRole()
    {
        ExecuteSafely(() =>
        {
            var userRole = Preferences.Get("userRole", "User");
            canEditPrice = userRole == "Admin" || userRole == "User";

            Dispatcher.Dispatch(() => UpdatePriceEditVisibility());
        });
    }

    private void EditGallonsButton_Clicked(object sender, EventArgs e)
    {
        ExecuteSafely(() =>
        {
            if (SecondEntry == null) return;

            isGallonsEditable = !isGallonsEditable;
            SecondEntry.IsEnabled = isGallonsEditable;
            if (isGallonsEditable)
            {
                SecondEntry.Focus();
                SecondEntry.CursorPosition = SecondEntry.Text?.Length ?? 0;
            }
        });
    }

    private void OnCloseTapped(object sender, EventArgs e)
    {
        ExecuteSafely(() =>
        {
            ResetViewModel();
            ClosePopupSafely();
        });
    }

    private async void Add_Dispenser(object sender, EventArgs e)
    {
        if (!IsAvailable()) return;

        await ExecuteSafelyAsync(async () =>
        {
            if (SelectedHose is not null)
            {
                var selectedHoseId = SelectedHose.IdHose;

                if (AccumulatedAmount == 0)
                {
                    await CustomAlert.ShowErrorAsync("Debe ingresar el monto total de la venta para continuar", "Monto Requerido");
                    return;
                }

                if (AccumulatedGallons == 0)
                {
                    await CustomAlert.ShowErrorAsync("Debe ingresar la cantidad de galones vendidos", "Galones Requeridos");
                    return;
                }

                if (AccumulatedAmount < LastAccumulatedAmount)
                {
                    await CustomAlert.ShowErrorAsync($"El monto acumulado (${AccumulatedAmount:F2}) debe ser mayor que el último monto registrado (${LastAccumulatedAmount:F2})", "Monto Inválido");
                    return;
                }

                if (AccumulatedGallons < LastAccumulatedGallons)
                {
                    await CustomAlert.ShowErrorAsync($"Los galones acumulados ({AccumulatedGallons:F2}) deben ser mayores que los últimos galones registrados ({LastAccumulatedGallons:F2})", "Galones Inválidos");
                    return;
                }

                // >>> VALIDATION: price per gallon must be > 0 before adding
                var effectivePrice = SelectedHose?.EffectiveSellPrice ?? 0d;
                if (effectivePrice <= 0d)
                {
                    await CustomAlert.ShowErrorAsync("El precio por galón debe ser mayor a 0. Edite el precio antes de continuar.", "Precio inválido");
                    // intenta enfocar el Entry si está visible
                    PriceEditEntry?.Focus();
                    return;
                }

                // Synchronize data with CourtService before calling the method
                courtService.IdHose = SelectedHose.IdHose;

                // Call the service method to add the dispenser
                await courtService.AddDispenserFromPopup();

                // Add the selected hose to the selected hoses list in the service
                courtService.AddSelectedHose(SelectedHose);

                ResetViewModel();
                ClosePopupSafely();
            }
            else
            {
                await CustomAlert.ShowErrorAsync("Debe seleccionar una manguera del dispensador para continuar", "Manguera Requerida");
            }
        });
    }

    private void EntryAccumulatedCompleted(object sender, EventArgs e)
    {
        ExecuteSafely(() => UpdateAccumulatedValues());
    }

    private void OnEntryUnfocused(object sender, FocusEventArgs e)
    {
        ExecuteSafely(() => UpdateAccumulatedValues());
    }

    private void UpdateAccumulatedValues()
    {
        ExecuteSafely(() =>
        {
            if (SelectedHose is not null)
            {
                if (AccumulatedAmount > LastAccumulatedAmount)
                {
                    double amountDifference = AccumulatedAmount - LastAccumulatedAmount;
                    double currentSellPrice = SelectedHose.EffectiveSellPrice;

                    // Fixed: Add division to zero check
                    if (currentSellPrice > 0)
                    {
                        AccumulatedGallons = LastAccumulatedGallons + (amountDifference / currentSellPrice);
                    }
                    else
                    {
                        Debug.WriteLine("Warning: EffectiveSellPrice is zero or negative, cannot calculate gallons");
                    }
                }
                UpdateAccumulatedColors();
                OnPropertyChanged(nameof(FutureStock));
            }
        });
    }

    private void UpdateAccumulatedColors()
    {
        ExecuteSafely(() =>
        {
            if (FirstEntry?.Parent is Frame amountFrame)
            {
                amountFrame.BorderColor = AccumulatedAmount >= LastAccumulatedAmount ? Colors.Green : Colors.Red;
            }

            if (SecondEntry?.Parent is Frame gallonFrame)
            {
                gallonFrame.BorderColor = AccumulatedGallons >= LastAccumulatedGallons ? Colors.Green : Colors.Red;
            }
        });
    }

    private void EntryGallonsCompleted(object sender, EventArgs e)
    {
        ExecuteSafely(() =>
        {
            UpdateAccumulatedColors();
            AddButton?.Focus();
        });
    }

    private void HoseSelected(object sender, EventArgs e)
    {
        ExecuteSafely(() =>
        {
            if (HosePicker?.SelectedIndex != -1 && FirstEntry != null)
            {
                FirstEntry.IsEnabled = true;
                FirstEntry.Focus();
                FirstEntry.CursorPosition = FirstEntry.Text?.Length ?? 0;

                if (SelectedHose is not null)
                {
                    // Cargar históricos
                    LastAccumulatedAmount = SelectedHose.AccumulatedAmount;
                    LastAccumulatedGallons = SelectedHose.AccumulatedGallons;

                    double effectiveSellPrice = SelectedHose.EffectiveSellPrice;
                    _lastValidSellPrice = effectiveSellPrice; // >>> VALIDATION: guarda valor válido para rollback

                    if (PricePerGallonLabel != null)
                    {
                        PricePerGallonLabel.Text = $"{effectiveSellPrice:C3}";
                    }

                    if (canEditPrice && PriceEditEntry != null)
                    {
                        PriceEditEntry.Text = effectiveSellPrice.ToString("F2", CultureInfo.InvariantCulture);
                    }
                }
                else
                {
                    LastAccumulatedAmount = 0;
                    LastAccumulatedGallons = 0;

                    if (PricePerGallonLabel != null)
                        PricePerGallonLabel.Text = "##.###";

                    if (canEditPrice && PriceEditEntry != null)
                        PriceEditEntry.Text = "";
                }

                UpdatePriceEditVisibility();
                UpdateAccumulatedColors();
            }
            else
            {
                LastAccumulatedAmount = 0;
                LastAccumulatedGallons = 0;

                if (PricePerGallonLabel != null)
                    PricePerGallonLabel.Text = "##.###";

                if (canEditPrice && PriceEditEntry != null)
                    PriceEditEntry.Text = "";
            }
        });
    }

    private void UpdatePriceEditVisibility()
    {
        ExecuteSafely(() =>
        {
            if (PriceEditEntry != null && PriceEditButton != null)
            {
                PriceEditEntry.IsVisible = canEditPrice;
                PriceEditButton.IsVisible = canEditPrice;

                if (PricePerGallonLabel != null)
                {
                    PricePerGallonLabel.IsVisible = !canEditPrice;
                }
            }
        });
    }

    private void PriceEditEntry_Completed(object sender, EventArgs e)
    {
        ExecuteSafely(() => UpdateSelectedHoseSellPrice());
    }

    private void PriceEditEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        ExecuteSafely(() =>
        {
            if (SelectedHose is null || PriceEditEntry is null) return;

            var newSellPrice = ParseDoubleInvariant(e.NewTextValue);

            if (newSellPrice > 0)
            {
                // Actualiza precio (hose + product)
                SelectedHose.SellPrice = newSellPrice;
                if (SelectedHose.ProductEntity != null)
                    SelectedHose.ProductEntity.SellPrice = newSellPrice;

                _lastValidSellPrice = newSellPrice;

                if (PricePerGallonLabel != null)
                    PricePerGallonLabel.Text = $"{newSellPrice:C2}";

                // Recalcula si corresponde
                if (AccumulatedAmount > LastAccumulatedAmount)
                {
                    double amountDifference = AccumulatedAmount - LastAccumulatedAmount;
                    AccumulatedGallons = LastAccumulatedGallons + (amountDifference / newSellPrice);
                    UpdateAccumulatedColors();
                }
                
                OnPropertyChanged(nameof(FutureStock));
            }
            else
            {
                // Valor inválido: no recalcular; UI XAML ya deshabilita "Agregar"
                Debug.WriteLine("PriceEditEntry_TextChanged: valor no válido (<= 0).");
            }
        });
    }

    private void PriceEditButton_Clicked(object sender, EventArgs e)
    {
        ExecuteSafely(() => PriceEditEntry?.Focus());
    }

    private void UpdateSelectedHoseSellPrice()
    {
        ExecuteSafely(() =>
        {
            if (SelectedHose is not null && PriceEditEntry != null)
            {
                var newSellPrice = ParseDoubleInvariant(PriceEditEntry.Text);

                if (newSellPrice > 0)
                {
                    SelectedHose.SellPrice = newSellPrice;
                    if (SelectedHose.ProductEntity != null)
                        SelectedHose.ProductEntity.SellPrice = newSellPrice;

                    _lastValidSellPrice = newSellPrice;

                    if (PricePerGallonLabel != null)
                        PricePerGallonLabel.Text = $"{newSellPrice:C2}";

                    if (AccumulatedAmount > LastAccumulatedAmount)
                    {
                        double amountDifference = AccumulatedAmount - LastAccumulatedAmount;
                        AccumulatedGallons = LastAccumulatedGallons + (amountDifference / newSellPrice);
                        UpdateAccumulatedColors();
                    }
                    
                    OnPropertyChanged(nameof(FutureStock));
                }
                else
                {
                    // >>> VALIDATION: revertir al último valor válido y avisar
                    PriceEditEntry.Text = _lastValidSellPrice.ToString("F2", CultureInfo.InvariantCulture);
                    _ = CustomAlert.ShowErrorAsync("El precio por galón debe ser mayor a 0.", "Precio inválido");
                }
            }
        });
    }

    // --- Helpers de ejecución segura ---
    private void ExecuteSafely(Action action)
    {
        lock (_disposeLock)
        {
            if (_isDisposed) return;

            try
            {
                action?.Invoke();
            }
            catch (ObjectDisposedException)
            {
                _isDisposed = true;
                Debug.WriteLine($"{GetType().Name}: Object was disposed during operation");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{GetType().Name}: Error in safe execution - {ex.Message}");
            }
        }
    }

    private async Task ExecuteSafelyAsync(Func<Task> asyncAction)
    {
        lock (_disposeLock)
        {
            if (_isDisposed) return;
        }

        try
        {
            await (asyncAction?.Invoke() ?? Task.CompletedTask);
        }
        catch (ObjectDisposedException)
        {
            lock (_disposeLock)
            {
                _isDisposed = true;
            }
            Debug.WriteLine($"{GetType().Name}: Object was disposed during async operation");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{GetType().Name}: Error in safe async execution - {ex.Message}");
        }
    }

    private bool IsAvailable()
    {
        lock (_disposeLock)
        {
            return !_isDisposed;
        }
    }

    private void ClosePopupSafely()
    {
        lock (_disposeLock)
        {
            if (_isDisposed) return;

            try
            {
                _isDisposed = true;
                Close();
            }
            catch (ObjectDisposedException)
            {
                Debug.WriteLine($"{GetType().Name}: Popup was already disposed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{GetType().Name}: Error closing popup - {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Resetea el ViewModel a valores por defecto
    /// </summary>
    private void ResetViewModel()
    {
        ExecuteSafely(() =>
        {
            try
            {
                AccumulatedAmount = 0;
                AccumulatedGallons = 0;
                LastAccumulatedAmount = 0;
                LastAccumulatedGallons = 0;
                courtService.SelectedHose = null;
                courtService.IdHose = 0;
                _lastValidSellPrice = 0d; // >>> reset value
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Warning: Could not reset ViewModel properties - {ex.Message}");
            }
        });
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // --- Helpers de validación/parsing ---
    private static double ParseDoubleInvariant(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0d;

        // Normaliza coma/punto
        var normalized = text.Replace(',', '.');

        if (double.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out var val))
            return val;

        return 0d;
    }
}
