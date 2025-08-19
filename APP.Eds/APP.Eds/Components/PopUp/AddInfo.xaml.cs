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

    private async void OnSaveTapped(object sender, EventArgs e)
    {
        var editor = this.FindByName<Editor>("EditorDescription");

        if (editor != null && !string.IsNullOrWhiteSpace(editor.Text))
        {
            await courtService.SaveAdditionalInfoAsync(editor.Text);
            Close();
        }
        else
        {
            Close();
        }
    }
}
