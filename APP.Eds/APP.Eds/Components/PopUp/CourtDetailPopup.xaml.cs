using System.Windows.Input;
using APP.Eds.Models.Court;
using CommunityToolkit.Maui.Views;

namespace APP.Eds.Components.PopUp;

public partial class CourtDetailPopup : Popup
{
    public CourtDetailPopup(CourtListItemModel court)
    {
        InitializeComponent();
        BindingContext = court;

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
