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
            await _islanderService.GetIslandersAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al cargar isleros: {ex.Message}", "OK");
        }
        finally
        {
            LoadingOverlay?.HideLoading();
        }
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        if (BindingContext is not IslanderService vm)
        {
            await DisplayAlert("Error", "Error del sistema", "OK");
            return;
        }

        try
        {
            // Validation
            if (string.IsNullOrWhiteSpace(vm.Name))
            {
                await DisplayAlert("Error", "Por favor, ingrese un nombre", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(vm.FirstName))
            {
                await DisplayAlert("Error", "Por favor, ingrese el nombre de pila", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(vm.LastName))
            {
                await DisplayAlert("Error", "Por favor, ingrese el apellido", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(vm.Email))
            {
                await DisplayAlert("Error", "Por favor, ingrese un correo electrónico", "OK");
                return;
            }

            // Simple email validation
            if (!IsValidEmail(vm.Email))
            {
                await DisplayAlert("Error", "Por favor, ingrese un correo electrónico válido", "OK");
                return;
            }

            if (vm.SelectedEds is null)
            {
                await DisplayAlert("Error", "Por favor, seleccione una EDS", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(vm.Password))
            {
                await DisplayAlert("Error", "Por favor, ingrese una contraseña", "OK");
                return;
            }

            if (vm.Password.Length < 6)
            {
                await DisplayAlert("Error", "La contraseña debe tener al menos 6 caracteres", "OK");
                return;
            }

            // Show loading
            LoadingOverlay?.ShowLoading();
            
            // Save islander
            await vm.SaveIslanderDataAsync();
            
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
        }
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
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
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}