using APP.Eds.Models.Court;

namespace APP.Eds.UsesCases.Court;

public partial class CourtDetailPage : ContentPage
{
    private readonly CourtListItemModel _court;
    
    public CourtDetailPage(CourtListItemModel court)
    {
        InitializeComponent();
        _court = court;
        BindingContext = court;
        
        // Debug information about Collections
        LogCourtData(court);
    }

    private void LogCourtData(CourtListItemModel court)
    {
        if (court != null)
        {
            System.Diagnostics.Debug.WriteLine($"CourtDetailPage - Court ID: {court.Id}");
            System.Diagnostics.Debug.WriteLine($"CourtDetailPage - TotalAccumulatedAmount: {court.TotalAccumulatedAmount:C}");
            System.Diagnostics.Debug.WriteLine($"CourtDetailPage - Collections count: {court.Collections?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine($"CourtDetailPage - Dispensers count: {court.Dispensers?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine($"CourtDetailPage - Expenditures count: {court.Expenditures?.Count ?? 0}");
            
            // Verificar si hay Collections
            if (court.Collections != null && court.Collections.Any())
            {
                System.Diagnostics.Debug.WriteLine("? Collections data found - showing payment methods:");
                foreach (var collection in court.Collections)
                {
                    System.Diagnostics.Debug.WriteLine($"  - {collection.Collection}: ${collection.Amount:C}");
                }
                
                // Validar consistencia de totales
                ValidateCollectionsTotal(court);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("?? No Collections data - showing 'No hay medios de pago registrados' message");
            }
        }
    }

    private void ValidateCollectionsTotal(CourtListItemModel court)
    {
        if (court.Collections == null || !court.Collections.Any()) return;

        var totalFromCollections = court.Collections.Sum(c => c.Amount);
        var difference = Math.Abs(totalFromCollections - court.TotalAccumulatedAmount);
        
        System.Diagnostics.Debug.WriteLine($"CourtDetailPage - Total from Collections: ${totalFromCollections:C}");
        System.Diagnostics.Debug.WriteLine($"CourtDetailPage - TotalAccumulatedAmount: ${court.TotalAccumulatedAmount:C}");
        
        if (difference > 0.01) // Permitir peque�as diferencias de redondeo
        {
            System.Diagnostics.Debug.WriteLine($"?? WARNING: Payment methods total (${totalFromCollections:C}) doesn't match accumulated amount (${court.TotalAccumulatedAmount:C})");
            System.Diagnostics.Debug.WriteLine($"   Difference: ${difference:C}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("? Payment methods total matches accumulated amount");
        }
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