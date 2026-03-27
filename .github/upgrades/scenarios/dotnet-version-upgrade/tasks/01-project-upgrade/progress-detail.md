# Progress Detail: 01-project-upgrade

## Summary
All 4 projects successfully upgraded to net10.0. Build: ✅ 0 errors, 0 warnings.

## Files Modified
**Progress**: 1/3 tasks complete (33%) ![33%](https://progress-bar.xyz/33)
### ConsoleFramework/ConsoleFramework.csproj
- `TargetFrameworks` → `TargetFramework` = `net10.0` (dropped netstandard2.0, netstandard2.1, netcoreapp3.0)
- Removed `System.Threading.Thread` 4.3.0 package reference (built into .NET 10 — NuGet.0003)
- ✅ 01-project-upgrade: Upgrade all projects to net10.0
- 🔄 02-code-modernization: Modernize codebase to current C# standards

### Tests/Tests.csproj
- `TargetFramework` → `net10.0`
- `Microsoft.NET.Test.Sdk`: 15.3.0-preview → 17.13.0
- `xunit`: 2.2.0 → 2.9.3
- `xunit.runner.visualstudio`: 2.2.0 → 2.8.2 (stable v2 line; added PrivateAssets)
- Removed legacy `<Project>` and `<Name>` sub-elements from ProjectReference

### ExamplesStandalone/ManyControls/ManyControls.csproj
- `TargetFramework` → `net10.0`

### ExamplesStandalone/TextEditor/TextEditor.csproj
- `TargetFramework` → `net10.0`

## Issues Encountered

### DOTNETCORE define removed (fixed)
- **Problem**: Removed `DefineConstants` when cleaning legacy properties; `#if MONO` guard was activated causing CS0246 errors for `Mono.Unix` references.
- **Fix**: Restored `<DefineConstants>$(DefineConstants);DOTNETCORE</DefineConstants>` — this custom constant marks the build as targeting modern .NET (not Mono) and is used throughout the codebase for platform branching.

### Api.0002 (TimeSpan.FromMilliseconds) — No action required
- All 4 occurrences compiled cleanly on net10.0. The integer literals (300, 100, -1) resolved correctly to the `long` overload without ambiguity.

## Build Result
- 0 errors · 0 warnings
- All outputs: `bin\Debug\net10.0\`
