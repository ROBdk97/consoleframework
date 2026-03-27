# Scenario Instructions: dotnet-version-upgrade

## Strategy
**Selected**: All-at-Once — all 4 projects upgraded simultaneously.

## Preferences
- **Flow Mode**: Automatic
- **Commit Strategy**: Single Commit at End
- **Target Framework**: net10.0 (LTS)
- **Code Modernization**: Apply SOLID principles, early returns, and latest C# language features

## Source Control
- Source branch: `develop`
- Working branch: `upgrade-to-NET10`

### Execution Constraints
- Single atomic upgrade — all projects updated together in one pass
- Fix all compilation errors in a single bounded pass after TFM update
- Validate full solution build (0 errors) before moving to code modernization
- Code modernization must not change public APIs or observable behavior
- Run full test suite after all changes complete

## Key Decisions Log

| Date | Decision |
|------|----------|
| Today | Target net10.0 LTS confirmed by user |
| Today | Code modernization (SOLID, early returns, latest C# features) to be included |
| Today | Flow mode: Automatic |
| Today | Strategy: All-at-Once (4 projects, straightforward upgrade) |
| Today | Commit strategy: Single Commit at End |
