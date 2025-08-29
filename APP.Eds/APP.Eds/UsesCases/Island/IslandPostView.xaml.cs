using APP.Eds.Services.Island;
using APP.Eds.Components.PopUp;

namespace APP.Eds.UsesCases.Island;

public partial class IslandPostView : ContentPage
{
    private IslandService _islandService;
    
    public IslandPostView()
    {
        InitializeComponent();
        _islandService = new IslandService();
        BindingContext = _islandService;
    }

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        try
        {
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await CustomAlert.ShowErrorAsync($"Error al navegar hacia atrás:\n\n{ex.Message}", "Error de Navegación");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Refresh islands list when page appears
        if (_islandService != null)
        {
            await _islandService.GetIslandAsync();
        }
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        if (BindingContext is IslandService vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Description))
            {
                await CustomAlert.ShowErrorAsync("La descripción de la isla es obligatoria para el registro", "Descripción Requerida");
                return;
            }

            if (vm.Description.Length < 3)
            {
                await CustomAlert.ShowErrorAsync("La descripción debe tener al menos 3 caracteres", "Descripción Muy Corta");
                return;
            }

            if (vm.Description.Length > 200)
            {
                await CustomAlert.ShowErrorAsync("La descripción no puede exceder los 200 caracteres", "Descripción Muy Larga");
                return;
            }

            try
            {
                await vm.SaveIslandDataAsync();
            }
            catch (Exception ex)
            {
                await CustomAlert.ShowErrorAsync($"Error al guardar la isla:\n\n{ex.Message}", "Error del Sistema");
            }
        }
        else
        {
            await CustomAlert.ShowErrorAsync("Error interno del sistema. Por favor, intente nuevamente", "Error de Contexto");
        }
    }
}