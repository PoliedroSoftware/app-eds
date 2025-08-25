using APP.Eds.Services.Island;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace APP.Eds.UsesCases.Island;

public partial class IslandPostView : ContentPage, INotifyPropertyChanged
{
    private IslandService _islandService;
    
    public IslandPostView()
    {
        InitializeComponent();
        _islandService = new IslandService();
        BindingContext = _islandService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            LoadingOverlay?.ShowLoading();
            await _islandService.InitializeAsync();
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
        var button = sender as Button;
        if (button != null)
        {
            button.IsEnabled = false;
        }

        try
        {
            // Enhanced validation
            if (string.IsNullOrWhiteSpace(_islandService.IslandName))
            {
                await DisplayAlert("Error", "Por favor, ingrese un nombre para la isla", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(_islandService.SelectedLocation))
            {
                await DisplayAlert("Error", "Por favor, seleccione la ubicacion de la isla", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(_islandService.SelectedType))
            {
                await DisplayAlert("Error", "Por favor, seleccione el tipo de isla", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(_islandService.SelectedCapacity))
            {
                await DisplayAlert("Error", "Por favor, seleccione la capacidad de dispensadores", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(_islandService.SelectedStatus))
            {
                await DisplayAlert("Error", "Por favor, seleccione el estado operativo", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(_islandService.Description))
            {
                await DisplayAlert("Error", "Por favor, agregue una descripcion de la isla", "OK");
                return;
            }

            LoadingOverlay?.ShowLoading();
            
            await _islandService.SaveIslandDataAsync();
            
            // Clear form after successful save
            ClearForm();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al guardar: {ex.Message}", "OK");
        }
        finally
        {
            LoadingOverlay?.HideLoading();
            if (button != null)
            {
                button.IsEnabled = true;
            }
        }
    }

    private void ClearForm()
    {
        if (_islandService != null)
        {
            _islandService.IslandName = string.Empty;
            _islandService.SelectedLocation = null;
            _islandService.SelectedType = null;
            _islandService.SelectedCapacity = null;
            _islandService.SelectedStatus = null;
            _islandService.Description = string.Empty;
            _islandService.InstallationDate = DateTime.Now;
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}