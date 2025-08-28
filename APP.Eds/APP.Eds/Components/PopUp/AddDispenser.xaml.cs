using APP.Eds.Services.Court;
using APP.Eds.Components.PopUp;
using CommunityToolkit.Maui.Views;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace APP.Eds.Components.PopUp;

public partial class AddDispenser : Popup, INotifyPropertyChanged
{
    private readonly CourtService courtService;
    private bool isGallonsEditable = false;
    private bool canEditPrice = false;
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
        } 
    }

    public Models.Court.HoseCourtModel SelectedHose
    {
        get => courtService.SelectedHose;
        set 
        { 
            courtService.SelectedHose = value;
            OnPropertyChanged();
        }
    }

    public double AmountDifferenceResult => courtService.AmountDifferenceResult;
    public double GallonsDifferenceResult => courtService.GallonsDifferenceResult;

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
                    double currentPrice = SelectedHose.Price;
                    
                    // Fixed: Add division by zero check
                    if (currentPrice > 0)
                    {
                        AccumulatedGallons = LastAccumulatedGallons + (amountDifference / currentPrice);
                    }
                    else
                    {
                        Debug.WriteLine("Warning: Price is zero or negative, cannot calculate gallons");
                    }
                }
                UpdateAccumulatedColors();
            }
        });
    }

    private void UpdateAccumulatedColors()
    {
        ExecuteSafely(() =>
        {
            // Update visual feedback through border colors instead of BoxView
            // The Entry controls are now wrapped in Frames, so we can update Frame border colors
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
                    double price = SelectedHose.Price;
                    if (PricePerGallonLabel != null)
                    {
                        PricePerGallonLabel.Text = $"{price:C3}";
                    }
                    
                    if (canEditPrice && PriceEditEntry != null)
                    {
                        PriceEditEntry.Text = price.ToString("F2");
                    }
                }
                else
                {
                    if (PricePerGallonLabel != null)
                    {
                        PricePerGallonLabel.Text = "##.###";
                    }
                    if (canEditPrice && PriceEditEntry != null)
                    {
                        PriceEditEntry.Text = "";
                    }
                }
                
                UpdatePriceEditVisibility();
            }
            else
            {
                if (PricePerGallonLabel != null)
                {
                    PricePerGallonLabel.Text = "##.###";
                }
                if (canEditPrice && PriceEditEntry != null)
                {
                    PriceEditEntry.Text = "";
                }
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
        ExecuteSafely(() => UpdateSelectedHosePrice());
    }

    private void PriceEditEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        ExecuteSafely(() =>
        {
            if (SelectedHose is not null && PriceEditEntry != null)
            {
                if (double.TryParse(e.NewTextValue, out double newPrice) && newPrice > 0)
                {
                    SelectedHose.Price = newPrice;
                    if (PricePerGallonLabel != null)
                    {
                        PricePerGallonLabel.Text = $"{newPrice:C2}";
                    }
                    
                    if (AccumulatedAmount > LastAccumulatedAmount)
                    {
                        double amountDifference = AccumulatedAmount - LastAccumulatedAmount;
                        // Fixed: Add division by zero check
                        if (newPrice > 0)
                        {
                            AccumulatedGallons = LastAccumulatedGallons + (amountDifference / newPrice);
                            UpdateAccumulatedColors();
                        }
                    }
                }
            }
        });
    }

    private void PriceEditButton_Clicked(object sender, EventArgs e)
    {
        ExecuteSafely(() => PriceEditEntry?.Focus());
    }

    private void UpdateSelectedHosePrice()
    {
        ExecuteSafely(() =>
        {
            if (SelectedHose is not null && PriceEditEntry != null)
            {
                if (double.TryParse(PriceEditEntry.Text, out double newPrice) && newPrice > 0)
                {
                    SelectedHose.Price = newPrice;
                    if (PricePerGallonLabel != null)
                    {
                        PricePerGallonLabel.Text = $"{newPrice:C2}";
                    }
                    
                    if (AccumulatedAmount > LastAccumulatedAmount)
                    {
                        double amountDifference = AccumulatedAmount - LastAccumulatedAmount;
                        
                        // Fixed: Add division by zero check
                        if (newPrice > 0)
                        {
                            AccumulatedGallons = LastAccumulatedGallons + (amountDifference / newPrice);
                            UpdateAccumulatedColors();
                        }
                    }
                }
                else
                {
                    PriceEditEntry.Text = SelectedHose.Price.ToString("F2");
                }
            }
        });
    }

    // Métodos de seguridad integrados en la clase
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
                // El popup ya fue dispuesto, esto es normal
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
}