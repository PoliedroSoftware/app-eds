# .NET 9 Upgrade Plan

## Execution Steps

Execute steps below sequentially one by one in the order they are listed.

1. Validate that a .NET 9 SDK required for this upgrade is installed on the machine and if not, help to get it installed.
2. Ensure that the SDK version specified in global.json files is compatible with the .NET 9 upgrade.
3. Upgrade Helpers\Helpers.csproj
4. Upgrade Mobile_views\Mobile_views.csproj
5. Upgrade Mobile_tests\Mobile_tests.csproj
6. Upgrade APP.Eds\Poliedro.csproj

## Settings

This section contains settings and data used by execution steps.

### Aggregate NuGet packages modifications across all projects

NuGet packages used across all selected projects or their dependencies that need version update in projects that reference them.

| Package Name                         | Current Version | New Version | Description                                   |
|:-------------------------------------|:---------------:|:-----------:|:----------------------------------------------|
| Microsoft.Extensions.Configuration.Json | 8.0.1         | 9.0.10      | Recommended for .NET 9                        |
| Microsoft.Extensions.Logging.Debug  | 8.0.1           | 9.0.10      | Recommended for .NET 9                        |
| Microsoft.NET.ILLink.Tasks          | 8.0.15          | 9.0.10      | Recommended for .NET 9                        |
| Newtonsoft.Json                     | 13.0.3          | 13.0.4      | Recommended for .NET 9                        |

### Project upgrade details
This section contains details about each project upgrade and modifications that need to be done in the project.

#### Helpers\Helpers.csproj modifications

Project properties changes:
  - Target framework should be changed from `net8.0` to `net9.0`

NuGet packages changes:
  - Microsoft.Extensions.Configuration.Json should be updated from `8.0.1` to `9.0.10` (*recommended for .NET 9*)
  - Microsoft.NET.ILLink.Tasks should be updated from `8.0.15` to `9.0.10` (*recommended for .NET 9*)

#### Mobile_views\Mobile_views.csproj modifications

Project properties changes:
  - Target framework should be changed from `net8.0` to `net9.0`

NuGet packages changes:
  - Microsoft.NET.ILLink.Tasks should be updated from `8.0.15` to `9.0.10` (*recommended for .NET 9*)

#### Mobile_tests\Mobile_tests.csproj modifications

Project properties changes:
  - Target framework should be changed from `net8.0` to `net9.0`

NuGet packages changes:
  - Microsoft.NET.ILLink.Tasks should be updated from `8.0.15` to `9.0.10` (*recommended for .NET 9*)

#### APP.Eds\Poliedro.csproj modifications

Project properties changes:
  - Target frameworks should be changed from `net8.0-android;net8.0-ios;net8.0-maccatalyst;net8.0-windows10.0.19041.0` to `net9.0-android;net9.0-ios;net9.0-maccatalyst;net9.0-windows10.0.19041.0`

NuGet packages changes:
  - Microsoft.Extensions.Logging.Debug should be updated from `8.0.1` to `9.0.10` (*recommended for .NET 9*)
  - Newtonsoft.Json should be updated from `13.0.3` to `13.0.4` (*recommended for .NET 9*)