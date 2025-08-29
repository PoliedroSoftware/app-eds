using APP.Eds.Components.PopUp;

namespace APP.Eds.Services.Alert;

public class AlertService : IAlertService
{
    public async Task ShowAlert(string title, string message, string cancelButton)
    {
        try
        {
            // Use custom alert if available, fallback to standard DisplayAlert
            await CustomAlert.ShowErrorAsync(message, title);
        }
        catch
        {
            // Fallback to standard DisplayAlert if custom alert fails
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(title, message, cancelButton);
            }
        }
    }

    // Additional methods for different alert types
    public static async Task ShowErrorAsync(string message, string title = "Error")
    {
        await CustomAlert.ShowErrorAsync(message, title);
    }

    public static async Task ShowWarningAsync(string message, string title = "Advertencia")
    {
        await CustomAlert.ShowWarningAsync(message, title);
    }

    public static async Task ShowInfoAsync(string message, string title = "Información")
    {
        await CustomAlert.ShowInfoAsync(message, title);
    }

    public static async Task ShowSuccessAsync(string message, string title = "Éxito")
    {
        await CustomAlert.ShowSuccessAsync(message, title);
    }

    public static async Task<bool> ShowConfirmAsync(string message, string title = "Confirmar", string confirmText = "Sí", string cancelText = "No")
    {
        return await CustomAlert.ShowConfirmAsync(message, title, confirmText, cancelText);
    }
}

