using APP.Eds.Services.Business;
using CommunityToolkit.Maui.Views;

namespace APP.Eds.Components.PopUp;

public partial class OkBusiness : Popup
{
    private readonly BusinessService businessService;
    public OkBusiness(BusinessService businessService)
	{
		InitializeComponent();
        this.businessService = businessService;
    }
    private void OnCloseTapped(object sender, EventArgs e)
    {
        Close();
    }

    private async void GoToEds(object sender, EventArgs e)
    {
        Close();
        await Task.Delay(200);
        await Shell.Current.GoToAsync("/EdsPostView");
    }
}   