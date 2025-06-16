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

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        if (BindingContext is TankService vm)
        {
            try
            {
                LoadingOverlay.ShowLoading();
                if (string.IsNullOrWhiteSpace(vm.Number))
                {
                    await DisplayAlert("Error", "Por favor, ingrese un n�mero v�lido (no puede estar vac�o)", "OK");
                    return;
                }

                if (vm.Compartment <= 0)
                {
                    await DisplayAlert("Error", "Por favor, ingrese un numero mayor que 0", "OK");
                    return;
                }

                if (vm.Stock <= 0)
                {
                    await DisplayAlert("Error", "Por favor, ingrese un numero mayor que 0", "OK");
                    return;
                }

                if (vm.Ability <= 0)
                {
                    await DisplayAlert("Error", "Por favor, ingrese un numero mayor que 0", "OK");
                    return;
                }

                await vm.SaveTankDataAsync();
            }

            finally
            {
                LoadingOverlay.HideLoading();

                Number = string.Empty;
                Compartment = 0;
                Ability = 0;
                Stock = 0;
            }
            
        }
        else
        {
            await DisplayAlert("Error", "Context error", "OK");
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
    public int Compartment
    {
        get => _tankService.Compartment;
        set
        {
            _tankService.Compartment = value;
            OnPropertyChanged();
        }
    }

    public double Ability
    {
        get => _tankService.Ability;
        set
        {
            _tankService.Ability = value;
            OnPropertyChanged();
        }
    }
    public double Stock
    {
        get => _tankService.Stock;
        set
        {
            _tankService.Stock = value;
            OnPropertyChanged();
        }
    }
}