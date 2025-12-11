using APP.Eds.Models.Court;
using APP.Eds.Helpers;
using APP.Eds.Services.Config;
using APP.Eds.Services.Court;
using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Windows.Input;

namespace APP.Eds.UsesCases.Court;

public partial class CourtDetailPage : ContentPage
{
    private readonly CourtListItemModel _court;
    private readonly string? _authToken;

    /// <summary>
    /// Collection of court image URLs for display
    /// </summary>
    public ObservableCollection<string> CourtImages { get; set; } = new();

    /// <summary>
    /// Command to open an image in the browser
    /// </summary>
    public ICommand OpenImageCommand { get; }
    
    public CourtDetailPage(CourtListItemModel court)
    {
        InitializeComponent();
        _court = court;
        _authToken = TokenHelper.LoadToken();

        // Initialize the command for opening images
        OpenImageCommand = new Command<string>(async (imageUrl) => await OpenImageAsync(imageUrl));
        
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
        
        if (difference > 0.01) // Permitir pequeñas diferencias de redondeo
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

        // Load court images
        await LoadCourtImagesAsync();
        
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

    /// <summary>
    /// Loads court images from the backend API
    /// </summary>
    private async Task LoadCourtImagesAsync()
    {
        if (_court == null)
        {
            return;
        }

        try
        {
            ImagesLoadingIndicator.IsVisible = true;
            ImagesLoadingIndicator.IsRunning = true;

            var imagesService = new CourtImagesService();
            var images = await imagesService.GetCourtImagesAsync(_court.Id);

            CourtImages.Clear();
            if (images != null && images.Any())
            {
                foreach (var imageUrl in images)
                {
                    CourtImages.Add(imageUrl);
                }
                ImagesCollectionView.IsVisible = true;
                NoImagesMessage.IsVisible = false;
            }
            else
            {
                ImagesCollectionView.IsVisible = false;
                NoImagesMessage.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading court images: {ex.Message}");
            ImagesCollectionView.IsVisible = false;
            NoImagesMessage.IsVisible = true;
        }
        finally
        {
            ImagesLoadingIndicator.IsVisible = false;
            ImagesLoadingIndicator.IsRunning = false;
        }
    }

    /// <summary>
    /// Opens an image in full screen using the system browser
    /// </summary>
    private async Task OpenImageAsync(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl))
            return;

        try
        {
            // Open image in full screen using Browser or custom viewer
            await Browser.Default.OpenAsync(imageUrl, BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error opening image: {ex.Message}");
            await DisplayAlert("Error", "No se pudo abrir la imagen", "OK");
        }
    }
}