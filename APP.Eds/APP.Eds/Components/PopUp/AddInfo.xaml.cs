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
        // Access the Editor by its AutomationId to get the entered text.
        var editor = this.FindByName<Editor>("EditorDescription");

        if (editor != null && !string.IsNullOrWhiteSpace(editor.Text))
        {
            // Assuming CourtService has a method to save additional info.
            // The exact method name and parameters are unknown.
            // I'll use a placeholder method `SaveAdditionalInfoAsync` for now.
            // The user might need to provide the correct method name or signature.
            await courtService.SaveAdditionalInfoAsync(editor.Text);
            Close(); // Close the popup after saving.
        }
        else
        {
            // If the editor is empty or not found, just close the popup.
            Close();
        }
    }


