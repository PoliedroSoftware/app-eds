using APP.Eds.Services.Hose;
using APP.Eds.UsesCases.Compartiment;
using APP.Eds.Controls;

namespace APP.Eds.UsesCases.Hose;

public partial class HosePostView : ContentPage
{
    private HoseService _hoseService;

    public HosePostView()
    {
        InitializeComponent();
        _hoseService = new HoseService();
        BindingContext = _hoseService;
    }

    private async void OnCompartimentButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CompartimentPostView());
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        if (BindingContext is HoseService vm)
        {
            try
            {
                // Disable button to prevent multiple submissions
                if (sender is HoverButton hoverButton)
                {
                    hoverButton.IsEnabled = false;
                }

                LoadingOverlay.ShowLoading();
                
                // Validate required fields
                if (vm.Number <= 0)
                {
                    await DisplayAlert("Error", "Por favor ingrese un número válido (debe ser mayor que 0)", "OK");
                    return;
                }

                if (vm.AccumulatedAmount <= 0)
                {
                    await DisplayAlert("Error", "Por favor ingrese un monto acumulado válido (debe ser mayor que 0)", "OK");
                    return;
                }

                if (vm.AccumulatedGallons <= 0)
                {
                    await DisplayAlert("Error", "Por favor ingrese galones acumulados válidos (debe ser mayor que 0)", "OK");
                    return;
                }

                if (vm.SelectedDispensers is null)
                {
                    await DisplayAlert("Error", "Por favor seleccione un Dispensador", "OK");
                    return;
                }

                if (vm.SelectProductType is null)
                {
                    await DisplayAlert("Error", "Por favor seleccione un Tipo de Producto", "OK");
                    return;
                }

                await vm.SaveHoseDataAsync();
            }
            finally
            {
                LoadingOverlay.HideLoading();

                // Clear form fields after successful submission
                Number = 0;
                AccumulatedAmount = 0;
                AccumulatedGallons = 0;
                _hoseService.SelectedDispensers = null;
                _hoseService.SelectProductType = null;

                // Re-enable button
                if (sender is HoverButton hoverButton)
                {
                    hoverButton.IsEnabled = true;
                }
            }
        }
        else
        {
            await DisplayAlert("Error", "Error de contexto", "OK");
        }
    }

    public int Number
    {
        get => _hoseService.Number;
        set
        {
            _hoseService.Number = value;
            OnPropertyChanged();
        }
    }

    public double AccumulatedAmount
    {
        get => _hoseService.AccumulatedAmount;
        set
        {
            _hoseService.AccumulatedAmount = value;
            OnPropertyChanged();
        }
    }

    public double AccumulatedGallons
    {
        get => _hoseService.AccumulatedGallons;
        set
        {
            _hoseService.AccumulatedGallons = value;
            OnPropertyChanged();
        }
    }
}