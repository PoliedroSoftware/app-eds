# Manual Testing Guide: Version Update Notification Feature

## Overview
This guide provides instructions for manually testing the version update notification feature.

**Important:** The feature is **disabled by default** (controlled by `Configuration.EnableAutoVersionCheck = false`). This allows safe testing before production rollout.

## Prerequisites
- Android device or emulator
- App installed with current version (1.0.1)
- Internet connection
- Access to publish a new version to Google Play Store (for full testing)

## Testing Mode

### Option 1: Manual Testing (Recommended for Initial Testing)

**Current Configuration:** Feature is disabled by default.

**How to Test:**
1. Build and run the app normally
2. The version check will NOT run automatically
3. To test the feature, manually trigger it from code or debug console:
   ```csharp
   ((App)Application.Current).CheckForUpdates();
   ```
4. Or add a test button in your UI that calls this method

**Benefits:**
- Test without affecting normal app usage
- Control exactly when the check happens
- Easy to debug and verify behavior

### Option 2: Automatic Testing (After Manual Testing Successful)

**To Enable Automatic Checks:**
1. Open `APP.Eds/Services/Config/Configuration.cs`
2. Change: `public static bool EnableAutoVersionCheck => true;`
3. Rebuild and redeploy the app
4. Version check will now run automatically on every app startup

## Test Scenarios

### Scenario 1: No Update Available (Up to Date)

**Setup:**
- App version matches Play Store version

**Steps (Manual Testing):**
1. Launch the app
2. Call `((App)Application.Current).CheckForUpdates();` from debug console or test button
3. Wait a few seconds
4. Observe the version information dialog

**Steps (Automatic Testing - if enabled):**
1. Launch the app
2. Wait for version check to complete
3. Observe the version information dialog

**Expected Result:**
- Dialog appears with title "Información de Versión"
- Message shows: "Versión actual: X.X.X\n\nTienes la última versión disponible en Google Play Store."
- One button: "OK"
- App continues normally after closing dialog

**Verification:**
- Current version is displayed
- User is informed they have the latest version

---

### Scenario 2: Update Available (Happy Path)

**Setup:**
- Play Store has version 1.0.2 published
- App has version 1.0.1 installed

**Steps (Manual Testing):**
1. Launch the app
2. Call `((App)Application.Current).CheckForUpdates();` from debug console or test button
3. Wait for version check to complete (should be within seconds)
4. Observe the update notification dialog

**Steps (Automatic Testing - if enabled):**
1. Launch the app
2. Wait for version check to complete (should be within seconds)
3. Observe the update notification dialog

**Expected Result:**
- Dialog appears with title "Actualización Disponible"
- Message shows: "Versión actual: 1.0.1\n\nHay una nueva versión (1.0.2) disponible en Google Play Store. ¿Deseas actualizar ahora?"
- Two buttons: "Actualizar" and "Más tarde"

**Test Path A - User Accepts Update:**
1. Tap "Actualizar" button
2. Verify Play Store opens with app page
3. User can choose to update from Play Store

**Test Path B - User Declines Update:**
1. Tap "Más tarde" button
2. Dialog closes
3. App continues to main screen

---

### Scenario 3: Network Failure

**Setup:**
- Disable device internet connection or use airplane mode

**Steps (Manual Testing):**
1. Enable airplane mode
2. Launch the app
3. Call `((App)Application.Current).CheckForUpdates();`
4. Wait a few seconds
5. Observe the version information dialog

**Steps (Automatic Testing - if enabled):**
1. Enable airplane mode
2. Launch the app
3. Wait for version check to complete
4. Observe the version information dialog

**Expected Result:**
- App launches normally without delays
- Dialog appears with title "Información de Versión"
- Message shows: "Versión actual: X.X.X\n\nNo se pudo verificar actualizaciones en este momento." OR "No se pudo verificar la versión en Google Play Store."
- Current version is always displayed even when network fails
- Debug log shows: "Network error fetching version" or "Error checking for updates"

**Verification:**
- App should not hang or crash
- Current version is still shown to user despite network error
- No user-facing error dialogs
- Startup time not significantly affected

---

### Scenario 4: Play Store Unavailable

**Setup:**
- Block access to play.google.com (e.g., via hosts file or DNS)

**Steps:**
1. Block play.google.com
2. Launch the app
3. Observe behavior

**Expected Result:**
- App launches normally
- 10-second timeout occurs silently
- No update notification appears
- Debug log shows timeout error

---

### Scenario 5: Rapid App Restarts

**Setup:**
- Normal network conditions

**Steps:**
1. Launch the app
2. Immediately force close the app
3. Launch the app again
4. Repeat 5 times

**Expected Result:**
- Version check runs each time
- No crashes or memory issues
- HttpClient properly disposed between launches
- App remains responsive

