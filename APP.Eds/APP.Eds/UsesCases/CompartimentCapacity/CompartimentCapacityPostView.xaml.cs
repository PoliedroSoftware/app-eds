using System.Collections.ObjectModel;
using APP.Eds.Services.CompartimentCapacity;

namespace APP.Eds.UsesCases.CompartimentCapacity;

public partial class CompartimentCapacityPostView : ContentPage
{
    private CompartimentCapacityService _compartimentCapacityService;

    public CompartimentCapacityPostView()
    {
        InitializeComponent();
        _compartimentCapacityService = new CompartimentCapacityService();
        BindingContext = _compartimentCapacityService;
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        try
        {
            // Disable button to prevent multiple submissions
            if (sender is Button button)
            {
                button.IsEnabled = false;
                button.Text = "Enviando...";
            }

            // Validate required fields
            if (_compartimentCapacityService.SelectCapacity == null)
            {
                await DisplayAlert("Error", "Por favor seleccione un tanque", "OK");
                return;
            }

            if (_compartimentCapacityService.SelectCompartiment == null)
            {
                await DisplayAlert("Error", "Por favor seleccione un compartimento", "OK");
                return;
            }

            if (_compartimentCapacityService.Default <= 0)
            {
                await DisplayAlert("Error", "Por favor ingrese una capacidad válida (mayor que 0)", "OK");
                return;
            }

            if (_compartimentCapacityService.Default > 100000)
            {
                await DisplayAlert("Error", "La capacidad no puede exceder 100,000 litros", "OK");
                return;
            }

            // Show confirmation dialog
            string tankCode = _compartimentCapacityService.SelectCapacity.Code ?? "N/A";
            int compartmentNumber = _compartimentCapacityService.SelectCompartiment.Number;
            byte capacity = _compartimentCapacityService.Default;

            bool confirm = await DisplayAlert("Confirmar", 
                $"¿Desea asignar {capacity} L de capacidad al compartimento #{compartmentNumber} del tanque {tankCode}?", 
                "Sí", "No");

            if (!confirm) return;

            LoadingOverlay.ShowLoading();
            await _compartimentCapacityService.SaveCompartimentCapacityDataAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al guardar la configuración: {ex.Message}", "OK");
        }
        finally
        {
            LoadingOverlay.HideLoading();

            // Clear form fields after successful submission
            _compartimentCapacityService.SelectCapacity = null;
            _compartimentCapacityService.SelectCompartiment = null;
            Default = 0;

            // Re-enable button
            if (sender is Button button)
            {
                button.IsEnabled = true;
                button.Text = "?? Enviar Datos"; // Restore original text
            }
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            LoadingOverlay.ShowLoading();
            
            // Refresh data when page appears
            await Task.WhenAll(
                RefreshCapacityDataAsync(),
                RefreshCompartimentDataAsync()
            );
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error cargando datos: {ex.Message}", "OK");
        }
        finally
        {
            LoadingOverlay.HideLoading();
        }
    }

    private async Task RefreshCapacityDataAsync()
    {
        try
        {
            await _compartimentCapacityService.GetAllCapacityDataAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error refreshing capacity data: {ex.Message}");
        }
    }

    private async Task RefreshCompartimentDataAsync()
    {
        try
        {
            await _compartimentCapacityService.GetAllCompartimentDataAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error refreshing compartment data: {ex.Message}");
        }
    }

    public byte Default
    {
        get => _compartimentCapacityService.Default;
        set
        {
            _compartimentCapacityService.Default = value;
            OnPropertyChanged();
        }
    }
}
