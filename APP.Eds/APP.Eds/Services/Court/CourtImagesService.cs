using APP.Eds.Helpers;
using APP.Eds.Services.Config;
using System.Net.Http.Headers;
using System.Text.Json;

namespace APP.Eds.Services.Court;

/// <summary>
/// Service to fetch court images from the backend API
/// </summary>
public class CourtImagesService
{
    private readonly string? _authToken;

    public CourtImagesService()
    {
        _authToken = TokenHelper.LoadToken();
    }

    /// <summary>
    /// Gets the list of image URLs for a specific court
    /// </summary>
    /// <param name="courtId">The ID of the court</param>
    /// <returns>List of image URLs or empty list if none found</returns>
    public async Task<List<string>> GetCourtImagesAsync(int courtId)
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            System.Diagnostics.Debug.WriteLine("CourtImagesService.GetCourtImagesAsync: No authentication token found");
            return new List<string>();
        }

        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            System.Diagnostics.Debug.WriteLine($"CourtImagesService.GetCourtImagesAsync: Fetching images for court {courtId}");

            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/files/court/{courtId}");

            if (string.IsNullOrEmpty(response))
            {
                System.Diagnostics.Debug.WriteLine("CourtImagesService.GetCourtImagesAsync: Empty response received");
                return new List<string>();
            }

            System.Diagnostics.Debug.WriteLine($"CourtImagesService.GetCourtImagesAsync: Response received: {response.Substring(0, Math.Min(200, response.Length))}...");

            var result = JsonSerializer.Deserialize<CourtImagesResponse>(response, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });

            if (result?.Images != null && result.Images.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"CourtImagesService.GetCourtImagesAsync: Found {result.Images.Count} images");
                return result.Images;
            }

            System.Diagnostics.Debug.WriteLine("CourtImagesService.GetCourtImagesAsync: No images found");
            return new List<string>();
        }
        catch (HttpRequestException httpEx)
        {
            System.Diagnostics.Debug.WriteLine($"CourtImagesService.GetCourtImagesAsync: HTTP error: {httpEx.Message}");
            return new List<string>();
        }
        catch (TaskCanceledException)
        {
            System.Diagnostics.Debug.WriteLine("CourtImagesService.GetCourtImagesAsync: Request timeout");
            return new List<string>();
        }
        catch (JsonException jsonEx)
        {
            System.Diagnostics.Debug.WriteLine($"CourtImagesService.GetCourtImagesAsync: JSON parsing error: {jsonEx.Message}");
            return new List<string>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CourtImagesService.GetCourtImagesAsync: Unexpected error: {ex.Message}");
            return new List<string>();
        }
    }

    /// <summary>
    /// Response model for the court images API endpoint
    /// </summary>
    private class CourtImagesResponse
    {
        public List<string>? Images { get; set; }
    }
}
