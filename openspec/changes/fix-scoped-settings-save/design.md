# Design

## Context

See proposal.md, Why. The crash path, from the log and screenshot attached to #358 (0.22.0-preview.48):

- `SettingsState.Open` loads the whole saved settings as text and marks the one value the dialog edits by its `SettingsScope`.
- On F5, `SettingsView.Saved` runs `Settings.Validate` over all of it, with the real folder and file checks, and on a refusal sets `Focus = FieldOf(problem)`.
- `SettingsControls` builds a `Fields` dictionary holding only the scope's controls, and `Show` calls `Fields[state.Focus].SetFocus()`, which throws for any other field.

Values outside the scope come from settings that were valid when saved, but two checks also depend on the disc (a root's folder, a custom executable), and one file reading is lenient (a root listed twice is kept). Those are the only refusals that can name a field outside the scope.

## Goals / Non-Goals

**Goals:**

- The reducer never answers a focus the dialog does not show, for any scope and any refusal.
- A setting can be saved whatever has changed on disc for another setting.

**Non-Goals:**

- Changing `Settings.Validate`, `SettingsControls` or the problem words.

## Decisions

### 1. Each scope names the fields its dialog shows

`SettingsScope` gains `Fields`: its own `Field` for every scope, and Editor, Executable and Arguments for `Editor`. It is the Core statement of what `SettingsControls` builds per scope.

- **Alternative:** read the shown fields from the Tui dictionary. Rejected: the reducer is in Core and cannot see Tui.
- **Alternative:** guard in `SettingsControls.Show` with `TryGetValue`. Rejected as the fix: it hides a reducer answer the screen cannot show, and leaves the save blocked by a vanished root; the reducer is where the rule is tested.

### 2. The disc checks follow the scope's fields

`Saved` hands `Settings.Validate` the state's `FolderExists` only when the scope shows `Roots`, and its `FileExists` only when it shows `Executable`; otherwise a check that always answers true. A root or editor that has gone is still reported where it is fixed, and is marked on the Config tab as today.

- **Alternative:** validate only the scope's own value and copy the rest from the saved `Settings`. Rejected: it needs a second path through `Settings.Validate` or a `Settings.With...` per value, for no visible gain; the form checks on saved values pass already.

### 3. A refusal outside the scope keeps the current focus

`Saved` sets `Focus` to `FieldOf(problem)` when the scope shows that field, and otherwise leaves `state.Focus`, which is always one the dialog shows (it starts at `Scope.Field` and changes only through `FieldFocused` from the dialog's own controls). The problem line still names the refusal, so the user knows which setting to fix. After decision 2 this is reached only by a root listed twice in a hand-edited file.

- **Alternative:** move focus to `Scope.Field`. Rejected: in the editor dialog it would pull the cursor out of the Arguments box the user was in.

## Risks / Trade-offs

- **`SettingsScope.Fields` and the `SettingsControls` dictionary could drift** when a scope gains a field → a theory in `SettingsViewShould` over every scope and every refusal checks focus is in `Fields`; the smoke script opens each settings dialog; the Tui dictionary stays beside the scope switch it mirrors.
- **A gone root is saved again unchanged** by another dialog → intended: removing it is the user's choice in the Repository roots dialog, and the Config tab marks it (not found).
