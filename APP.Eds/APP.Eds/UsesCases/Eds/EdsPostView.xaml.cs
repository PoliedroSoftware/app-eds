using System.Net;
using APP.Eds.Services.Eds;
using APP.Eds.Controls;

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
                // Disable button to prevent multiple submissions
                if (sender is HoverButton hoverButton)
                {
                    hoverButton.IsEnabled = false;
                }

                // Validate required fields
                if (string.IsNullOrWhiteSpace(_edsService.Name) ||
                    string.IsNullOrWhiteSpace(_edsService.Nit) || 
                    string.IsNullOrWhiteSpace(_edsService.Sicom) ||
                    string.IsNullOrWhiteSpace(_edsService.Address) ||
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

                // Clear form fields after successful submission
                _edsService.Name = string.Empty;
                _edsService.Nit = string.Empty;
                _edsService.Address = string.Empty;
                _edsService.Sicom = string.Empty;
                _edsService.SelectedBusiness = null;

                // Re-enable button
                if (sender is HoverButton hoverButton)
                {
                    hoverButton.IsEnabled = true;
                }
            }  
        }
    }
}