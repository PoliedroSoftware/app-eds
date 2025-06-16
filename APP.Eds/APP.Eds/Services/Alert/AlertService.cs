namespace APP.Eds.Services.Alert;

public class AlertService : IAlertService
{
	public async Task ShowAlert(string title, string message, string cancelButton)
	{
		if (Application.Current?.MainPage != null)
		{
			await Application.Current.MainPage.DisplayAlert(title, message, cancelButton);
		}
	}
}

