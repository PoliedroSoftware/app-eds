using APP.Eds.Services.IoT;

namespace APP.Eds.UsesCases.IoT;

public partial class ValveControlPage : ContentPage
{
    private readonly IoTService _iotService;

    public ValveControlPage()
    {
        InitializeComponent();
        _iotService = new IoTService();
        BindingContext = _iotService;
    }

    private async void OnValveToggled(object sender, ToggledEventArgs e)
    {
        try
        {
            // Disable the switch temporarily to prevent multiple rapid toggles
            var switchControl = sender as Switch;
            if (switchControl != null)
            {
                switchControl.IsEnabled = false;
            }

            // Send command to toggle valve
            bool success = await _iotService.PublishValveCommandAsync(e.Value);

            // If the command failed, revert the switch state
            if (!success && switchControl != null)
            {
                // Prevent event from firing again
                switchControl.Toggled -= OnValveToggled;
                switchControl.IsToggled = !e.Value;
                switchControl.Toggled += OnValveToggled;
            }

            // Re-enable the switch
            if (switchControl != null)
            {
                switchControl.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error toggling valve: {ex.Message}");
            
            // Revert switch if there was an error
            var switchControl = sender as Switch;
            if (switchControl != null)
            {
                switchControl.Toggled -= OnValveToggled;
                switchControl.IsToggled = !e.Value;
                switchControl.Toggled += OnValveToggled;
                switchControl.IsEnabled = true;
            }
        }
    }

    private async void OnOpenValveClicked(object sender, EventArgs e)
    {
        try
        {
            var button = sender as Button;
            if (button != null)
            {
                button.IsEnabled = false;
            }

            await _iotService.OpenValveAsync();

            if (button != null)
            {
                button.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error opening valve: {ex.Message}");
            
            var button = sender as Button;
            if (button != null)
            {
                button.IsEnabled = true;
            }
        }
    }

    private async void OnCloseValveClicked(object sender, EventArgs e)
    {
        try
        {
            var button = sender as Button;
            if (button != null)
            {
                button.IsEnabled = false;
            }

            await _iotService.CloseValveAsync();

            if (button != null)
            {
                button.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error closing valve: {ex.Message}");
            
            var button = sender as Button;
            if (button != null)
            {
                button.IsEnabled = true;
            }
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _iotService?.Dispose();
    }
}
