using APP.Eds.Services.Islander;
using APP.Eds.Components.PopUp;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace APP.Eds.UsesCases.Islander;

public partial class IslanderPostView : ContentPage, INotifyPropertyChanged
{
    private IslanderService _islanderService;
    
    public IslanderPostView()
    {
        InitializeComponent();
        _islanderService = new IslanderService();
        BindingContext = _islanderService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            // Show loading while refreshing the islanders list
            LoadingOverlay?.ShowLoading();
            await _islanderService.InitializeAsync();
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error al cargar los datos iniciales:\n\n{ex.Message}", "Error de Inicializaci�n");
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
            // Enhanced validation with professional alerts
            if (string.IsNullOrWhiteSpace(_islanderService.Name))
            {
                await CustomAlert.ShowErrorAsync("El nombre completo del islero es obligatorio para el registro", "Nombre Requerido");
                return;
            }

            if (string.IsNullOrWhiteSpace(_islanderService.FirstName))
            {
                await CustomAlert.ShowErrorAsync("El primer nombre es obligatorio para completar el registro", "Primer Nombre Requerido");
                return;
            }

            if (string.IsNullOrWhiteSpace(_islanderService.LastName))
            {
                await CustomAlert.ShowErrorAsync("Los apellidos son obligatorios para identificar al islero", "Apellidos Requeridos");
                return;
            }

            if (string.IsNullOrWhiteSpace(_islanderService.Email))
            {
                await CustomAlert.ShowErrorAsync("El correo electr�nico es necesario para las comunicaciones del sistema", "Email Requerido");
                return;
            }

            // Enhanced email validation
            if (!IsValidEmail(_islanderService.Email))
            {
                await CustomAlert.ShowErrorAsync("Por favor ingrese un correo electr�nico v�lido (ejemplo: usuario@dominio.com)", "Email Inv�lido");
                return;
            }

            if (_islanderService.SelectedEds == null)
            {
                await CustomAlert.ShowErrorAsync("Debe seleccionar la estaci�n de servicio (EDS) donde trabajar� el islero", "EDS Requerida");
                return;
            }

            if (string.IsNullOrWhiteSpace(_islanderService.SelectedRole))
            {
                await CustomAlert.ShowErrorAsync("Debe seleccionar el rol o posici�n que tendr� el islero en la estaci�n", "Rol Requerido");
                return;
            }

            if (string.IsNullOrWhiteSpace(_islanderService.Password))
            {
                await CustomAlert.ShowErrorAsync("Debe establecer una contrase�a de acceso al sistema", "Contrase�a Requerida");
                return;
            }

            if (_islanderService.Password.Length < 6)
            {
                await CustomAlert.ShowErrorAsync("La contrase�a debe tener al menos 6 caracteres para mayor seguridad", "Contrase�a Muy Corta");
                return;
            }

            // Validate name length
            if (_islanderService.Name.Length < 3)
            {
                await CustomAlert.ShowErrorAsync("El nombre completo debe tener al menos 3 caracteres", "Nombre Muy Corto");
                return;
            }

            if (_islanderService.FirstName.Length < 2)
            {
                await CustomAlert.ShowErrorAsync("El primer nombre debe tener al menos 2 caracteres", "Primer Nombre Muy Corto");
                return;
            }

            if (_islanderService.LastName.Length < 2)
            {
                await CustomAlert.ShowErrorAsync("Los apellidos deben tener al menos 2 caracteres", "Apellidos Muy Cortos");
                return;
            }

            LoadingOverlay?.ShowLoading();
            await _islanderService.SaveIslanderDataAsync();

            if(_islanderService.ActivateClearForm)
            {
                ClearForm();
            }

        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error al registrar el islero:\n\n{ex.Message}", "Error del Sistema");
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

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // Basic email validation using regex
            var emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
            return emailRegex.IsMatch(email);
        }
        catch
        {
            return false;
        }
    }

    private void ClearForm()
    {
        if (_islanderService != null)
        {
            _islanderService.Name = string.Empty;
            _islanderService.FirstName = string.Empty;
            _islanderService.LastName = string.Empty;
            _islanderService.Email = string.Empty;
            _islanderService.Password = string.Empty;
            _islanderService.SelectedEds = null;
            _islanderService.SelectedRole = null;
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}