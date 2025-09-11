using APP.Eds.Services.Business;
using APP.Eds.Components.PopUp;

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

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            LoadingOverlay?.ShowLoading();
            await _businessService.InitializeAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al cargar datos: {ex.Message}", "OK");
        }
        finally
        {
            LoadingOverlay?.HideLoading();
        }
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        try
        {
            // Disable button to prevent multiple submissions
            if (sender is Button button)
            {
                button.IsEnabled = false;
                button.Text = "Registrando...";
            }

            // Show loading
            LoadingOverlay?.ShowLoading();

            // Execute save command
            await _businessService.SaveBusinessDataAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al registrar el negocio: {ex.Message}", "OK");
        }
        finally
        {
            LoadingOverlay?.HideLoading();
            
            // Re-enable button
            if (sender is Button button)
            {
                button.IsEnabled = true;
                button.Text = "?? Registrar Negocio";
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

    public string Description
    {
        get => _businessService.Description;
        set
        {
            _businessService.Description = value;
            OnPropertyChanged();
        }
    }

    public string BusinessNotes
    {
        get => _businessService.BusinessNotes;
        set
        {
            _businessService.BusinessNotes = value;
            OnPropertyChanged();
        }
    }
}

