using APP.Eds.Services.Hose;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        if (BindingContext is HoseService vm)
        {
            try
            {
                LoadingOverlay.ShowLoading();
                if (vm.Number <= 0)
                {
                    await DisplayAlert("Error", "Please enter a valid number (must be greater than 0)", "OK");
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

                if (vm.SelectProductType is null)
                {
                    await DisplayAlert("Error", "Please select a Product Type", "OK");
                    return;
                }

                await vm.SaveHoseDataAsync();
            }
            finally
            {
                LoadingOverlay.HideLoading();

                Number = 0;
                AccumulatedAmount = 0;
                AccumulatedGallons = 0;
                _hoseService.SelectedDispensers = null;
                _hoseService.SelectProductType = null;
            }
        }
        else
        {
            await DisplayAlert("Error", "Context error", "OK");
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