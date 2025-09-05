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
        
        // Set BindingContext for proper data binding
        BindingContext = courtService;
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
        try
        {
            // Disable button to prevent multiple submissions
            if (sender is Button button)
            {
                button.IsEnabled = false;
                button.Text = "Guardando...";
            }

            var editor = this.FindByName<Editor>("EditorDescription");

            if (editor != null && !string.IsNullOrWhiteSpace(editor.Text))
            {
                // Validate description length
                if (editor.Text.Length < 10)
                {
                    await Application.Current.MainPage.DisplayAlert("Validación", 
                        "La descripción debe tener al menos 10 caracteres para ser informativa.", "OK");
                    return;
                }

                if (editor.Text.Length > 500)
                {
                    await Application.Current.MainPage.DisplayAlert("Validación", 
                        "La descripción no puede exceder 500 caracteres.", "OK");
                    return;
                }

                // Save the additional information
                await courtService.SaveAdditionalInfoAsync(editor.Text.Trim());
                
                // Show success feedback
                await Application.Current.MainPage.DisplayAlert("Éxito", 
                    "La información adicional ha sido guardada correctamente.", "OK");
                
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
                await Application.Current.MainPage.DisplayAlert("Validación", 
                    "Por favor, ingrese una descripción antes de guardar.", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving additional info: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Error", 
                $"Error al guardar la información: {ex.Message}", "OK");
        }
        finally
        {
            // Re-enable button
            if (sender is Button button)
            {
                button.IsEnabled = true;
                button.Text = "Guardar";
            }
        }
    }
}
