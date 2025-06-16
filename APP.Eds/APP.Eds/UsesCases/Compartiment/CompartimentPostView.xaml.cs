using APP.Eds.Services.Compartiment;

namespace APP.Eds.UsesCases.Compartiment;

public partial class CompartimentPostView : ContentPage
{
    private CompartimentService _compartimentService;
    public CompartimentPostView()
	{
		InitializeComponent();
        _compartimentService = new CompartimentService();
        BindingContext = _compartimentService;
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        if (BindingContext is CompartimentService vm)
        {
            try
            {
                LoadingOverlay.ShowLoading();
                if (vm.Number <= 0)
                {
                    await DisplayAlert("Error", "Please enter a valid Number (must be greater than 0)", "OK");
                    return;
                }

                if (vm.Nominal < 0)
                {
                    await DisplayAlert("Error", "Nominal value cannot be negative", "OK");
                    return;
                }

                if (vm.Operative < 0)
                {
                    await DisplayAlert("Error", "Operative value cannot be negative", "OK");
                    return;
                }

                if (vm.Stock < 0)
                {
                    await DisplayAlert("Error", "Stock cannot be negative", "OK");
                    return;
                }

                if (vm.Height < 0)
                {
                    await DisplayAlert("Error", "Height cannot be negative", "OK");
                    return;
                }

                if (vm.IdTank <= 0)
                {
                    await DisplayAlert("Error", "Please select a valid Tank", "OK");
                    return;
                }

                await vm.SaveCompartimentDataAsync();
            }

            finally
            {
                LoadingOverlay.HideLoading();

                Number = 0;
                Nominal = 0;
                Operative = 0;
                Stock = 0;
                Height = 0;
                IdTank = 0;
            }
            
        }
        else
        {
            await DisplayAlert("Error", "Context error", "OK");
        }
    }

    public int Number
    {
        get => _compartimentService.Number;
        set
        {
            _compartimentService.Number = value;
            OnPropertyChanged();
        }
    }

    public double Nominal
    {
        get => _compartimentService.Nominal;
        set
        {
            _compartimentService.Nominal = value;
            OnPropertyChanged();
        }
    }
    public double Operative
    {
        get => _compartimentService.Operative;
        set
        {
            _compartimentService.Operative = value;
            OnPropertyChanged();
        }
    }
    public double Stock
    {
        get => _compartimentService.Stock;
        set
        {
            _compartimentService.Stock = value;
            OnPropertyChanged();
        }
    }
    public double Height
    {
        get => _compartimentService.Height;
        set
        {
            _compartimentService.Height = value;
            OnPropertyChanged();
        }
    }
    public int IdTank
    {
        get => _compartimentService.IdTank;
        set
        {
            _compartimentService.IdTank = value;
            OnPropertyChanged();
        }
    }

}