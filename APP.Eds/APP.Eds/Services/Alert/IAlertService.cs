namespace APP.Eds.Services.Alert;

public interface IAlertService
{
	Task ShowAlert(string title, string message, string cancelButton);
}
