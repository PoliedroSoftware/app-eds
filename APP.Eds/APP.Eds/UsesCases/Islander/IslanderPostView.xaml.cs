using APP.Eds.Services.Islander;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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
            if (string.IsNullOrWhiteSpace(_islanderService.Name))
            {
                await DisplayAlert("Error", "Por favor, ingrese el nombre completo del islero", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(_islanderService.FirstName))
            {
                await DisplayAlert("Error", "Por favor, ingrese el primer nombre", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(_islanderService.LastName))
            {
                await DisplayAlert("Error", "Por favor, ingrese los apellidos", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(_islanderService.Email))
            {
                await DisplayAlert("Error", "Por favor, ingrese el correo electronico", "OK");
                return;
            }

            // Enhanced email validation
            if (!IsValidEmail(_islanderService.Email))
            {
                await DisplayAlert("Error", "Por favor, ingrese un correo electronico valido", "OK");
                return;
            }

            if (_islanderService.SelectedEds == null)
            {
                await DisplayAlert("Error", "Por favor, seleccione la estacion de servicio (EDS) asignada", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(_islanderService.SelectedRole))
            {
                await DisplayAlert("Error", "Por favor, seleccione el rol o posicion del islero", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(_islanderService.Password))
            {
                await DisplayAlert("Error", "Por favor, ingrese una contraseña de acceso", "OK");
                return;
            }

            if (_islanderService.Password.Length < 6)
            {
                await DisplayAlert("Error", "La contraseña debe tener al menos 6 caracteres", "OK");
                return;
            }

            // Validate password strength
            if (!IsStrongPassword(_islanderService.Password))
            {
                var result = await DisplayAlert("Contraseña Debil", 
                    "La contraseña es débil. Se recomienda usar letras, números y caracteres especiales.\n¿Desea continuar de todas formas?", 
                    "Continuar", "Cancelar");
                if (!result) return;
            }

            if (!string.IsNullOrWhiteSpace(_islanderService.PhoneNumber))
            {
                if (!IsValidPhoneNumber(_islanderService.PhoneNumber))
                {
                    await DisplayAlert("Error", "Por favor, ingrese un numero de telefono valido", "OK");
                    return;
                }
            }

            // Show loading
            LoadingOverlay?.ShowLoading();
            
            // Save islander
            await _islanderService.SaveIslanderDataAsync();
            
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

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email && email.Contains("@") && email.Contains(".");

        }
        catch
        {
            return false;
        }
    }

    private bool IsStrongPassword(string password)
    {
        if (password.Length < 8) return false;
        
        bool hasUpper = password.Any(char.IsUpper);
        bool hasLower = password.Any(char.IsLower);
        bool hasDigit = password.Any(char.IsDigit);
        bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));
        
        return (hasUpper && hasLower && hasDigit) || (hasUpper && hasLower && hasSpecial) || (hasDigit && hasSpecial);
    }

    private bool IsValidPhoneNumber(string phone)
    {
        // Remove common separators
        string cleanPhone = phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("+", "");
        
        // Check if all remaining characters are digits and length is reasonable
        return cleanPhone.All(char.IsDigit) && cleanPhone.Length >= 7 && cleanPhone.Length <= 15;
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
            _islanderService.PhoneNumber = string.Empty;
            _islanderService.SelectedEds = null;
            _islanderService.SelectedRole = null;
            _islanderService.IsActive = true; // Default to active
            _islanderService.CanManageDispensers = false;
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}