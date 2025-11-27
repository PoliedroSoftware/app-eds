namespace APP.Eds.Services.VersionCheck;

/// <summary>
/// Service interface for checking app version updates from Google Play Store.
/// This service provides functionality to compare the installed app version
/// with the version available on the Play Store and notify users of updates.
/// </summary>
public interface IVersionCheckService
{
    /// <summary>
    /// Checks if a new version is available on Google Play Store by comparing
    /// the current installed version with the latest available version.
    /// </summary>
    /// <returns>
    /// True if an update is available (Play Store version is newer),
    /// false otherwise or if unable to determine.
    /// </returns>
    Task<bool> IsUpdateAvailableAsync();

    /// <summary>
    /// Gets the current version installed on the device.
    /// </summary>
    /// <returns>The current version string in format "X.Y.Z" (e.g., "1.0.1")</returns>
    string GetCurrentVersion();

    /// <summary>
    /// Gets the latest version available on Google Play Store by parsing
    /// the Play Store web page.
    /// </summary>
    /// <returns>
    /// The latest version string in format "X.Y.Z" (e.g., "1.0.2"),
    /// or null if unable to fetch or parse the version.
    /// </returns>
    Task<string?> GetLatestVersionAsync();
}
