# AGENTS.md

## Overview
.NET MAUI cross-platform app (Android, iOS, macOS, Windows) for gas station management. The UI and codebase are in **Spanish**.

## Build & SDK
- **SDK**: .NET 10.0.100 (pinned in `global.json` with `rollForward: latestFeature`)
- **Build**: `dotnet build APP.Eds/Poliedro.sln`
- **NuGet**: Standard nuget.org only (`nuget.config`)

## Solution Structure
```
APP.Eds/
  Poliedro.sln
  APP.Eds/Poliedro.csproj         ← Main MAUI app
  Helpers/Helpers.csproj          ← Appium drivers + UI element wrappers
  Mobile_views/Mobile_views.csproj ← Page Object Model views
  Mobile_tests/Mobile_tests.csproj ← NUnit 4 + Appium tests
```

## Architecture
- **MVVM-lite**: Views in `UsesCases/{Module}/`, services in `Services/{Module}/`, models in `Models/{Module}/`
- **DI** in `MauiProgram.cs` (AddSingleton for services)
- **Key packages**: `CommunityToolkit.Maui` 9.x, `CommunityToolkit.Mvvm` 8.x, `Newtonsoft.Json`
- **API**: AWS API Gateway (us-east-2), JWT-based auth (Keycloak/OAuth2)
- **App entry**: `App.xaml.cs` — clears session on startup, sets `MainPage = new NavigationPage(new MainPage())` (login page). Theme follows system with `AppTheme.Unspecified`

## Testing
- Tests are **NUnit 4 + Appium 8** (mobile UI tests), target `net10.0-android` only
- **Prerequisite**: Appium server running at `http://127.0.0.1:4723` + Android emulator with the app installed
- **Test config**: `APP.Eds/Mobile_tests/configuration.json`
- Base test auto-logs in with `admin` / `admin` credentials
- Run all: `dotnet test APP.Eds/Mobile_tests/Mobile_tests.csproj`
- Run category: `dotnet test APP.Eds/Mobile_tests/Mobile_tests.csproj --filter "Category=VersionCheck"`

## Important API Config
`APP.Eds/Services/Config/Configuration.cs` contains API URLs, tokens, and environment flags. The billing API token is hardcoded — **do not expose in logs, commits, or docs**.

## Encoding
UTF-8 with BOM preferred (VS default). Accented Spanish characters (`áéíóúñ`) were previously corrupted to `�` in several files. When saving `.cs` or `.xaml` files, ensure UTF-8 encoding is preserved — re-opening corrupted files in editors that misinterpret encoding will reintroduce the bug. See `ENCODING_GUIDELINES.md` for the full list of affected files and fix procedures.

## Feature Flags
`Configuration.EnableAutoVersionCheck` is currently `false`. Version check is disabled on startup until testing completes — do not enable without coordination.

## Linting & Formatting
No automated linting or formatting is configured (no `.editorconfig`, ESLint, StyleCop). SonarCloud analysis is available via `APP.Eds/run-sonar.bat` but runs as a manual one-off.
