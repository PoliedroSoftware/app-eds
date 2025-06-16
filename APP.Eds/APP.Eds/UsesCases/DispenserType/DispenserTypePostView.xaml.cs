using APP.Eds.Services.DispenserType;

namespace APP.Eds.UsesCases.DispenserType;

public partial class DispenserTypePostView : ContentPage
{
    private DispenserTypeService _dispenserTypeService;
    public DispenserTypePostView()
	{
		InitializeComponent();
        _dispenserTypeService = new DispenserTypeService();
        BindingContext = _dispenserTypeService;
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        try
        {
            LoadingOverlay.ShowLoading();
            await _dispenserTypeService.SaveDispenserTypeDataAsync();
        }
        finally
        {
            LoadingOverlay.HideLoading();
            Description = string.Empty;
        }
        
    }

    public string Description
    {
        get => _dispenserTypeService.Description;
        set
        {
            _dispenserTypeService.Description = value;
            OnParentChanged();
        }
    }
}