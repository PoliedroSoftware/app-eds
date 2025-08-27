using APP.Eds.Services.HoseHistory;
using APP.Eds.Controls;

namespace APP.Eds.UsesCases.HoseHistory;

public partial class HoseHistoryPostView : ContentPage
{
    private HoseHistoryService _hosehistoryService;

    public HoseHistoryPostView()
    {
        InitializeComponent();
        _hosehistoryService = new HoseHistoryService();
        BindingContext = _hosehistoryService;
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        if (BindingContext is HoseHistoryService vm)
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
                if (vm.Date.Date < DateTime.Now.Date)
                {
                    await DisplayAlert("Error", "La fecha seleccionada no puede ser anterior a la fecha actual", "OK");
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

                if (vm.SelectHose is null)
                {
                    await DisplayAlert("Error", "Por favor seleccione una Manguera", "OK");
                    return;
                }

                await vm.SaveHoseHistoryDataAsync();
            }
            finally
            {
                LoadingOverlay.HideLoading();

                // Clear form fields after successful submission
                _hosehistoryService.Date = DateTime.Now;
                AccumulatedAmount = 0;
                AccumulatedGallons = 0;
                _hosehistoryService.SelectedDispensers = null;
                _hosehistoryService.SelectHose = null;

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

    public double AccumulatedAmount
    {
        get => _hosehistoryService.AccumulatedAmount;
        set
        {
            _hosehistoryService.AccumulatedAmount = value;
            OnPropertyChanged();
        }
    }
    
    public double AccumulatedGallons
    {
        get => _hosehistoryService.AccumulatedGallons;
        set
        {
            _hosehistoryService.AccumulatedGallons = value;
            OnPropertyChanged();
        }
    }
}
