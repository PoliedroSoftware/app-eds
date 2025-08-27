using APP.Eds.Services.Court;
using APP.Eds.Components.PopUp;
using CommunityToolkit.Maui.Views;
using System.Diagnostics;

namespace APP.Eds.Components.PopUp;

public partial class AddDispenser : Popup
{
    private readonly CourtService courtService;
    private bool isGallonsEditable = false;
    private bool canEditPrice = false;
    private bool _isDisposed = false;
    private readonly object _disposeLock = new object();

    public AddDispenser(CourtService courtService)
    {
        InitializeComponent();
        this.courtService = courtService;
        SecondEntry.IsEnabled = isGallonsEditable;
        
        CheckUserRole();
        
        // Configurar binding context de forma segura
        BindingContext = courtService;
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
            if (BindingContext is CourtService vm && vm.SelectedHose is not null)
            {
                var selectedHoseId = vm.SelectedHose.IdHose;

                if (vm.AccumulatedAmount == 0)
                {
                    await CustomAlert.ShowErrorAsync("Debe ingresar el monto total de la venta para continuar", "Monto Requerido");
                    return;
                }

                if (vm.AccumulatedGallons == 0)
                {
                    await CustomAlert.ShowErrorAsync("Debe ingresar la cantidad de galones vendidos", "Galones Requeridos");
                    return;
                }

                if (vm.AccumulatedAmount < vm.LastAccumulatedAmount)
                {
                    await CustomAlert.ShowErrorAsync($"El monto acumulado (${vm.AccumulatedAmount:F2}) debe ser mayor que el último monto registrado (${vm.LastAccumulatedAmount:F2})", "Monto Inválido");
                    return;
                }

                if (vm.AccumulatedGallons < vm.LastAccumulatedGallons)
                {
                    await CustomAlert.ShowErrorAsync($"Los galones acumulados ({vm.AccumulatedGallons:F2}) deben ser mayores que los últimos galones registrados ({vm.LastAccumulatedGallons:F2})", "Galones Inválidos");
                    return;
                }

                await courtService.AddDispenserFromPopup();
                vm.AddSelectedHose(vm.SelectedHose);
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
            if (BindingContext is CourtService vm && vm.SelectedHose is not null)
            {
                if (vm.AccumulatedAmount > vm.LastAccumulatedAmount)
                {
                    double amountDifference = vm.AccumulatedAmount - vm.LastAccumulatedAmount;
                    double currentPrice = vm.SelectedHose.Price;
                    
                    if (currentPrice > 0)
                    {
                        vm.AccumulatedGallons = vm.LastAccumulatedGallons + (amountDifference / currentPrice);
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
            if (BindingContext is CourtService vm)
            {
                AmountBoxView.Color = vm.AccumulatedAmount >= vm.LastAccumulatedAmount ? Colors.Green : Colors.Red;
                GallonBoxView.Color = vm.AccumulatedGallons >= vm.LastAccumulatedGallons ? Colors.Green : Colors.Red;
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
            if (HosePicker.SelectedIndex != -1)
            {
                FirstEntry.IsEnabled = true;
                FirstEntry.Focus();
                FirstEntry.CursorPosition = FirstEntry.Text?.Length ?? 0;

                if (BindingContext is CourtService vm && vm.SelectedHose is not null)
                {
                    double price = vm.SelectedHose.Price;
                    PricePerGallonLabel.Text = $"{price:C3}";
                    
                    if (canEditPrice && PriceEditEntry != null)
                    {
                        PriceEditEntry.Text = price.ToString("F2");
                    }
                }
                else
                {
                    PricePerGallonLabel.Text = "##.###";
                    if (canEditPrice && PriceEditEntry != null)
                    {
                        PriceEditEntry.Text = "";
                    }
                }
                
                UpdatePriceEditVisibility();
            }
            else
            {
                PricePerGallonLabel.Text = "##.###";
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
            if (BindingContext is CourtService vm && vm.SelectedHose is not null && PriceEditEntry != null)
            {
                if (double.TryParse(e.NewTextValue, out double newPrice) && newPrice > 0)
                {
                    vm.SelectedHose.Price = newPrice;
                    PricePerGallonLabel.Text = $"{newPrice:C2}";
                    
                    if (vm.AccumulatedAmount > vm.LastAccumulatedAmount)
                    {
                        double amountDifference = vm.AccumulatedAmount - vm.LastAccumulatedAmount;
                        vm.AccumulatedGallons = vm.LastAccumulatedGallons + (amountDifference / newPrice);
                        UpdateAccumulatedColors();
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
            if (BindingContext is CourtService vm && vm.SelectedHose is not null && PriceEditEntry != null)
            {
                if (double.TryParse(PriceEditEntry.Text, out double newPrice) && newPrice > 0)
                {
                    vm.SelectedHose.Price = newPrice;
                    PricePerGallonLabel.Text = $"{newPrice:C2}";
                    
                    if (vm.AccumulatedAmount > vm.LastAccumulatedAmount)
                    {
                        double amountDifference = vm.AccumulatedAmount - vm.LastAccumulatedAmount;
                        vm.AccumulatedGallons = vm.LastAccumulatedGallons + (amountDifference / newPrice);
                        
                        UpdateAccumulatedColors();
                    }
                }
                else
                {
                    PriceEditEntry.Text = vm.SelectedHose.Price.ToString("F2");
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
            await asyncAction?.Invoke();
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

    private async Task DisplayAlertSafely(string title, string message, string cancel)
    {
        try
        {
            if (!_isDisposed && Application.Current?.MainPage != null)
            {
                // Use CustomAlert instead of standard DisplayAlert
                await CustomAlert.ShowErrorAsync(message, title);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{GetType().Name}: Error displaying alert - {ex.Message}");
        }
    }

    /// <summary>
    /// Resetea el ViewModel a valores por defecto
    /// </summary>
    private void ResetViewModel()
    {
        ExecuteSafely(() =>
        {
            if (BindingContext is CourtService vm)
            {
                vm.AccumulatedAmount = 0;
                vm.AccumulatedGallons = 0;
                vm.LastAccumulatedAmount = 0;
                vm.LastAccumulatedGallons = 0;
            }
        });
    }
}