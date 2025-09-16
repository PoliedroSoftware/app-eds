using APP.Eds.Services.Court;
using APP.Eds.Components.PopUp;
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
                    await CustomAlert.ShowWarningAsync("La descripción debe tener al menos 10 caracteres para ser informativa.", "Descripción Muy Corta");
                    return;
                }

                if (editor.Text.Length > 500)
                {
                    await CustomAlert.ShowWarningAsync("La descripción no puede exceder 500 caracteres.", "Descripción Muy Larga");
                    return;
                }

                // Save the additional information
                await courtService.SaveAdditionalInfoAsync(editor.Text.Trim());
                
                // Show success feedback
                await CustomAlert.ShowSuccessAsync(
                    $"La información adicional ha sido guardada correctamente:\n\n" +
                    $"• Caracteres guardados: {editor.Text.Trim().Length}\n" +
                    $"• Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}", 
                    "Información Guardada");
                
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
                await CustomAlert.ShowWarningAsync("Por favor, ingrese una descripción antes de guardar.", "Descripción Requerida");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving additional info: {ex.Message}");
            await CustomAlert.ShowErrorAsync($"Error al guardar la información:\n\n{ex.Message}", "Error del Sistema");
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
