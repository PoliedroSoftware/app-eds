using CommunityToolkit.Maui.Views;
using System.Diagnostics;

namespace APP.Eds.Components.PopUp;

public partial class CustomAlert : Popup
{
    public enum AlertType
    {
        Error,
        Warning,
        Info,
        Success
    }

    private TaskCompletionSource<bool> _taskCompletionSource;
    private bool _isConfirm = false;

    public CustomAlert(string title, string message, string confirmText = "OK", string cancelText = null, AlertType alertType = AlertType.Error)
    {
        InitializeComponent();
        
        SetupAlert(title, message, confirmText, cancelText, alertType);
        _taskCompletionSource = new TaskCompletionSource<bool>();
    }

    private void SetupAlert(string title, string message, string confirmText, string cancelText, AlertType alertType)
    {
        // Set texts
        TitleLabel.Text = title;
        MessageLabel.Text = message;
        ConfirmButton.Text = confirmText;

        // Configure alert type appearance
        ConfigureAlertType(alertType);

        // Show/Hide cancel button
        if (!string.IsNullOrEmpty(cancelText))
        {
            CancelButton.Text = cancelText;
            CancelButtonBorder.IsVisible = true;
            _isConfirm = true;
        }
        else
        {
            CancelButtonBorder.IsVisible = false;
            _isConfirm = false;
        }
    }

    private void ConfigureAlertType(AlertType alertType)
    {
        switch (alertType)
        {
            case AlertType.Error:
                // Softer red colors - more muted
                HeaderBorder.BackgroundColor = Color.FromArgb("#E8A5A5"); // Light muted red
                ConfirmButtonBorder.BackgroundColor = Color.FromArgb("#D78787"); // Softer red
                // Use simple text symbols instead of emojis for better compatibility
                IconLabel.Text = "!";
                IconLabel.FontSize = 32;
                IconLabel.FontAttributes = FontAttributes.Bold;
                IconLabel.TextColor = Color.FromArgb("#8B0000"); // Dark red for contrast
                break;
                
            case AlertType.Warning:
                // Softer amber/orange colors
                HeaderBorder.BackgroundColor = Color.FromArgb("#F5E6A3"); // Light muted amber
                ConfirmButtonBorder.BackgroundColor = Color.FromArgb("#E8D078"); // Softer amber
                IconLabel.Text = "!";
                IconLabel.FontSize = 32;
                IconLabel.FontAttributes = FontAttributes.Bold;
                IconLabel.TextColor = Color.FromArgb("#B8860B"); // Dark amber for contrast
                break;
                
            case AlertType.Info:
                // Softer blue colors
                HeaderBorder.BackgroundColor = Color.FromArgb("#A8C8E1"); // Light muted blue
                ConfirmButtonBorder.BackgroundColor = Color.FromArgb("#7FB3D3"); // Softer blue
                IconLabel.Text = "i";
                IconLabel.FontSize = 32;
                IconLabel.FontAttributes = FontAttributes.Bold;
                IconLabel.TextColor = Color.FromArgb("#2F4F4F"); // Dark blue-gray for contrast
                break;
                
            case AlertType.Success:
                // Softer green colors
                HeaderBorder.BackgroundColor = Color.FromArgb("#B8E6B8"); // Light muted green
                ConfirmButtonBorder.BackgroundColor = Color.FromArgb("#90D690"); // Softer green
                IconLabel.Text = "?";
                IconLabel.FontSize = 28;
                IconLabel.FontAttributes = FontAttributes.Bold;
                IconLabel.TextColor = Color.FromArgb("#006400"); // Dark green for contrast
                break;
        }
    }

    public void SetDetailMessage(string detail)
    {
        if (!string.IsNullOrEmpty(detail))
        {
            DetailLabel.Text = detail;
            DetailLabel.IsVisible = true;
        }
    }

    private async void OnConfirmClicked(object sender, EventArgs e)
    {
        try
        {
            _taskCompletionSource?.TrySetResult(true);
            await CloseAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in OnConfirmClicked: {ex.Message}");
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        try
        {
            _taskCompletionSource?.TrySetResult(false);
            await CloseAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in OnCancelClicked: {ex.Message}");
        }
    }

    public Task<bool> ShowAsync()
    {
        try
        {
            Application.Current?.MainPage?.ShowPopup(this);
            return _taskCompletionSource.Task;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error showing custom alert: {ex.Message}");
            _taskCompletionSource?.TrySetResult(false);
            return _taskCompletionSource.Task;
        }
    }

    private async Task CloseAsync()
    {
        try
        {
            await Task.Delay(100); // Small delay for smooth animation
            Close();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error closing custom alert: {ex.Message}");
        }
    }

    // Static helper methods for easy usage
    public static async Task ShowErrorAsync(string message, string title = "Error")
    {
        var alert = new CustomAlert(title, message, "OK", null, AlertType.Error);
        await alert.ShowAsync();
    }

    public static async Task ShowWarningAsync(string message, string title = "Advertencia")
    {
        var alert = new CustomAlert(title, message, "OK", null, AlertType.Warning);
        await alert.ShowAsync();
    }

    public static async Task ShowInfoAsync(string message, string title = "Información")
    {
        var alert = new CustomAlert(title, message, "OK", null, AlertType.Info);
        await alert.ShowAsync();
    }

    public static async Task ShowSuccessAsync(string message, string title = "Éxito")
    {
        var alert = new CustomAlert(title, message, "OK", null, AlertType.Success);
        await alert.ShowAsync();
    }

    public static async Task<bool> ShowConfirmAsync(string message, string title = "Confirmar", string confirmText = "Sí", string cancelText = "No")
    {
        var alert = new CustomAlert(title, message, confirmText, cancelText, AlertType.Warning);
        return await alert.ShowAsync();
    }
}