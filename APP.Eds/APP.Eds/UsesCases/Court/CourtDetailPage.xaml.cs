using APP.Eds.Models.Court;

namespace APP.Eds.UsesCases.Court;

public partial class CourtDetailPage : ContentPage
{
    public CourtDetailPage(CourtListItemModel court)
    {
        InitializeComponent();
        BindingContext = court;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Add a subtle entrance animation
        await AnimateEntryAsync();
    }

    private async Task AnimateEntryAsync()
    {
        try
        {
            // Start with slight transparency and scale
            MainScroll.Opacity = 0;
            MainScroll.Scale = 0.95;
            
            // Animate entry with smooth effect
            await Task.WhenAll(
                MainScroll.FadeTo(1, 400, Easing.CubicOut),
                MainScroll.ScaleTo(1, 400, Easing.CubicOut)
            );
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error animating court detail page entry: {ex.Message}");
        }
    }
}