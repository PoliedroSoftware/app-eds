using APP.Eds.Models.Compartiment;
using APP.Eds.Services.Compartiment;
using APP.Eds.Components.PopUp;
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

                // Enhanced validation with professional alerts
                if (vm.SelectedTank == null)
                {
                    await CustomAlert.ShowErrorAsync("Debe seleccionar un tanque antes de crear el compartimento", "Tanque Requerido");
                    return;
                }

                if (vm.Number <= 0)
                {
                    await CustomAlert.ShowErrorAsync("Debe especificar un número de compartimento válido (mayor que 0)", "Número Inválido");
                    return;
                }

                if (vm.Number > 20)
                {
                    await CustomAlert.ShowErrorAsync("El número del compartimento no puede ser mayor a 20 por limitaciones del sistema", "Número Excesivo");
                    return;
                }

                if (vm.Nominal <= 0)
                {
                    await CustomAlert.ShowErrorAsync("Debe especificar una capacidad nominal válida (mayor que 0 litros)", "Capacidad Nominal Inválida");
                    return;
                }

                if (vm.Operative <= 0)
                {
                    await CustomAlert.ShowErrorAsync("Debe especificar una capacidad operativa válida (mayor que 0 litros)", "Capacidad Operativa Inválida");
                    return;
                }

                if (vm.Operative > vm.Nominal)
                {
                    await CustomAlert.ShowErrorAsync($"La capacidad operativa ({vm.Operative:F2} L) no puede ser mayor que la capacidad nominal ({vm.Nominal:F2} L)", "Capacidad Inconsistente");
                    return;
                }

                // Warning if operative capacity is too close to nominal
                double efficiencyRatio = (vm.Operative / vm.Nominal) * 100;
                if (efficiencyRatio > 95)
                {
                    bool confirm = await CustomAlert.ShowConfirmAsync(
                        $"La capacidad operativa ({vm.Operative:F2} L) es muy cercana a la nominal ({vm.Nominal:F2} L).\n\nEficiencia: {efficiencyRatio:F1}%\n\n¿Está seguro de que estos valores son correctos?",
                        "Capacidades Muy Cercanas",
                        "Continuar",
                        "Revisar");
                    
                    if (!confirm) return;
                }

                if (vm.Height <= 0)
                {
                    await CustomAlert.ShowErrorAsync("Debe especificar una altura válida del compartimento (mayor que 0 metros)", "Altura Inválida");
                    return;
                }

                if (vm.Height > 50)
                {
                    await CustomAlert.ShowErrorAsync("La altura no puede ser mayor a 50 metros por razones de seguridad", "Altura Excesiva");
                    return;
                }

                // Validate stock if provided
                if (vm.Stock < 0)
                {
                    await CustomAlert.ShowErrorAsync("El stock actual no puede ser negativo", "Stock Inválido");
                    return;
                }

                if (vm.Stock > vm.Operative)
                {
                    bool confirmStock = await CustomAlert.ShowConfirmAsync(
                        $"El stock actual ({vm.Stock:F2} L) excede la capacidad operativa ({vm.Operative:F2} L).\n\n¿Confirma que este valor es correcto?",
                        "Stock Excede Capacidad",
                        "Confirmar",
                        "Revisar");
                    
                    if (!confirmStock) return;
                }

                LoadingOverlay.ShowLoading();
                await vm.SaveCompartimentDataAsync();
                
                // Refresh the compartment list after successful save
                await vm.GetCompartimentAsync();
                
                await CustomAlert.ShowSuccessAsync(
                    $"Compartimento #{vm.Number} creado exitosamente:\n\n" +
                    $"• Tanque: {vm.SelectedTank.Number}\n" +
                    $"• Capacidad Nominal: {vm.Nominal:F2} L\n" +
                    $"• Capacidad Operativa: {vm.Operative:F2} L\n" +
                    $"• Altura: {vm.Height:F2} m\n" +
                    $"• Stock Inicial: {vm.Stock:F2} L",
                    "Compartimento Creado");
            }
            catch (Exception ex)
            {
                await CustomAlert.ShowErrorAsync($"Error al guardar el compartimento:\n\n{ex.Message}", "Error del Sistema");
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
            await CustomAlert.ShowErrorAsync("Error interno del sistema. Por favor, intente nuevamente", "Error de Contexto");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Console.WriteLine("OnAppearing de CompartimentPostView iniciado.");
        try
        {
            LoadingOverlay.ShowLoading();
            Console.WriteLine("Intentando cargar datos de tanques...");
            await _compartimentService.GetAllTankData(); // Cargar la lista de tanques
            Console.WriteLine("Datos de tanques cargados (o intento de carga finalizado).");
            Console.WriteLine("Intentando cargar datos de compartimentos...");
            await _compartimentService.GetCompartimentAsync();
            Console.WriteLine("Datos de compartimentos cargados (o intento de carga finalizado).");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Excepción en OnAppearing: {ex.Message}");
            await CustomAlert.ShowErrorAsync($"Error al cargar la lista de compartimentos o tanques:\n\n{ex.Message}", "Error de Carga");
        }
        finally
        {
            LoadingOverlay.HideLoading();
            Console.WriteLine("OnAppearing de CompartimentPostView finalizado.");
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

            await CustomAlert.ShowInfoAsync($"Los datos del compartimento #{compartiment.Number} han sido cargados en el formulario para su edición", "Edición Activada");
        }
    }

    private async void OnDeleteCompartiment(object obj)
    {
        if (obj is CompartimentResponse compartiment)
        {
            bool confirm = await CustomAlert.ShowConfirmAsync(
                $"¿Está seguro de que desea eliminar el compartimento #{compartiment.Number}?\n\n" +
                $"• Capacidad Nominal: {compartiment.Nominal:F2} L\n" +
                $"• Capacidad Operativa: {compartiment.Operative:F2} L\n" +
                $"• Stock Actual: {compartiment.Stock:F2} L\n\n" +
                $"Esta acción no se puede deshacer.",
                "Confirmar Eliminación", 
                "Eliminar", 
                "Cancelar");
                
            if (confirm)
            {
                try
                {
                    LoadingOverlay.ShowLoading();
                    bool deleted = await _compartimentService.DeleteCompartimentAsync(compartiment.IdCompartment);
                    if (deleted)
                    {
                        await CustomAlert.ShowSuccessAsync($"El compartimento #{compartiment.Number} ha sido eliminado correctamente del sistema", "Compartimento Eliminado");
                        // Refresh list after deletion
                        await _compartimentService.GetCompartimentAsync();
                    }
                }
                catch (Exception ex)
                {
                    await CustomAlert.ShowErrorAsync($"Error al eliminar el compartimento:\n\n{ex.Message}", "Error de Eliminación");
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