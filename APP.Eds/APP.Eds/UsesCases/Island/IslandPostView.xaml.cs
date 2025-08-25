using APP.Eds.Services.Island;

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
            await DisplayAlert("Error", $"Error al navegar: {ex.Message}", "OK");
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
                await DisplayAlert("Error", "Por favor, ingrese una Description", "OK");
                return;
            }

            await vm.SaveIslandDataAsync();
        }
        else
        {
            await DisplayAlert("Error", "Context error", "OK");
        }
    }
}