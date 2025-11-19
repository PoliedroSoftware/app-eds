using System.Text.RegularExpressions;

namespace APP.Eds.Services.VersionCheck;

/// <summary>
/// Service for checking app version updates from Google Play Store.
/// Implements IDisposable to properly dispose of HttpClient resources.
/// </summary>
public class VersionCheckService : IVersionCheckService, IDisposable
{
    private const string PlayStoreUrl = "https://play.google.com/store/apps/details?id=com.companyname.app.EDS&hl=en";
    private readonly HttpClient _httpClient;
    private bool _disposed;

    public VersionCheckService()
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10) // Set timeout to prevent hanging
        };
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Linux; Android 10) AppleWebKit/537.36");
    }

    /// <summary>
    /// Gets the current version installed on the device
    /// </summary>
    public string GetCurrentVersion()
    {
        return AppInfo.VersionString;
    }

    /// <summary>
    /// Gets the latest version available on Google Play Store
    /// </summary>
    /// <returns>The version string from Play Store, or null if unable to fetch</returns>
    public async Task<string?> GetLatestVersionAsync()
    {
        try
        {
            var response = await _httpClient.GetStringAsync(PlayStoreUrl);
            
            // Try multiple regex patterns to handle different Play Store layouts
            // Pattern 1: Standard layout - "Current Version</div><span...>1.0.1</span>"
            // Pattern 2: Spanish layout - "Versión actual</div><span...>1.0.1</span>"
            var versionPattern = @"(?:Current Version|Versión actual)[^>]*>\s*<[^>]*>\s*<[^>]*>([0-9]+\.[0-9]+\.?[0-9]*)<";
            var match = Regex.Match(response, versionPattern, RegexOptions.IgnoreCase);
            
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }

            // Pattern 3: JSON-like structure in Play Store API responses
            versionPattern = @"\[\[\[""([0-9]+\.[0-9]+\.?[0-9]*)""\]\]";
            match = Regex.Match(response, versionPattern);
            
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }

            // Pattern 4: More lenient search for version numbers in the page
            versionPattern = @"softwareVersion['""]?\s*[:=]\s*['""]?([0-9]+\.[0-9]+\.?[0-9]*)";
            match = Regex.Match(response, versionPattern, RegexOptions.IgnoreCase);
            
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }

            System.Diagnostics.Debug.WriteLine("Version pattern not found in Play Store response");
            return null;
        }
        catch (TaskCanceledException)
        {
            System.Diagnostics.Debug.WriteLine("Version check timed out");
            return null;
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Network error fetching version: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching latest version: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Checks if a new version is available on Google Play Store
    /// </summary>
    public async Task<bool> IsUpdateAvailableAsync()
    {
        try
        {
            var currentVersion = GetCurrentVersion();
            var latestVersion = await GetLatestVersionAsync();

            if (string.IsNullOrEmpty(latestVersion))
            {
                return false;
            }

            return CompareVersions(currentVersion, latestVersion) < 0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error checking for updates: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Compares two version strings
    /// </summary>
    /// <returns>
    /// -1 if version1 is less than version2,
    /// 0 if they are equal,
    /// 1 if version1 is greater than version2
    /// </returns>
    private int CompareVersions(string version1, string version2)
    {
        var v1Parts = version1.Split('.').Select(int.Parse).ToArray();
        var v2Parts = version2.Split('.').Select(int.Parse).ToArray();

        int maxLength = Math.Max(v1Parts.Length, v2Parts.Length);
        
        for (int i = 0; i < maxLength; i++)
        {
            int v1Part = i < v1Parts.Length ? v1Parts[i] : 0;
            int v2Part = i < v2Parts.Length ? v2Parts[i] : 0;

            if (v1Part < v2Part) return -1;
            if (v1Part > v2Part) return 1;
        }

        return 0;
    }

    /// <summary>
    /// Disposes of the HttpClient resources
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Protected implementation of Dispose pattern
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _httpClient?.Dispose();
            }
            _disposed = true;
        }
    }
}
