# .NET 10 Upgrade Plan

## Overview

**Target**: net10.0 (LTS)
**Scope**: 4 projects — 1 class library (ConsoleFramework), 2 example apps (ManyControls, TextEditor), 1 test project (Tests)

### Selected Strategy
**All-At-Once** — All projects upgraded simultaneously in a single operation.
**Rationale**: 4 projects, all on modern netstandard/netcoreapp, straightforward upgrade (TFM bumps + 1 package removal + minor behavioral fixes).

## Tasks

### 01-project-upgrade: Upgrade all projects to net10.0

Update target frameworks across all 4 projects from their current values (`netstandard2.0;netstandard2.1;netcoreapp3.0` / `netcoreapp3.0`) to `net10.0`. Remove the `System.Threading.Thread` 4.3.0 NuGet package from ConsoleFramework (functionality is now built into the framework). Update stale packages (`Microsoft.NET.Test.Sdk`, `xunit`, `xunit.runner.visualstudio`) to current versions. Fix the 4 occurrences of the `TimeSpan.FromMilliseconds(double)` Api.0002 breaking change in `EventManager.cs`, `ButtonBase.cs`, and `ConsoleApplication.cs` — the overload signature changed in modern .NET and must be updated to use `TimeSpan.FromMilliseconds(long)` or an equivalent. Restore dependencies and perform a full solution build, resolving all compilation errors in a single pass.

**Done when**: Solution builds with 0 errors, 0 warnings related to the upgrade; all projects target net10.0; `System.Threading.Thread` package reference is removed.

---

### 02-code-modernization: Modernize codebase to current C# standards

Apply targeted modernization across the codebase using the latest C# language features and SOLID principles. This covers: replacing verbose patterns with pattern matching, switch expressions, and target-typed `new`; applying early returns to reduce nesting; using `file`-scoped namespaces, `record` types where applicable, `is not null` checks, and `var` where it improves clarity; adding `readonly` to fields that are never reassigned; and replacing manual null checks with null-coalescing or null-conditional operators. Apply the same pass to test code where relevant. Do not alter public APIs or change observable behavior — only internal code quality improvements.

**Done when**: All modernized files build cleanly; no behavioral changes introduced; code follows early-return and latest C# idioms throughout.

---

### 03-test-validation: Run full test suite and validate

Run the full xunit test suite in the `Tests` project. All tests must pass. Address any test failures introduced by the upgrade or modernization tasks (not pre-existing failures).

**Done when**: All tests pass; no regressions introduced by this upgrade.