---

### Scenario 6: Background to Foreground

**Setup:**
- App is running

**Steps:**
1. Launch the app
2. Press home button (send to background)
3. Wait 30 seconds
4. Bring app back to foreground

**Expected Result:**
- No duplicate version check notifications
- Version check only happens on initial app start (OnStart)
- App resumes normally

---

## Debug Verification

### Enable Debug Output

Check debug logs for these messages:

**Success Messages:**
- "Version check completed successfully"
- "Current version: X.X.X, Latest version: Y.Y.Y"

**Info Messages:**
- "No update available"
- "Version pattern not found in Play Store response"

**Error Messages:**
- "Network error fetching version: ..."
- "Version check timed out"
- "Error checking for updates: ..."

### Android Studio Logcat Filter

Use this filter to see version check logs:
```
tag:VersionCheck
```

Or search for:
```
System.Diagnostics.Debug
```

---

## Performance Testing

### Startup Time Impact

**Objective:** Verify version check doesn't significantly delay app startup

**Steps:**
1. Time app startup without version check (baseline)
2. Time app startup with version check
3. Compare times

**Expected Result:**
- Startup time increase < 1 second
- UI remains responsive during check
- No blocking of main thread

**Tools:**
- Stopwatch
- Android Studio Profiler
- Frame timing analysis

---

## Unit Test Verification

Run the automated tests:

```bash
cd APP.Eds
dotnet test Mobile_tests/Mobile_tests.csproj --filter "Category=VersionCheck"
```

**Expected Result:**
```
Passed!  - Failed: 0, Passed: 7, Skipped: 0, Total: 7
```

**Tests:**
1. CompareVersions_SameVersions_ReturnsZero ✓
2. CompareVersions_FirstVersionLower_ReturnsNegative ✓
3. CompareVersions_FirstVersionHigher_ReturnsPositive ✓
4. CompareVersions_MajorVersionDifferent_ReturnsCorrectResult ✓
5. CompareVersions_MinorVersionDifferent_ReturnsCorrectResult ✓
6. CompareVersions_DifferentLengths_ComparesCorrectly ✓
7. CompareVersions_DifferentLengthsSameValue_ReturnsZero ✓

---

## Regression Testing

Verify these existing features still work:

- [ ] User login
- [ ] Theme switching (Light/Dark mode)
- [ ] Navigation between screens
- [ ] Data entry forms
- [ ] Business operations
- [ ] Network requests to backend API

---

## Edge Cases

### Play Store HTML Changes

**Risk:** Google may change Play Store HTML structure

**Mitigation:**
- Service uses multiple regex patterns
- Graceful fallback if parsing fails
- No user impact on failure

**Test:**
- Manually check if service can parse current Play Store page
- Review debug logs for parsing warnings

### Version Format Variations

**Test Cases:**
- Version "1.0" vs "1.0.0" → Should be equal
- Version "2.0.0" vs "2.0" → Should be equal
- Version "1.0.1" vs "1.1.0" → 1.1.0 is newer
- Version "2.0.0" vs "1.9.9" → 2.0.0 is newer

**Verification:**
- Run unit tests
- Check CompareVersions logic

---

## Security Considerations

### Verify:
- [x] Only HTTPS used for Play Store requests
- [x] No sensitive data transmitted
- [x] No user credentials involved
- [x] Timeout prevents indefinite waiting
- [x] CodeQL security scan passed (0 vulnerabilities)

---

## Rollback Plan

If issues are found in production:

1. **Quick Fix:**
   - Comment out `CheckForUpdates()` call in App.xaml.cs OnStart()
   - Redeploy

2. **Revert:**
   - `git revert <commit-hash>`
   - Rebuild and redeploy

3. **Disable Feature:**
   - Add feature flag to Configuration
   - Check flag before calling CheckForUpdates()

---

## Success Criteria

The feature is considered successful if:

✅ All automated tests pass  
✅ No crashes or errors during version check  
✅ App startup not noticeably delayed  
✅ Users receive clear, actionable update notifications  
✅ Network failures handled gracefully  
✅ Play Store link opens correctly  
✅ No security vulnerabilities introduced  
✅ Existing app features unaffected  

---

## Known Limitations

1. **Play Store Scraping:** Uses web scraping, not official API
   - May break if Google changes HTML structure
   - Multiple patterns provide resilience

2. **Check Frequency:** Only checks on app start
   - Consider adding periodic checks in future

3. **Language:** Notification text is Spanish only
   - Localization can be added later

4. **No Force Update:** Users can skip updates
   - Add critical update logic if needed

---

## Support Information

For issues or questions:
- Check README.md in Services/VersionCheck/
- Review debug logs
- Contact development team
