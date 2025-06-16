using APP.Eds.Services.Dispensers;

namespace APP.Eds.UsesCases.Dispensers;

public partial class DispensersPostView : ContentPage
{
    private DispensersService _dispensersService;
    public DispensersPostView()
	{
		InitializeComponent();
        _dispensersService = new DispensersService();
        BindingContext = _dispensersService;
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        if (BindingContext is DispensersService vm)
        {
            try
            {
                LoadingOverlay.ShowLoading();
                if (string.IsNullOrWhiteSpace(vm.Code))
                {
                    await DisplayAlert("Error", "Please enter a valid Code", "OK");
                    return;
                }

                if (vm.Number <= 0)
                {
                    await DisplayAlert("Error", "Please enter a valid Number (must be greater than 0)", "OK");
                    return;
                }

                if (vm.DispenserTypeId <= 0)
                {
                    await DisplayAlert("Error", "Please select a valid Dispenser Type", "OK");
                    return;
                }

                if (vm.HoseNumber <= 0)
                {
                    await DisplayAlert("Error", "Please enter a valid Hose Number (must be greater than 0)", "OK");
                    return;
                }

                if (vm.EdsId <= 0)
                {
                    await DisplayAlert("Error", "Please select a valid EDS", "OK");
                    return;
                }

                if (vm.IdIsland <= 0)
                {
                    await DisplayAlert("Error", "Please select a valid Island", "OK");
                    return;
                }

                await vm.SaveDispensersDataAsync();
            }
            finally
            {
                LoadingOverlay.HideLoading();

                _dispensersService.SelectedDispenserType = null;
                _dispensersService.SelectedEds = null;
                _dispensersService.SelectedIsland = null;
                Code = string.Empty;
                Number = 0;
                HoseNumber = 0;

            }
             
        }
        else
        {
            await DisplayAlert("Error", "Context error", "OK");
        }
    }

    public string Code
    {
        get => _dispensersService.Code;
        set
        {
            _dispensersService.Code = value;
            OnPropertyChanged();
        }
    }

    public int Number
    {
        get => _dispensersService.Number;
        set
        {
            _dispensersService.Number = value;
            OnPropertyChanged();
        }
    }

    public int HoseNumber
    {
        get => _dispensersService.HoseNumber;
        set
        {
            _dispensersService.HoseNumber = value;
            OnPropertyChanged();
        }
    }

}