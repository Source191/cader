# Design

## Context

- `LaunchWizardDialog` binds `WizardControls` to `WizardView.Reduce`. Every filter change arrives as `WizardInput.FilterChanged` from the filter's `TextChanging`, and `Push` fills the list from `state.Choices` first from `Initialized` and then after every reduce.
- `RepositoryChoice.Matches` is a case-insensitive `Contains`, so a filter holding one blank character matches nothing. The wizard then shows an empty list, an empty preview and no problem.
- `DiagnosticLog.Record` takes `DiagnosticEvent` cases. `DiagnosticEventsShould` allows only numbers, `bool`, `TimeSpan`, versions, enums and the vetted values as members, and `DiagnosticLine.Body` gives every case its line. Problem reports already carry the log's lines.
- `DialogEffects` builds the wizard and has no clock. Its two constructions are `ConsoleComposition.Dialogs` and `tools\ReadmeScene\SceneHost`.

## Goals / Non-Goals

**Goals:**
- One rule for "invisible", shared by the problem line and the log, so the two never disagree.
- Every rule in Core, under tests; the Tui only calls it.

**Non-Goals:**
- Changing what the filter holds, or how keys reach it.
- Recording anything for the other dialogs.

## Decisions

### A character kind in Domain
`Domain\CharacterKind` (`LetterOrDigit`, `Whitespace`, `Control`, `Format`, `Other`) and `Domain\CharacterKinds`:
- **`Of(Rune)`:** whitespace first (`Rune.IsWhiteSpace`), then the Unicode categories `Control` and `Format`, then `Rune.IsLetterOrDigit`, otherwise `Other`.
- **`First(string)`:** the kind of the first rune.
- **`AllInvisible(string)`:** true for a non-empty text whose every rune is `Whitespace`, `Control` or `Format`.

Runes rather than chars, so a character outside the basic plane counts once and is never split into its surrogates. The Unicode category tables are part of CoreLib, not ICU, so this holds in invariant globalisation mode. Rejected alternative: `char.IsWhiteSpace` alone, which misses zero-width and control characters, the likeliest strays besides a space.

### The problem is the reducer's
`WizardProblem.InvisibleFilter`, set in `WizardView.Filtered` when `CharacterKinds.AllInvisible(text)`. Any other filter text clears it, but only that case: `NoRepository` and `Refused` stay as they behave today. `Wording.Describe` words it as "The filter holds only blank or invisible characters; Backspace clears it." That is 72 cells, inside the 76-cell problem line. Rejected alternative: deriving the message from `Filter` in `Wording`, which would bypass the problem line's one source, `state.Problem`.

### Two events, not one
- **`WizardOpened(int Repositories)`:** recorded as the overview's wizard opens.
- **`WizardFilterStarted(TimeSpan After, CharacterKind FirstCharacter, int Characters, bool ListFilled)`:** recorded at most once per wizard, on the first change that leaves the filter non-empty.

The count is recorded as the wizard opens, not with the first change, so a wizard whose filter never changes still answers the "empty catalogue" question. Lines:
- `wizard opened offering 3 repositories`
- `wizard filter first changed after 12 ms: Whitespace, 1 character, list filled`

### A per-wizard watch in Application
`Application\Diagnostics\WizardFilterWatch` holds the opening timestamp (`TimeProvider.GetTimestamp`), whether the list has been filled, and whether the first change has been recorded:
- **`Open(WizardState, DiagnosticLog, TimeProvider)`:** records `WizardOpened` only when `state.ShowsChooser`. A fixed folder has no filter and records nothing.
- **`ListFilled()`:** marks the list as filled.
- **`FilterChanged(string)`:** records `WizardFilterStarted` once.

In `LaunchWizardDialog`, the filter's `TextChanging` handler calls `FilterChanged` before `Apply`, so `ListFilled` reflects the state before that input's own `Push`. `Push` calls `ListFilled()`. Rejected alternative: timing in the Tui with locals, which would leave the once-only and the fixed-folder rules untested.

### The clock reaches the dialog through DialogEffects
`DialogEffects` takes a `TimeProvider`, passed on to `LaunchWizardDialog.Show`. `ConsoleComposition` and `SceneHost` pass `TimeProvider.System`; CI builds `SceneHost`, so a missed one fails the build. Rejected alternative: exposing `DiagnosticLog`'s clock, which would widen a class that only records.

## Risks / Trade-offs

- **[The message hides a real cause behind a workaround]:** the filter is never cleaned, and the log records the first change, so the evidence survives.
- **[A first change of several characters]:** a paste or a burst of escape-sequence text is recorded as its first character's kind and its count. The count alone shows a burst.
- **[The stray character arrives before `Initialized`]:** `ListFilled` would read false. That is the fact we want to know, not an error.
- **[Recording personal data]:** the events hold an enum, counts, a duration and a flag. `DiagnosticEventsShould` holds that by reflection, and a `DiagnosticsPrivacyShould` fact types a name into the filter and checks it is absent from the log.

## Migration Plan

None: no state file or setting changes. Patch version bump, and a `## Unreleased` changelog line for the wizard's new message.
