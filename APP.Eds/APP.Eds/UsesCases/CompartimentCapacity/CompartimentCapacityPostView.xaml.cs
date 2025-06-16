using System.Collections.ObjectModel;
using APP.Eds.Services.CompartimentCapacity;

namespace APP.Eds.UsesCases.CompartimentCapacity;

public partial class CompartimentCapacityPostView : ContentPage
{
    private CompartimentCapacityService _compartimentCapacityService;

    public CompartimentCapacityPostView()
    {
        InitializeComponent();
        _compartimentCapacityService = new CompartimentCapacityService();
        BindingContext = _compartimentCapacityService;
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        try
        {
            LoadingOverlay.ShowLoading();
            await _compartimentCapacityService.SaveCompartimentCapacityDataAsync();
        }
        finally
        {
            LoadingOverlay.HideLoading();

            _compartimentCapacityService.SelectCapacity = null;
            _compartimentCapacityService.SelectCompartiment= null;
            Default = 0;
        }
    }

    public byte Default
    {
        get => _compartimentCapacityService.Default;
        set
        {
            _compartimentCapacityService.Default = value;
            OnPropertyChanged();
        }
    }
}
