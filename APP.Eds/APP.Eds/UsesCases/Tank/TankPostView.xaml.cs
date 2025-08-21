using APP.Eds.Services.Tank;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace APP.Eds.UsesCases.Tank;

public partial class TankPostView : ContentPage
{
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
                LoadingOverlay.ShowLoading();
                if (string.IsNullOrWhiteSpace(vm.Number))
                {
                    await DisplayAlert(_tankService.Error, _tankService.ErrorEnterNumber, "OK");
                    return;
                }

                if (vm.Compartment <= 0)
                {
                    await DisplayAlert(_tankService.Error, _tankService.ErrorNegativeNumber, "OK");
                    return;
                }

               

                if (vm.Ability <= 0)
                {
                    await DisplayAlert(_tankService.Error, _tankService.ErrorAbilityNegative, "OK");
                    return;
                }

                await vm.SaveTankDataAsync();
            }

            finally
            {
                LoadingOverlay.HideLoading();

                Number = string.Empty;
                Compartment = null;
                Ability = null;
                Stock = null;
            }
            
        }
        else
        {
            await DisplayAlert("Error", "Context error", "OK");
        }
    }
}