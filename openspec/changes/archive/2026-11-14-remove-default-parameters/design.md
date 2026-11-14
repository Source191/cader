# Design

## Decisions

- **The check:** a syntax-tree fact over `src` that fails for any `ParameterSyntax` with a `Default`. The exception is by name: parameters of the five state-file records, whose `= null` marks an optional JSON member the reader relies on. Any other default needs an argued exception in that list.
- **Call sites:** a value is passed positionally when the parameter's meaning is clear from the expression, and by name when a literal `true`, `false` or `1` would not say what it is (`inUse: false`, `Span: 1`, `branchCreatedHere: true`).
- **Settings constants:** `Settings.ChecksForUpdatesWhenUnset`, `PlaceWhenUnset` and `DiagnosticLogWhenUnset` remain; callers that want the unset value pass them by name.
- **Outcome records:** only the parameter's default goes; the cases are unchanged.
