using APP.Eds.Services.Court;
using CommunityToolkit.Maui.Views;

namespace APP.Eds.Components.PopUp;

public partial class AddInfo : Popup
{
    private readonly CourtService courtService;

    public AddInfo(CourtService courtService)
    {
        InitializeComponent();
        this.courtService = courtService;
    }

    private void OnCloseTapped(object sender, EventArgs e)
    {
        Close();
    }
}   

