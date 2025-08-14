using APP.Eds.Models.Dispenser;
using APP.Eds.Services.Dispensers;
using APP.Eds.UsesCases.DispenserType;
using System.Windows.Input;

namespace APP.Eds.UsesCases.Dispensers;

public partial class DispensersPostView : ContentPage
{
    private DispensersService _dispensersService;
    public ICommand EditDispenserCommand { get; }
    public ICommand DeleteDispenserCommand { get; }

    public DispensersPostView()
	{
		InitializeComponent();
        _dispensersService = new DispensersService();
        BindingContext = _dispensersService;
        EditDispenserCommand = new Command<object>(OnEditDispenser);
        DeleteDispenserCommand = new Command<object>(OnDeleteDispenser);
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
                    await DisplayAlert("Error", "Por favor ingrese un código valido!", "OK");
                    return;
                }

                if (vm.Number <= 0)
                {
                    await DisplayAlert("Error", "Por favor Ingrese un número valido y/o mayor a 0!", "OK");
                    return;
                }

                if (vm.SelectedDispenserType is null)
                {
                    await DisplayAlert("Error", "Por favor ingrese un dispensador valido!", "OK");
                    return;
                }

                if (vm.HoseNumber <= 0)
                {
                    await DisplayAlert("Error", "Por favor ingrese un número de manguera valido y/o mayor a 0!", "OK");
                    return;
                }

                if (vm.SelectedEds is null)
                {
                    await DisplayAlert("Error", "Por favor seleccione un EDS valido!", "OK");
                    return;
                }

                if (vm.SelectedIsland is null)
                {
                    await DisplayAlert("Error", "Por favor seleccione una isla valida!", "OK");
                    return;
                }
                await vm.SaveDispensersDataAsync();

                _dispensersService.SelectedDispenserType = null;
                _dispensersService.SelectedEds = null;
                _dispensersService.SelectedIsland = null;
                Code = string.Empty;
                Number = 0;
                HoseNumber = 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"error: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Error", "Error de Conexion", "OK");
        }
        finally
        {
            LoadingOverlay.HideLoading();
        }   
        
    }

    private void OnAddDispenserTypeClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new    DispenserTypePostView());
    }

    private async void OnEditDispenser(object obj)
    {
        if (obj is DispenserModelResponse dispenser)
        {
            Code = dispenser.Code;
            Number = dispenser.Number;
            //HoseNumber = dispenser.HoseNumber;
            // Aquí puedes cargar los demás campos según tu modelo y UI
        }
    }

    private async void OnDeleteDispenser(object obj)
    {
        if (obj is DispenserModelResponse dispenser)
        {
            bool confirm = await DisplayAlert("Confirmar", $"¿Desea eliminar el dispensador {dispenser.Code}?", "Sí", "No");
            if (confirm)
            {
                await _dispensersService.DeleteDispenserAsync(dispenser.IdDispensers);
            }
        }
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

}