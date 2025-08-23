using APP.Eds.Models.Compartiment;
using APP.Eds.Services.Compartiment;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace APP.Eds.UsesCases.Compartiment;

public partial class CompartimentPostView : ContentPage, INotifyPropertyChanged
{
    private CompartimentService _compartimentService;

    public ICommand EditCompartimentCommand { get; }
    public ICommand DeleteCompartimentCommand { get; }

    public CompartimentPostView()
	{
		InitializeComponent();
        _compartimentService = new CompartimentService();
        BindingContext = _compartimentService;
        EditCompartimentCommand = new Command<object>(OnEditCompartiment);
        DeleteCompartimentCommand = new Command<object>(OnDeleteCompartiment);
    }

    private void NumericEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is Entry entry)
        {
            string input = e.NewTextValue;
            bool esValido = double.TryParse(
                input,
                System.Globalization.NumberStyles.Any,
                new System.Globalization.CultureInfo("es-CO"),
                out _);

            if (!string.IsNullOrEmpty(input) && !esValido)
            {
                entry.Text = e.OldTextValue;
            }
        }
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        if (BindingContext is CompartimentService vm)
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
                if (vm.SelectedTank == null)
                {
                    await DisplayAlert("Error", "Por favor seleccione un tanque", "OK");
                    return;
                }

                if (vm.Number <= 0)
                {
                    await DisplayAlert("Error", "Por favor ingrese un número de compartimento válido (mayor que 0)", "OK");
                    return;
                }

                if (vm.Number > 20)
                {
                    await DisplayAlert("Error", "El número del compartimento no puede ser mayor a 20", "OK");
                    return;
                }

                if (vm.Nominal <= 0)
                {
                    await DisplayAlert("Error", "Por favor ingrese una capacidad nominal válida (mayor que 0)", "OK");
                    return;
                }

                if (vm.Operative <= 0)
                {
                    await DisplayAlert("Error", "Por favor ingrese una capacidad operativa válida (mayor que 0)", "OK");
                    return;
                }

                if (vm.Operative > vm.Nominal)
                {
                    await DisplayAlert("Error", "La capacidad operativa no puede ser mayor que la capacidad nominal", "OK");
                    return;
                }

                if (vm.Height <= 0)
                {
                    await DisplayAlert("Error", "Por favor ingrese una altura válida (mayor que 0)", "OK");
                    return;
                }

                if (vm.Height > 50)
                {
                    await DisplayAlert("Error", "La altura no puede ser mayor a 50 metros", "OK");
                    return;
                }

                LoadingOverlay.ShowLoading();
                await vm.SaveCompartimentDataAsync();
                
                // Refresh the compartment list after successful save
                await vm.GetCompartimentAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al guardar el compartimento: {ex.Message}", "OK");
            }
            finally
            {
                LoadingOverlay.HideLoading();

                // Clear form fields after successful submission
                Number = 0;
                Nominal = 0;
                Operative = 0;
                Stock = 0;
                Height = 0;
                IdTank = 0;

                // Re-enable button
                if (sender is Button button)
                {
                    button.IsEnabled = true;
                    button.Text = _compartimentService.SendData; // Restore original text from translations
                }
            }
        }
        else
        {
            await DisplayAlert("Error", "Error de contexto", "OK");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            LoadingOverlay.ShowLoading();
            await _compartimentService.GetCompartimentAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error cargando compartimentos: {ex.Message}", "OK");
        }
        finally
        {
            LoadingOverlay.HideLoading();
        }
    }

    public int Number
    {
        get => _compartimentService.Number;
        set
        {
            _compartimentService.Number = value;
            OnPropertyChanged();
        }
    }

    public double Nominal
    {
        get => _compartimentService.Nominal;
        set
        {
            _compartimentService.Nominal = value;
            OnPropertyChanged();
        }
    }
    
    public double Operative
    {
        get => _compartimentService.Operative;
        set
        {
            _compartimentService.Operative = value;
            OnPropertyChanged();
        }
    }
    
    public double Stock
    {
        get => _compartimentService.Stock;
        set
        {
            _compartimentService.Stock = value;
            OnPropertyChanged();
        }
    }
    
    public double Height
    {
        get => _compartimentService.Height;
        set
        {
            _compartimentService.Height = value;
            OnPropertyChanged();
        }
    }
    
    public int IdTank
    {
        get => _compartimentService.IdTank;
        set
        {
            _compartimentService.IdTank = value;
            OnPropertyChanged();
        }
    }

    private async void OnEditCompartiment(object obj)
    {
        if (obj is CompartimentResponse compartiment)
        {
            Number = compartiment.Number;
            Nominal = compartiment.Nominal;
            Operative = compartiment.Operative;
            Stock = compartiment.Stock;
            Height = compartiment.Height;
            IdTank = compartiment.IdTank;
            
            // Find and select the corresponding tank
            var tank = _compartimentService.TankList.FirstOrDefault(t => t.IdTank == compartiment.IdTank);
            if (tank != null)
            {
                _compartimentService.SelectedTank = tank;
            }

            await DisplayAlert("Editar", $"Datos del compartimento #{compartiment.Number} cargados para edición", "OK");
        }
    }

    private async void OnDeleteCompartiment(object obj)
    {
        if (obj is CompartimentResponse compartiment)
        {
            bool confirm = await DisplayAlert("Confirmar", 
                $"¿Desea eliminar el compartimento #{compartiment.Number}?", "Sí", "No");
            if (confirm)
            {
                try
                {
                    LoadingOverlay.ShowLoading();
                    bool deleted = await _compartimentService.DeleteCompartimentAsync(compartiment.IdCompartment);
                    if (deleted)
                    {
                        await DisplayAlert("Éxito", "Compartimento eliminado correctamente", "OK");
                    }
                }
                finally
                {
                    LoadingOverlay.HideLoading();
                }
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}