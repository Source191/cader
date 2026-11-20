# Proposal

## Why

Saving one setting's dialog can crash the console (#358). Each Config row opens a dialog that edits one value, but F5 checks every setting, including the ones the dialog does not show. When one of those is refused, the dialog moves focus to the refused field, which only another dialog has, and `SettingsControls.Show` throws `KeyNotFoundException`, ending Cader with exit code 1.

It happens to ordinary users without a hand-edited file:

- **A repository root that has gone:** a saved root whose folder was deleted, renamed or is on a drive no longer attached (the Config tab already marks it (not found)). Every dialog but Repository roots then crashes on F5. This is #358: changing Sessions refresh from 5 to 2.
- **A custom editor that has gone:** the settings file keeps a custom editor whose executable no longer exists (the settings spec requires it), so every dialog but Open folder with crashes on F5.
- **A root listed twice in `config.json`:** a hand-edited file is read with the duplicate kept, so the same happens.

## Benefit

- **Users:** changing one setting never crashes the console, and is no longer blocked by something else that changed on disc since that setting was saved. A refusal names its problem and keeps the cursor in the dialog.
- **Maintainers:** the reducer's focus is always a field the dialog shows, held by a test over every scope, so the dialog can trust it.

## What Changes

- **Check the disc only for the value being edited:** saving checks that the repository roots exist only in the Repository roots dialog, and that a custom executable exists only in the Open folder with dialog. Every other value is still checked for its form, as today.
- **Keep focus in the dialog:** each `SettingsScope` names the fields its dialog shows, and a refusal about a field the dialog does not show keeps the problem line but leaves focus on the dialog's own field.
- **Version and changelog:** a preview step and a Fixed entry naming #358.

Out of scope:

- Removing a duplicate root when the settings file is read: a hand-edited file stays as lenient as the spec says; the Repository roots dialog is where it is fixed.
- Telling the user, in another setting's dialog, that a root has gone: the Config tab's (not found) mark and tooltip already do.
- Checking only the edited value's form: the other values come from saved settings and pass today; narrowing `Settings.Validate` itself is a larger change for no user gain.
- Multiple Claude accounts beyond naming a session's owner, which is a later change.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `settings`: "Settings are validated before they are saved" gains that a dialog's save is not refused by a folder or file that has gone for a setting it does not edit, and that a refusal never moves focus out of the dialog.

## Impact

- **Implement with:** Opus 5.5 at medium effort: a pure reducer change in Core with sociable tests, no threading, files or screen layout.
- **`src`:** `Application\Views\SettingsScope` (the fields each scope shows) and `Application\Views\SettingsView` (`Saved`). `Tui\SettingsControls` is unchanged.
- **Tests:** `SettingsViewShould` (Core.Tests).
- **Version and changelog:** `Version` takes the next preview step after the one `main` holds when this merges (0.22.0-preview.49, or .50 if `reduce-refresh-allocations` merges first); `CHANGELOG.md` gets a Fixed entry naming #358.
- **Users:** saving a setting works as described above; nothing else changes.
