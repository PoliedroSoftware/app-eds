using APP.Eds.Services.EdsTank;

namespace APP.Eds.UsesCases.EdsTank;

public partial class EdsTankPostView : ContentPage
{
    private EdsTankService _edsTankService;

    public EdsTankPostView()
    {
        InitializeComponent();
        _edsTankService = new EdsTankService();
        BindingContext = _edsTankService;
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        try
        {
            LoadingOverlay.ShowLoading();
            await _edsTankService.SaveEdsTankDataAsync();
        }
        finally
        {
            LoadingOverlay.HideLoading();

            _edsTankService.SelectEds = null;
            _edsTankService.SelectTank = null;
        }
    }
}
