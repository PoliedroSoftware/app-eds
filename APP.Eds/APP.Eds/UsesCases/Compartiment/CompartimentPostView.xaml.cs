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
    private string _newDispenserNumber;
    private string _newDispenserNominal;

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
                LoadingOverlay.ShowLoading();

                if (vm.Number <= 0)
                {
                    await DisplayAlert("Error", "Debe ingresar un n�mero v�lido mayor que 0.", "OK");
                    return;
                }

                if (vm.Nominal <= 0)
                {
                    await DisplayAlert("Error", "Debe ingresar un valor nominal mayor que 0.", "OK");
                    return;
                }

                if (vm.Operative <= 0)
                {
                    await DisplayAlert("Error", "Debe ingresar un valor operativo mayor que 0.", "OK");
                    return;
                }

                if (vm.Stock <= 0)
                {
                    await DisplayAlert("Error", "Debe ingresar un stock mayor que 0.", "OK");
                    return;
                }

                if (vm.Height <= 0)
                {
                    await DisplayAlert("Error", "Debe ingresar una altura mayor que 0.", "OK");
                    return;
                }

                if (vm.SelectedTank == null || vm.IdTank <= 0)
                {
                    await DisplayAlert("Error", "Debe seleccionar un tanque v�lido.", "OK");
                    return;
                }

                await vm.SaveCompartimentDataAsync();
                
                Number = 0;
                Nominal = 0;
                Operative = 0;
                Stock = 0;
                Height = 0;
                IdTank = 0;

            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error inesperado: {ex.Message}", "OK");
            }
            finally
            {
                LoadingOverlay.HideLoading();
            }
        }
        else
        {
            await DisplayAlert("Error", "No se pudo obtener el contexto.", "OK");
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

}