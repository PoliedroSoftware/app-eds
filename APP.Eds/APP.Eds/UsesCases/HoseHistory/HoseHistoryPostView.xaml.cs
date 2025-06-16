using APP.Eds.Services.HoseHistory;

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
                LoadingOverlay.ShowLoading();
                if (vm.Date.Date <= DateTime.Now.Date)
                {
                    await DisplayAlert("Error", "La fecha seleccionada debe ser mayor a la fecha actual", "OK");
                    return;
                }

                if (vm.AccumulatedAmount <= 0)
                {
                    await DisplayAlert("Error", "Please enter a valid accumulated amount (must be greater than 0)", "OK");
                    return;
                }

                if (vm.AccumulatedGallons <= 0)
                {
                    await DisplayAlert("Error", "Please enter valid accumulated gallons (must be greater than 0)", "OK");
                    return;
                }

                if (vm.SelectedDispensers is null)
                {
                    await DisplayAlert("Error", "Please select a Dispenser", "OK");
                    return;
                }

                if (vm.SelectHose is null)
                {
                    await DisplayAlert("Error", "Please select a Product Type", "OK");
                    return;
                }

                await vm.SaveHoseHistoryDataAsync();
            }
            finally
            {
                LoadingOverlay.HideLoading();

                _hosehistoryService.Date = DateTime.Now;
                AccumulatedAmount = 0;
                AccumulatedGallons = 0;
                _hosehistoryService.SelectedDispensers = null;
                _hosehistoryService.SelectHose = null;

            }
        }
        else
        {
            await DisplayAlert("Error", "Context error", "OK");
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
