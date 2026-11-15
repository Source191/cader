# Proposal

## Why

Deleting the Cader or Cader session profile in Windows Terminal's settings, then running `cader setup-terminal` to add it back, does nothing visible: the fragment is written, Terminal is restarted, and the profile stays out of the new-tab menu and the Settings page, while `setup-terminal` reports success and the Config tab's Windows Terminal profiles row reads installed (issue #218, seen on 4 November 2026 with Windows Terminal 1.25.2733.0 under the tool's earlier name, Vantage). Terminal remembers every generated profile it has loaded, fragment profiles included, in `generatedProfiles` in its `state.json`, and keeps one hidden as deleted by the user whenever `settings.json` has no entry with its id, however often the fragment is rewritten.

## What Changes

- `cader setup-terminal` works out, after its fragment step, whether Windows Terminal is hiding either Cader profile as deleted: the profile's id (derived as Terminal derives it) is in `state.json`'s `generatedProfiles` and no entry in `settings.json`'s profile list has that id.
- When it is, the command shows Terminal's settings file and the entry it will add for each hidden profile (`guid`, `"hidden": false`, `name`, `source`), the entry the issue's workaround showed bringing a profile back, and asks "Show the hidden profiles again? [y/N]"; yes, or `--yes`, inserts only those entries into the profile list and leaves every other character of the file as it was, comments included. Terminal applies the change when the file is saved, with no restart.
- The guarantee that setup leaves Windows Terminal's settings file alone narrows to one exception: adding those entries, only for a hidden Cader profile, only after the user agrees. A run with nothing hidden still leaves the file untouched.
- When Terminal's files are missing (Terminal has never run) or cannot be read, the command reports nothing hidden or says it could not read them, writes nothing to them, and still writes the fragment and offers the bell as today.
- The Config tab's Windows Terminal profiles row reads that Windows Terminal is hiding the profiles, and to run `cader setup-terminal`, when the fragment exists and either profile is hidden; that counts as setup to do in the tab's amber count.
- The hiding rule and the id derivation are recorded with their date and Terminal version in `.claude\rules\windows.md`, and the user guide gains a troubleshooting line for profiles that do not appear after `setup-terminal`.

## Capabilities

### New Capabilities

None.

### Modified Capabilities
- `needs-input-notification`: "Setup leaves Windows Terminal's own settings alone" gains its one exception and a scenario for it, and the first-run scenario of "The setup command adds Cader's Windows Terminal profiles" is limited to a Terminal hiding neither profile; two added requirements say when setup reports a profile Terminal is hiding and how it shows it again.
- `config-tab`: an added requirement says what the Windows Terminal profiles row reads while Terminal hides a Cader profile, and that it counts as setup to do. The existing setup requirements are left as they are, because `install-with-winget` and `diagnostic-log-setting` already carry modified copies of them.

## Impact

- **Code:** a Domain rule for Terminal's profile ids; a new port for Terminal's settings and state files with its adapter under `Adapters\Terminal`; a new step in `TerminalSetupRunner`; a new `SetupState` case read by `SetupFactsReader`; the words in `Wording` and `WordingOfConfig` and the count in `WordingOfConsole`; the fragment's source name (`Cader`, today a literal in `TerminalFragment`) gets one home in `Ports\TerminalProfiles`. Wiring in `SharedParts`, `ConsoleComposition`, `TerminalSetupComposition` and `tools\ReadmeScene\SceneHost`.
- **Tests:** facts for the id rule (the issue's observed Vantage pair is the known answer), the new adapter over a temporary folder, the runner, the reader and the words; a hand-written fake for the new port in `TestDoubles`. No test or session reads the real Terminal files; `CLAUDE.md`'s working rule names them beside the Fragments folder.
- **Other open changes:** `diagnostic-log-setting` (F in `openspec\implementation-plan.md`) also changes `SetupFactsReader`'s constructor and `SetupFacts`, so this change follows F and takes its shape.
- **Version:** a minor raise, because it is a new behaviour.

## Out of scope

- Multiple Claude accounts, which is a later change. The "Claude (<name>)" profiles that `set-up-an-account` proposes sit in the same fragment folder, so Terminal can hide them the same way; this change covers the two Cader profiles only, and its id rule and inserter take any profile name, so that change can reuse them.
- Windows Terminal Preview, Canary and the unpackaged (zip or portable) builds: their settings and state live in other folders, unverified; only the Store package `Microsoft.WindowsTerminal_8wekyb3d8bbwe` is read.
- A Cader profile the user hid with Terminal's "Hide profile from dropdown" (its entry says `"hidden": true`): that is the user's choice in Terminal and is left alone.
- Editing Terminal's `state.json`, which Terminal owns and rewrites while it runs; the elevated `elevated-state.json`.
- `--remove`, which still deletes only the fragment folder and never edits Terminal's settings file.
- What `wt new-tab --profile "Cader session"` does while that profile is hidden; a manual check records it, and any fix it calls for is a change of its own.
