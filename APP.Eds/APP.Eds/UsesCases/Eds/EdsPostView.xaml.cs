using System.Net;
using APP.Eds.Services.Eds;

namespace APP.Eds.UsesCases.Eds;

public partial class EdsPostView : ContentPage
{
    private EdsService _edsService;
    public EdsPostView()
    {
        InitializeComponent();
        _edsService = new EdsService();
        BindingContext = _edsService;
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        if (BindingContext is EdsService vm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace (_edsService.Name) ||
                    string.IsNullOrWhiteSpace (_edsService.Nit) || 
                    string.IsNullOrWhiteSpace (_edsService.Sicom) ||
                    string.IsNullOrWhiteSpace (_edsService.Address)||
                    _edsService.SelectedBusiness is null)
                {
                   await DisplayAlert("Error", $"{_edsService.ErrorEmpty}", "OK");
                   return;
                }
                LoadingOverlay.ShowLoading();
                var selectedId = vm.SelectedBusiness.IdBusiness;
                await vm.SaveEdsDataAsync();
            }
            finally
            {
                LoadingOverlay.HideLoading();

                _edsService.Name = string.Empty;
                _edsService.Nit = string.Empty;
                _edsService.Address = string.Empty;
                _edsService.Sicom = string.Empty;
                _edsService.SelectedBusiness = null;
            }  
        }
    }
}