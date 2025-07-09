using APP.Eds.Services.Capacity;

namespace APP.Eds.UsesCases.Capacity;

public partial class CapacityPostView : ContentPage
{
    private CapacityService _capacityService;
    public CapacityPostView()
    {
        InitializeComponent();
        _capacityService = new CapacityService();
        BindingContext = _capacityService;
    }

    private async void SendCapacityButton(object sender, EventArgs e)
    {
        try
        {
            LoadingOverlay.ShowLoading();
            if (Code is null || Height is null || Gallon is null || Liters is null)
            {
                await DisplayAlert(_capacityService.Error, _capacityService.ErrorAllFieldsRequired, "OK");
                return;
            }
            await _capacityService.SaveCapacityDataAsync();
        }
        finally
        {
            LoadingOverlay.HideLoading();
            Code = null;
            Height = null;
            Gallon = null;
            Liters = null;
        }
        
    }
    public string? Code
    {
        get => _capacityService.Code;
        set
        {
            _capacityService.Code = value;
            OnPropertyChanged();
        }
    }
    public double? Height
    {
        get => _capacityService.Height;
        set
        {
            _capacityService.Height = value;
            OnPropertyChanged();
        }
    }

    public double? Gallon
    {
        get => _capacityService.Gallon;
        set
        {
            _capacityService.Gallon = value;
            OnPropertyChanged();
        }
    }

    public int? Liters
    {
        get => _capacityService.Liters;
        set
        {
            _capacityService.Liters  = value;
            OnPropertyChanged();
        }
    }
}