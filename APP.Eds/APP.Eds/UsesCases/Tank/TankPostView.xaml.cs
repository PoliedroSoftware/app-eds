using APP.Eds.Services.Tank;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace APP.Eds.UsesCases.Tank;

public partial class TankPostView : ContentPage
{
    private TankService _tankService;
    
    public TankPostView()
    {
        InitializeComponent();
        _tankService = new TankService();
        BindingContext = _tankService;
    }

    private async void SendData(object sender, EventArgs e)
    {
        if (BindingContext is TankService vm)
        {
            try
            {
                // Disable button to prevent multiple submissions
                if (sender is Button button)
                {
                    button.IsEnabled = false;
                    button.Text = "Enviando...";
                }

                // Validate required fields
                if (string.IsNullOrWhiteSpace(vm.Number))
                {
                    await DisplayAlert("Error", "Por favor ingrese el número del tanque", "OK");
                    return;
                }

                if (vm.Number.Length < 2)
                {
                    await DisplayAlert("Error", "El número del tanque debe tener al menos 2 caracteres", "OK");
                    return;
                }

                if (vm.Compartment <= 0)
                {
                    await DisplayAlert("Error", "Por favor ingrese un número de compartimientos válido (mayor que 0)", "OK");
                    return;
                }

                if (vm.Compartment > 10)
                {
                    await DisplayAlert("Error", "El número de compartimientos no puede ser mayor a 10", "OK");
                    return;
                }

                if (vm.Ability <= 0)
                {
                    await DisplayAlert("Error", "Por favor ingrese una capacidad válida (mayor que 0)", "OK");
                    return;
                }

                LoadingOverlay.ShowLoading();
                await vm.SaveTankDataAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al guardar el tanque: {ex.Message}", "OK");
            }
            finally
            {
                LoadingOverlay.HideLoading();

                // Clear form fields after successful submission
                Number = string.Empty;
                Compartment = null;
                Ability = null;
                Stock = null;

                // Re-enable button
                if (sender is Button button)
                {
                    button.IsEnabled = true;
                    button.Text = _tankService.SendData; // Restore original text from translations
                }
            }
        }
        else
        {
            await DisplayAlert("Error", "Error de contexto", "OK");
        }
    }

    public string Number
    {
        get => _tankService.Number;
        set
        {
            _tankService.Number = value;
            OnPropertyChanged();
        }
    }
    
    public int? Compartment
    {
        get => _tankService.Compartment;
        set
        {
            _tankService.Compartment = value;
            OnPropertyChanged();
        }
    }

    public double? Ability
    {
        get => _tankService.Ability;
        set
        {
            _tankService.Ability = value;
            OnPropertyChanged();
        }
    }
    
    public double? Stock
    {
        get => _tankService.Stock;
        set
        {
            _tankService.Stock = value;
            OnPropertyChanged();
        }
    }
}