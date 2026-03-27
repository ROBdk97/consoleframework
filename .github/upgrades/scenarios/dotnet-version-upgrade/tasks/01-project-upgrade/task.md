# Task: 01-project-upgrade

## Objective
Upgrade all 4 projects to net10.0, fix breaking changes, validate build.

## Research Findings

### Project Changes Required

| Project | Current TFM | Target TFM | Notes |
|---------|-------------|------------|-------|
| ConsoleFramework | netstandard2.0;netstandard2.1;netcoreapp3.0 | net10.0 | Single-target |
| Tests | netcoreapp3.0 | net10.0 | Update test packages |
| ManyControls | netcoreapp3.0 | net10.0 | No packages to change |
| TextEditor | netcoreapp3.0 | net10.0 | No packages to change |

### Package Changes

**ConsoleFramework.csproj**:
- REMOVE `System.Threading.Thread` 4.3.0 (NuGet.0003: built into .NET 10)
- REMOVE `RuntimeFrameworkVersion` (legacy .NET Core 1.x property, not applicable)

**Tests.csproj**:
- `Microsoft.NET.Test.Sdk`: 15.3.0-preview → 17.13.0
- `xunit`: 2.2.0 → 2.9.3
- `xunit.runner.visualstudio`: 2.2.0 → 2.8.2 (stable v2; compatible with xunit 2.9.x)
- Clean legacy ProjectReference sub-elements (`<Project>`, `<Name>`)

### Api.0002: TimeSpan.FromMilliseconds
- 4 occurrences in EventManager.cs (x2), ButtonBase.cs, ConsoleApplication.cs
- Flagged as "Potential" — will build first and fix actual errors
