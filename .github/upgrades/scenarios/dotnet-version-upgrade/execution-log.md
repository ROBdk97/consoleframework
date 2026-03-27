
## [2026-03-27 09:51] 01-project-upgrade

Upgraded all 4 projects to net10.0. Removed System.Threading.Thread 4.3.0 package (now built-in). Updated test packages (Microsoft.NET.Test.Sdk → 17.13.0, xunit → 2.9.3, xunit.runner.visualstudio → 2.8.2). Restored DOTNETCORE define constant required by platform-branching #if guards. Api.0002 TimeSpan.FromMilliseconds calls compiled cleanly without changes. Full solution build: 0 errors, 0 warnings.


## [2026-03-27 09:52] 01-project-upgrade

Upgraded all 4 projects to net10.0. Full solution build: 0 errors, 0 warnings.


## [2026-03-27 10:07] 02-code-modernization

Applied C# 13 / .NET 10 modernization across 80+ files: file-scoped namespaces (68 files via script), pattern matching in Equals overrides, HashCode.Combine, expression-bodied members, switch expressions, early returns/guard clauses, ArgumentNullException.ThrowIfNull, null-conditional event invocations, target-typed new, auto-properties. Key files fully modernized: Core types (Point, Rect, Size, Vector, Thickness), Events (RelayCommand, RoutedEvent, MouseEventArgs), Controls (ButtonBase, CheckBox, RadioButton), Binding (converters, validators), XAML attributes. No public API or behavioral changes. Build: 0 errors, 0 warnings.


## [2026-03-27 10:08] 03-test-validation

All 60 tests pass (0 failures). Committed 119 changed files to branch upgrade-to-NET10 with message 'upgrade: migrate solution to .NET 10 with code modernization'.

