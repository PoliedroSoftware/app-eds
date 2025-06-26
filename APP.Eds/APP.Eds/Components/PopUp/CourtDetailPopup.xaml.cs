using System.Net.Http.Headers;
using System.Text.Json;
using System.Windows.Input;

using APP.Eds.Helpers;
using APP.Eds.Models.Court;
using APP.Eds.Models.Translations;
using APP.Eds.Services.Config;
using APP.Eds.Services.Court;
using CommunityToolkit.Maui.Views;

namespace APP.Eds.Components.PopUp;

public partial class CourtDetailPopup : Popup
{
    private readonly CourtService courtService;
    private string? _authToken;
   
    public CourtDetailPopup(
        CourtListItemModel court)
    {
        InitializeComponent();
        BindingContext = court;
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
        AdjustSize();

        Application.Current.MainPage.Window.SizeChanged += (s, e) =>
        {
            AdjustSize();
        };
    }
       
    private void AdjustSize()
    {
        var window = Application.Current.MainPage.Window;

        Frame.WidthRequest = window.Width * 0.8;
        Frame.HeightRequest = window.Height * 0.8;
    }

    private void OnCloseTapped(object sender, EventArgs e)
    {
        Close();
    }

}
