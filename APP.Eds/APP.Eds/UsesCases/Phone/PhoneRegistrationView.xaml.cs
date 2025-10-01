using APP.Eds.Services.Phone;

namespace APP.Eds.UsesCases.Phone;

public partial class PhoneRegistrationView : ContentPage
{
    private readonly PhoneService _phoneService;

    public PhoneRegistrationView()
    {
        InitializeComponent();
        _phoneService = new PhoneService();
        BindingContext = _phoneService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        try
        {
            // Cualquier inicialización adicional si es necesaria
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al cargar la vista: {ex.Message}", "OK");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _phoneService?.Dispose();
    }
}