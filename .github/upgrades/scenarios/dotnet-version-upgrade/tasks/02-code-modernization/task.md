# Task: 02-code-modernization

## Objective
Apply C# 13 / .NET 10 idioms throughout the codebase. No public API changes. No behavioral changes.

## Modernization Patterns

| Pattern | Example |
|---------|---------|
| File-scoped namespaces | `namespace X;` |
| Pattern matching Equals | `if (o is not T t) return false;` |
| Expression-bodied members | `get => x;` / `=> expr;` |
| Target-typed new | `new()` instead of `new SomeType()` |
| Null-conditional invocation | `evt?.Invoke(...)` |
| `HashCode.Combine` | replace XOR chains in GetHashCode |
| `ArgumentNullException.ThrowIfNull` | replace manual null guard |
| Early returns / guard clauses | reduce nesting |
| `var` where type obvious | constructor calls |
| Auto-properties | single-line backing fields |

## Scope (80 cs files)
- ConsoleFramework/Core/* — value types (high density of expression body opportunities)
- ConsoleFramework/Events/* — relay commands, event args
- ConsoleFramework/Binding/** — converters, validators, observables
- ConsoleFramework/Controls/* — button base, controls
- ConsoleFramework/Xaml/* — markup types
- ConsoleFramework/ConsoleApplication.cs — only safe patterns
- Tests/* — test modernization

## Files modified in this session
(updated as changes are applied)
