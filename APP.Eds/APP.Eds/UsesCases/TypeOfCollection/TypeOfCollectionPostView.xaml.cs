using APP.Eds.Services.TypeOfCollection;
using APP.Eds.Components.PopUp;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace APP.Eds.UsesCases.TypeOfCollection;

public partial class TypeOfCollectionPostView : ContentPage, INotifyPropertyChanged
{
    private TypeOfCollectionService _typeOfCollectionService;

    public TypeOfCollectionPostView()
    {
        InitializeComponent();
        _typeOfCollectionService = new TypeOfCollectionService();
        BindingContext = _typeOfCollectionService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            LoadingOverlay?.ShowLoading();
            await _typeOfCollectionService.InitializeAsync();
            
            // ? NUEVO: Configurar total de ventas desde una fuente externa
            // Ejemplo: obtener desde preferencias, servicio de Court, etc.
            await ConfigureSalesTotalAsync();
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error al cargar los datos iniciales:\n\n{ex.Message}", "Error de Inicialización");
        }
        finally
        {
            LoadingOverlay?.HideLoading();
        }
    }

    /// <summary>
    /// ? NUEVO: Método para configurar el total de ventas
    /// </summary>
    private async Task ConfigureSalesTotalAsync()
    {
        try
        {
            // Opción 1: Obtener desde el servicio de Court si está disponible
            // var courtService = DependencyService.Get<CourtService>();
            // if (courtService != null)
            // {
            //     var totalVentas = courtService.GetTotalAmount();
            //     _typeOfCollectionService.SetSalesTotal((decimal)totalVentas);
            // }

            // Opción 2: Obtener desde preferencias
            var savedTotal = Preferences.Get("CurrentSalesTotal", 0.0);
            if (savedTotal > 0)
            {
                _typeOfCollectionService.SetSalesTotal((decimal)savedTotal);
            }

            // Opción 3: Permitir que el usuario ingrese el total manualmente
            // Esto se podría implementar con un Entry en la UI
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error configurando total de ventas: {ex.Message}");
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
            // Enhanced validation with professional alerts
            if (_typeOfCollectionService.SelectedPaymentType == null)
            {
                await CustomAlert.ShowErrorAsync("Debe seleccionar el tipo de pago que manejará este método", "Tipo de Pago Requerido");
                return;
            }

            if (_typeOfCollectionService.SelectedPaymentMethod == null)
            {
                await CustomAlert.ShowErrorAsync("Debe seleccionar el método de pago específico", "Método de Pago Requerido");
                return;
            }

            if (string.IsNullOrWhiteSpace(_typeOfCollectionService.PaymentName))
            {
                await CustomAlert.ShowErrorAsync("El nombre del método de pago es obligatorio para identificarlo", "Nombre Requerido");
                return;
            }

            if (_typeOfCollectionService.PaymentName.Length < 3)
            {
                await CustomAlert.ShowErrorAsync("El nombre del método de pago debe tener al menos 3 caracteres", "Nombre Muy Corto");
                return;
            }

            if (_typeOfCollectionService.PaymentName.Length > 50)
            {
                await CustomAlert.ShowErrorAsync("El nombre del método de pago no puede exceder 50 caracteres", "Nombre Muy Largo");
                return;
            }

            // Validate payment name characters (allow letters, numbers, spaces, and common symbols)
            if (!Regex.IsMatch(_typeOfCollectionService.PaymentName, @"^[\p{L}\p{N}\s\-_.,()]+$"))
            {
                await CustomAlert.ShowErrorAsync("El nombre contiene caracteres no válidos. Solo se permiten letras, números, espacios y símbolos básicos", "Caracteres Inválidos");
                return;
            }

            if (string.IsNullOrWhiteSpace(_typeOfCollectionService.PaymentProvider))
            {
                await CustomAlert.ShowErrorAsync("Debe especificar el proveedor o procesador del método de pago", "Proveedor Requerido");
                return;
            }

            if (!string.IsNullOrWhiteSpace(_typeOfCollectionService.ProcessingFee))
            {
                if (!decimal.TryParse(_typeOfCollectionService.ProcessingFee, out decimal fee) || fee < 0 || fee > 100)
                {
                    await CustomAlert.ShowErrorAsync("La comisión debe ser un valor numérico entre 0 y 100", "Comisión Inválida");
                    return;
                }
            }

            if (string.IsNullOrWhiteSpace(_typeOfCollectionService.SelectedStatus))
            {
                await CustomAlert.ShowErrorAsync("Debe seleccionar el estado del método de pago (Activo/Inactivo)", "Estado Requerido");
                return;
            }

            if (string.IsNullOrWhiteSpace(_typeOfCollectionService.Description))
            {
                await CustomAlert.ShowErrorAsync("Debe agregar observaciones o notas descriptivas del método de pago", "Descripción Requerida");
                return;
            }

            if (_typeOfCollectionService.Description.Length < 10)
            {
                await CustomAlert.ShowErrorAsync("La descripción debe ser más detallada (mínimo 10 caracteres)", "Descripción Muy Corta");
                return;
            }

            LoadingOverlay?.ShowLoading();
            
            await _typeOfCollectionService.SaveTypeOfCollectionDataAsync();
            
            // Clear form after successful save
            ClearForm();
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error al guardar el método de pago:\n\n{ex.Message}", "Error del Sistema");
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
        if (_typeOfCollectionService != null)
        {
            _typeOfCollectionService.SelectedPaymentType = null;
            _typeOfCollectionService.SelectedPaymentMethod = null;
            _typeOfCollectionService.PaymentName = string.Empty;
            _typeOfCollectionService.PaymentProvider = string.Empty;
            _typeOfCollectionService.ProcessingFee = string.Empty;
            _typeOfCollectionService.SelectedStatus = null;
            _typeOfCollectionService.RequiresAuth = false;
            _typeOfCollectionService.IsDefault = false;
            _typeOfCollectionService.Description = string.Empty;
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}