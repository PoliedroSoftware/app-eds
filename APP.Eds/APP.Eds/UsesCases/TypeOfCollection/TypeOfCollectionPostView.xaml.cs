using APP.Eds.Services.TypeOfCollection;
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
            if (string.IsNullOrWhiteSpace(_typeOfCollectionService.SelectedPaymentType))
            {
                await DisplayAlert("Error", "Por favor, seleccione un tipo de pago", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(_typeOfCollectionService.SelectedPaymentMethod))
            {
                await DisplayAlert("Error", "Por favor, seleccione un metodo de pago", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(_typeOfCollectionService.PaymentName))
            {
                await DisplayAlert("Error", "Por favor, ingrese un nombre para la forma de pago", "OK");
                return;
            }

            // Validate payment name characters (allow letters, numbers, spaces, and common symbols)
            if (!Regex.IsMatch(_typeOfCollectionService.PaymentName, @"^[\p{L}\p{N}\s\-_.,()]+$"))
            {
                await DisplayAlert("Error", "El nombre contiene caracteres no validos", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(_typeOfCollectionService.PaymentProvider))
            {
                await DisplayAlert("Error", "Por favor, ingrese el proveedor/procesador", "OK");
                return;
            }

            if (!string.IsNullOrWhiteSpace(_typeOfCollectionService.ProcessingFee))
            {
                if (!decimal.TryParse(_typeOfCollectionService.ProcessingFee, out decimal fee) || fee < 0 || fee > 100)
                {
                    await DisplayAlert("Error", "La comision debe ser un valor numerico entre 0 y 100", "OK");
                    return;
                }
            }

            if (string.IsNullOrWhiteSpace(_typeOfCollectionService.SelectedStatus))
            {
                await DisplayAlert("Error", "Por favor, seleccione el estado de la forma de pago", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(_typeOfCollectionService.Description))
            {
                await DisplayAlert("Error", "Por favor, agregue observaciones o notas", "OK");
                return;
            }

            LoadingOverlay?.ShowLoading();
            
            await _typeOfCollectionService.SaveTypeOfCollectionDataAsync();
            
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