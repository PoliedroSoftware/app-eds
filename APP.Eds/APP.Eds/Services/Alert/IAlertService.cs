namespace APP.Eds.Services.Alert;

public interface IAlertService
{
	Task ShowAlert(string title, string message, string cancelButton);
}

// Extension methods for additional functionality
public static class AlertServiceExtensions
{
	public static async Task ShowErrorAsync(string message, string title = "Error")
	{
		await AlertService.ShowErrorAsync(message, title);
	}

	public static async Task ShowWarningAsync(string message, string title = "Advertencia")
	{
		await AlertService.ShowWarningAsync(message, title);
	}

	public static async Task ShowInfoAsync(string message, string title = "Información")
	{
		await AlertService.ShowInfoAsync(message, title);
	}

	public static async Task ShowSuccessAsync(string message, string title = "Éxito")
	{
		await AlertService.ShowSuccessAsync(message, title);
	}

	public static async Task<bool> ShowConfirmAsync(string message, string title = "Confirmar", string confirmText = "Sí", string cancelText = "No")
	{
		return await AlertService.ShowConfirmAsync(message, title, confirmText, cancelText);
	}
}
