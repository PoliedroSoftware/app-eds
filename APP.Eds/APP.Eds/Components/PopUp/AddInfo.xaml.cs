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
        try
        {
            Close();
        }
        catch (ObjectDisposedException ex)
        {
            System.Diagnostics.Debug.WriteLine($"AddInfo popup was already disposed during close: {ex.Message}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error closing AddInfo popup: {ex.Message}");
        }
    }

    private async void OnSaveTapped(object sender, EventArgs e)
    {
        var editor = this.FindByName<Editor>("EditorDescription");

        if (editor != null && !string.IsNullOrWhiteSpace(editor.Text))
        {
            await courtService.SaveAdditionalInfoAsync(editor.Text);
            
            try
            {
                Close();
            }
            catch (ObjectDisposedException ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddInfo popup was disposed after saving: {ex.Message}");
            }
        }
        else
        {
            try
            {
                Close();
            }
            catch (ObjectDisposedException ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddInfo popup was disposed without saving: {ex.Message}");
            }
        }
    }
}
