using APP.Eds.Services.Business;
using APP.Eds.Components.PopUp;
 
 namespace APP.Eds.UsesCases.Bussines;

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
       await _businessService.GetBusinessList();
   }
    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        try
        {
            // Disable button to prevent multiple submissions
            if (sender is Button button)
            {
                button.IsEnabled = false;
                button.Text = "Guardando...";
            }

            // Enhanced validation with professional alerts
            if (string.IsNullOrWhiteSpace(_businessService.Name))
            {
                await CustomAlert.ShowErrorAsync("El nombre del negocio es obligatorio para el registro", "Nombre Requerido");
                return;
            }

            if (_businessService.Name.Length < 3)
            {
                await CustomAlert.ShowErrorAsync("El nombre del negocio debe tener al menos 3 caracteres", "Nombre Muy Corto");
                return;
            }

            if (_businessService.Name.Length > 100)
            {
                await CustomAlert.ShowErrorAsync("El nombre del negocio no puede exceder 100 caracteres", "Nombre Muy Largo");
                return;
            }

            // Clean up name
            string originalName = _businessService.Name;
            _businessService.Name = _businessService.Name.Trim();
            
            if (originalName != _businessService.Name)
            {
                await CustomAlert.ShowInfoAsync("Los espacios extra han sido removidos autom�ticamente del nombre", "Nombre Limpiado");
            }

            this.LoadingOverlay.ShowLoading();
            await _businessService.SaveBusinessDataAsync();
            
            await CustomAlert.ShowSuccessAsync($"El negocio '{_businessService.Name}' ha sido registrado exitosamente en el sistema", "Negocio Registrado");
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error al guardar el negocio:\n\n{ex.Message}", "Error del Sistema");
        }
        finally
        {
            this.LoadingOverlay.HideLoading();
            
            // Reset the form after successful submission
            Name = string.Empty;
            
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
}

