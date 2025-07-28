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
        BusinessNameErrorLabel.IsVisible = false; // Ocultar el mensaje de error al intentar enviar

        if (string.IsNullOrWhiteSpace(Name) || !System.Text.RegularExpressions.Regex.IsMatch(Name, @"^[a-zA-Z\s]+$"))
        {
            BusinessNameErrorLabel.Text = "Por favor, ingrese un nombre válido. Solo se permiten letras.";
            BusinessNameErrorLabel.IsVisible = true;
            await DisplayAlert("Error de Validación", "Por favor, ingrese un nombre válido. Solo se permiten letras.", "OK");
            return;
        }

        try
        {
            LoadingOverlay.ShowLoading();
            await _businessService.SaveBusinessDataAsync();
            await DisplayAlert("Éxito", "Datos de negocio guardados correctamente.", "OK"); // Mensaje de éxito
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error al guardar los datos: {ex.Message}", "OK"); // Mensaje de error general
        }
        finally
        {
            LoadingOverlay.HideLoading();
            Name = string.Empty;
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

