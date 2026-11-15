# Proposal

## Why

The new-session wizard sometimes opens with an empty repository list, and only closing it and pressing `n` again brings the repositories back. The screenshot's caret sits one cell into the filter, so the filter most likely holds one character that draws as blank, which matches no repository. Nothing in the wizard says so, and nothing in the log can tell that case apart from an empty catalogue or a list that was never filled. Until the source of the stray input is known, the user needs to see why the list is empty, and the next report needs enough evidence to name the cause.

## What Changes

- The wizard's problem line says when the filter holds only blank or invisible characters (whitespace, control or format characters), and that Backspace clears it. The filter is not trimmed or cleaned, so the evidence stays in front of the user.
- The diagnostic log records the number of repositories the wizard offered as it opens.
- The diagnostic log records the wizard's first filter change, once per wizard: how long after the dialog opened it arrived, the kind of its first character (letter or digit, whitespace, control, format, other), how many characters the filter then held, and whether the dialog had filled its controls before it. The character itself is never recorded.
- Both events reach a problem report (`!`) through the existing log, so the user can share them straight away.

Out of scope:
- Finding and removing the source of the stray input; that is a later change, decided by what these events show.
- Trimming or cleaning the filter, and any change to the other dialogs that open with focus in a text box (rename, forced removal, problem report, settings).
- Multiple Claude accounts, which is a later change.

## Capabilities

### New Capabilities

(none)

### Modified Capabilities

- `session-launch`: the repository picker explains a filter that holds only blank or invisible characters.
- `diagnostics`: the new-session wizard records the repositories it offered and its first filter change, as facts with nothing typed in them.

## Impact

- `Source191.Cader.Core`: a character-kind rule, a new `WizardProblem` case and its rule in `WizardView`, its words in `Wording`, two `DiagnosticEvent` cases and their lines in `DiagnosticLine`, and a small Application class that times the first filter change.
- `Source191.Cader.Tui`: `LaunchWizardDialog` records the events; `DialogEffects` takes a `TimeProvider`.
- Wiring: `ConsoleComposition` and `tools\ReadmeScene\SceneHost` pass `TimeProvider.System` to `DialogEffects`.
- Patch version bump in `src\Source191.Cader\Source191.Cader.csproj`.
