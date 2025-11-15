using APP.Eds.Models.Court;
using APP.Eds.Helpers;
using APP.Eds.Services.Config;
using System.Net.Http.Headers;
using System.Text.Json;

namespace APP.Eds.UsesCases.Court;

public partial class CourtDetailPage : ContentPage
{
    private readonly CourtListItemModel _court;
    private readonly string? _authToken;
    
    public CourtDetailPage(CourtListItemModel court)
    {
        InitializeComponent();
        _court = court;
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
        
        // Set initial binding context with list data
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
        
        // Load full court details from API to get observations
        await LoadFullCourtDetailsAsync();
        
        // Add a subtle entrance animation
        await AnimateEntryAsync();
    }

    private async Task LoadFullCourtDetailsAsync()
    {
        if (string.IsNullOrEmpty(_authToken) || _court == null)
        {
            return;
        }

        try
        {
            System.Diagnostics.Debug.WriteLine($"CourtDetailPage - Loading full details for court {_court.Id}");
            
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            
            // Call the API to get full court details including observations
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/court/{_court.Id}");
            
            System.Diagnostics.Debug.WriteLine($"CourtDetailPage - API Response: {response.Substring(0, Math.Min(200, response.Length))}...");
            
            // Deserialize the full court details
            var fullCourtDetails = JsonSerializer.Deserialize<CourtListItemModel>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            if (fullCourtDetails != null)
            {
                // Update the Descripcion property if it exists in the API response
                if (!string.IsNullOrWhiteSpace(fullCourtDetails.Descripcion))
                {
                    _court.Descripcion = fullCourtDetails.Descripcion;
                    System.Diagnostics.Debug.WriteLine($"CourtDetailPage - Observations loaded: {fullCourtDetails.Descripcion}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("CourtDetailPage - No observations found in API response");
                }
                
                // Refresh the binding context to update the UI
                BindingContext = null;
                BindingContext = _court;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CourtDetailPage - Error loading full court details: {ex.Message}");
            // Don't show error to user, just log it - the page will still work with list data
        }
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