using APP.Eds.Services.Business;

namespace APP.Eds.UsesCases.Business;

public partial class BusinessPostView : ContentPage
{
    private BusinessService _businessService;

    public BusinessPostView()
    {
        InitializeComponent();
        _businessService = new BusinessService();
        BindingContext = _businessService;
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        try
        {
            // Disable button to prevent multiple submissions
            if (sender is Button button)
            {
                button.IsEnabled = false;
            }

            LoadingOverlay.ShowLoading();
            await _businessService.SaveBusinessDataAsync();
        }
        finally
        {
            LoadingOverlay.HideLoading();
            
            // Reset the form after successful submission
            Name = string.Empty;
            
            // Re-enable button
            if (sender is Button button)
            {
                button.IsEnabled = true;
            }
        }
    }

    public string Name
    {
        get => _businessService.Name;
        set
        {
            _businessService.Name = value;
            OnPropertyChanged();
        }
    }
}

