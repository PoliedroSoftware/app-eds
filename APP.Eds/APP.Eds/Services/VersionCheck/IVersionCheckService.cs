namespace APP.Eds.Services.VersionCheck;

/// <summary>
/// Service interface for checking app version updates
/// </summary>
public interface IVersionCheckService
{
    /// <summary>
    /// Checks if a new version is available on Google Play Store
    /// </summary>
    /// <returns>True if an update is available, false otherwise</returns>
    Task<bool> IsUpdateAvailableAsync();

    /// <summary>
    /// Gets the current version installed on the device
    /// </summary>
    /// <returns>The current version string</returns>
    string GetCurrentVersion();

    /// <summary>
    /// Gets the latest version available on Google Play Store
    /// </summary>
    /// <returns>The latest version string, or null if unable to fetch</returns>
    Task<string?> GetLatestVersionAsync();
}
