# Version Check Service

## Overview

The Version Check Service provides automatic detection and notification of app updates available on Google Play Store. When a new version is published, users are notified and given the option to update immediately.

## Features

- **Automatic Version Detection**: Checks Google Play Store on app startup
- **Non-Blocking**: Version check runs asynchronously without blocking app launch
- **User-Friendly Notifications**: Uses the app's existing AlertService for consistent UI
- **Graceful Failure**: If version check fails, app continues normally without errors
- **Semantic Versioning**: Properly compares version numbers (e.g., 1.0.1 vs 1.0.2)
- **Direct Play Store Link**: One-tap access to update on Play Store

## Architecture

### Files

1. **IVersionCheckService.cs**: Interface defining the version check contract
2. **VersionCheckService.cs**: Implementation that fetches and compares versions
3. **Integration in App.xaml.cs**: Called on app startup via `OnStart()` method

### How It Works

1. **App Startup**: When the app starts, `CheckForUpdates()` is called
2. **Fetch Current Version**: Gets the installed version using `AppInfo.VersionString`
3. **Fetch Play Store Version**: Scrapes the Google Play Store page to extract version
4. **Compare Versions**: Uses semantic versioning logic to compare versions
5. **Notify User**: If newer version exists, shows a confirmation dialog
6. **Launch Play Store**: If user accepts, opens the app's Play Store page

## Implementation Details

### Version Parsing

The service uses multiple regex patterns to extract version numbers from the Play Store HTML:

```regex
Pattern 1: (?:Current Version|Versión actual)[^>]*>\s*<[^>]*>\s*<[^>]*>([0-9]+\.[0-9]+\.?[0-9]*)<
Pattern 2: \[\[\[\"([0-9]+\.[0-9]+\.?[0-9]*)\"\]\]
Pattern 3: softwareVersion['"]?\s*[:=]\s*['"]?([0-9]+\.[0-9]+\.?[0-9]*)
```

These patterns handle different Play Store layouts and language variants.

### Version Comparison

The `CompareVersions` method implements semantic versioning:

- Splits versions into parts (e.g., "1.0.1" → [1, 0, 1])
- Compares each part numerically from left to right
- Handles different length versions (e.g., "1.0" vs "1.0.1")

### Error Handling

The service handles various failure scenarios gracefully:

- **Network Timeout**: 10-second timeout prevents hanging
- **HTTP Errors**: Network issues are logged and return null
- **Parse Failures**: If version can't be extracted, no notification shown
- **Exception Safety**: All exceptions caught to prevent app crashes

## Testing

Unit tests are provided in `Mobile_tests/VersionCheckServiceTest.cs`:

- Version comparison logic tests
- Edge cases (same version, different lengths, major/minor differences)
- All 7 tests passing

To run tests:

```bash
cd APP.Eds
dotnet test Mobile_tests/Mobile_tests.csproj --filter "Category=VersionCheck"
```

## Configuration

The service is configured with:

- **Play Store URL**: `https://play.google.com/store/apps/details?id=com.companyname.app.EDS&hl=en`
- **Package ID**: `com.companyname.app.EDS`
- **HTTP Timeout**: 10 seconds
- **User Agent**: Mobile Android user agent for proper Play Store rendering

## Usage

The service is automatically invoked on app startup. No manual intervention required.

### Manual Usage (if needed)

```csharp
using var versionCheckService = new VersionCheckService();

// Check if update is available
bool updateAvailable = await versionCheckService.IsUpdateAvailableAsync();

// Get current version
string currentVersion = versionCheckService.GetCurrentVersion();

// Get latest Play Store version
string? latestVersion = await versionCheckService.GetLatestVersionAsync();
```

## Dependency Injection

The service is registered in `MauiProgram.cs`:

```csharp
builder.Services.AddSingleton<IVersionCheckService, VersionCheckService>();
```

## Localization

Currently, notification messages are in Spanish:

- Title: "Actualización Disponible"
- Message: "Hay una nueva versión ({version}) disponible..."
- Buttons: "Actualizar" / "Más tarde"

To localize, modify the strings in `App.xaml.cs` `CheckForUpdates()` method.

## Security Considerations

- **HTTPS Only**: All Play Store requests use HTTPS
- **No Sensitive Data**: No user data transmitted in version checks
- **Read-Only**: Service only reads from Play Store, never writes
- **Timeout Protection**: Prevents indefinite waiting on network issues

## Future Enhancements

Possible improvements:

1. **Caching**: Store last check time to avoid frequent checks
2. **Update Frequency Control**: Check only once per day
3. **Force Update**: Option to require update for critical versions
4. **Release Notes**: Show what's new in the update
5. **Background Checks**: Check periodically while app is running
6. **Localization**: Support multiple languages for notifications

## Troubleshooting

### Version Not Detected

If the Play Store version isn't detected:

1. Check network connectivity
2. Verify Play Store URL is accessible
3. Check if Play Store HTML structure changed
4. Review debug logs for parsing errors

### False Positives

If update notifications appear incorrectly:

1. Verify version format in Play Store matches app (e.g., "1.0.1")
2. Check `ApplicationDisplayVersion` in Poliedro.csproj
3. Ensure version comparison logic handles your version format

### No Notification

If no notification appears when update exists:

1. Check app is calling `CheckForUpdates()` on startup
2. Verify network permissions in AndroidManifest.xml
3. Check debug logs for errors
4. Ensure AlertService is functioning correctly
