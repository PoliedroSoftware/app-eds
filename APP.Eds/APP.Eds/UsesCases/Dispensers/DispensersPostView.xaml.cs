using APP.Eds.Models.Dispenser;
using APP.Eds.Services.Dispensers;
using APP.Eds.UsesCases.DispenserType;
using System.Windows.Input;

namespace APP.Eds.UsesCases.Dispensers;

public partial class DispensersPostView : ContentPage
{
    private DispensersService _dispensersService;

    public DispensersPostView()
	{
		InitializeComponent();
        _dispensersService = new DispensersService();
        BindingContext = _dispensersService;
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        try
        {
            if (BindingContext is DispensersService vm)
            { 
                LoadingOverlay.ShowLoading();

                if (string.IsNullOrWhiteSpace(vm.Code))
                {
                    await DisplayAlert("Error", "Por favor ingrese un código válido!", "OK");
                    return;
                }

                if (vm.Number <= 0)
                {
                    await DisplayAlert("Error", "Por favor ingrese un número válido mayor a 0!", "OK");
                    return;
                }

                if (vm.SelectedDispenserType is null)
                {
                    await DisplayAlert("Error", "Por favor seleccione un tipo de dispensador!", "OK");
                    return;
                }

                if (vm.HoseNumber <= 0)
                {
                    await DisplayAlert("Error", "Por favor ingrese un número de mangueras válido mayor a 0!", "OK");
                    return;
                }

                if (vm.SelectedEds is null)
                {
                    await DisplayAlert("Error", "Por favor seleccione una EDS válida!", "OK");
                    return;
                }

                if (vm.SelectedIsland is null)
                {
                    await DisplayAlert("Error", "Por favor seleccione una isla válida!", "OK");
                    return;
                }

                await vm.SaveDispensersDataAsync();

                _dispensersService.SelectedDispenserType = null;
                _dispensersService.SelectedEds = null;
                _dispensersService.SelectedIsland = null;
                Code = string.Empty;
                Number = 0;
                HoseNumber = 0;

                await _dispensersService.GetDispensersAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            await DisplayAlert("Error", $"Error de conexión: {ex.Message}", "OK");
        }
        finally
        {
            LoadingOverlay.HideLoading();
        }   
    }

    private void OnAddDispenserTypeClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new DispenserTypePostView());
    }

    public string Code
    {
        get => _dispensersService.Code;
        set
        {
            _dispensersService.Code = value;
            OnPropertyChanged();
        }
    }

    public int Number
    {
        get => _dispensersService.Number;
        set
        {
            _dispensersService.Number = value;
            OnPropertyChanged();
        }
    }

    public int HoseNumber
    {
        get => _dispensersService.HoseNumber;
        set
        {
            _dispensersService.HoseNumber = value;
            OnPropertyChanged();
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            LoadingOverlay.ShowLoading();
            await _dispensersService.GetDispensersAsync();
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
}