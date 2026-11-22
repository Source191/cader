---
paths:
  - "src/Source191.Cader.Core/Application/Views/**"
  - "src/Source191.Cader.Core/Application/Effects/**"
  - "src/Source191.Cader.Core/Application/Actions/**"
  - "test/Source191.Cader.Core.Tests/Application/Views/**"
  - "test/Source191.Cader.Core.Tests/Application/Effects/**"
  - "test/Source191.Cader.Core.Tests/Application/Actions/**"
  - "**/Wording*.cs"
  - "**/*Wording*Should.cs"
  - "**/StatusMessage*.cs"
  - "**/StatusTones*.cs"
  - "**/KeyHelp*.cs"
  - "**/ScreenLoop*.cs"
  - "**/*Handler*.cs"
  - "**/*Outcome*.cs"
  - "src/Source191.Cader.Tui/DialogEffects*.cs"
  - "src/Source191.Cader.Tui/TableBody*.cs"
  - "src/Source191.Cader.Tui/*Dialog*.cs"
  - "src/Source191.Cader.Tui/*Controls*.cs"
---

# Reducers, effects, keys and Wording

How a view, a key, an effect and their words fit together. Read this before planning a new key, effect, view or dialog. The screen's layout is in `console-screen.md`.

## Reducers and views

- **Reducers** (`Application\Views\*View.cs`) are pure functions of state and input, composed over one `ListSelection<TRow>`. Everything a key does is a tested effect; the `Tui` layer only renders and runs effects.
- **A view is an `IScreenDefinition`:** columns (each its help from `Wording`, its cell and its fit), colours, tab title, counts, key bar and reducer. A new view adds a reducer, a definition and a tab.
- **Dialogs with rules are reducers too,** bound to their controls (the wizard and the settings dialogs: `console-screen.md`, Dialogs).
- **`ReducersShould`** holds that every `*View` is a static class with only `static readonly` fields; the `IFolders` and `Func<string, bool>` that `WizardState` and `SettingsState` carry are the allowed exception.

## Routing effects

- A `TableBody` routes each effect by its marker:
  - **`ISwitchesTab`** to `ConsoleView.Apply`.
  - **`IConsultsSessions`** (Enter on a worktree) to `ConsoleView.OpenSession`, which answers from the Sessions rows with the same `RowDecision` as Sessions' own Enter.
  - **`ILeavesScreen`** (`Quit`; `AttachToSession` keeps the marker but only `cader here` raises it) back to `ScreenLoop`, which runs the console once, never hands its terminal to a child and throws `UnreachableException` for any exit but quit.
  - **`IDialogEffect`** to `Tui\DialogEffects`.
  - **`IRefreshesTab`** (F5) to `ITabHost.RefreshShown`, which starts the shown tab's refresh unless one is running.
  - **Everything else** to its handler through `Application\Effects\EffectDispatcher`.
- **`EffectRoutingShould`** fails for a marked effect nothing routes.

## A new key

- **Four pieces and no screen change:**
  1. The effect record, with a marker when it leaves the screen or needs a dialog.
  2. The reducer case and its test.
  3. A handler: an `Application\Effects\IEffectHandler<TEffect>` in `Application\Actions`, one class per effect, returning an outcome record declared in its own file, with a `...HandlerShould` test asserting that record.
  4. Its line in `Application\Effects\Composition.Handlers`.
- **Its words:** its text in `Wording` with a fact; the words for its outcome in `WordingOfOutcomes` with a fact; its tone in `Wording.ToneOf` with a line in `StatusTonesShould`; a dialog flow in `DialogEffects`.
- **What fails when a piece is missing:** `EffectDispatcherShould`, `DialogEffectsShould` and `ScreenLoopShould`; `HandlersShould` fails for a handler with no `<Name>Should` class.
- **Keys and columns are data in `Wording`:** a `KeyHelp` list per view (`SessionsKeys`, `WorktreesKeys`, `ConfigKeys`; `KeyHelp.HelpOnly` for a key only the help lists, such as the tab keys in `TabKeyHelp`) and a `ColumnHelp` list (`SessionsColumns`, `WorktreesColumns`, `ConfigColumns`, which also name the columns).
- **The help and every tooltip** are generated from those lists, so a new key or column is one line there. `KeyHelpParityShould` presses every listed key on a loaded table and fails for one nothing answers; the tab keys answer through `TabKeys` and `<` and `>` through `ScrollKeys`.
- **Folders to other tools:** a row's folder goes out through `c` (copy, `IClipboard` over Terminal.Gui's clipboard, `Tui\ScreenClipboard`), `f` (editor, `IEditorLocator` finding a preset's real executable as a `ProgramLocation`) and `s` (solution, `IDesktop` opening a file with its associated application), each an effect with a handler.

## Wording

- **All user-facing text** lives in `Application\Wording` and its partial files, each with its `Wording*Should` tests. Two exceptions: the command line's descriptions sit beside their symbols in `Application\CommandLine` (except the root's support line, `Wording.SupportLine`), and System.CommandLine's own help, parse errors and suggestions (American English, not replaceable).
- **Words at the last moment:** reducers return `StatusMessage` values, handlers return outcome records, and the screen asks `Wording` for the words. Reflection facts fail when a status message or outcome case has no words.
- **What is `Wording`'s:** the waiting words, failure prefixes, dialog titles and buttons, the STATE words (`Wording.StateWord`) and anything that varies with state. Static captions and hints beside a control may stay in the Tui binder that lays them out, as may glyphs and the placeholder a label needs.
- **`SentenceLiteralsShould`** fails for a string in `Application` outside `Wording*.cs`, `CommandLine.cs`, `Diagnostics\` and a `throw` that ends in `.`, `?` or `!`. `SourceConventionsShould.SpellBritish` checks the string literals of `Wording` and `CommandLine`.
- **User text never comes from an exception message:** text a person typed is checked by a `Validate` that returns the reason as a value (`NameProblem`, `LaunchProblem`) and `Wording` says it; text from outside the program goes through a `TryParse` that answers false; an exception's message is shown only to report what failed.
- **`catch (ArgumentException`** appears only in `ClaudeSettings`, which rethrows a repeated JSON key as a `JsonException` (`SourceConventionsShould.CatchArgumentExceptionOnlyWhereNamed`).

## Open hierarchies the reflection facts rely on

- `StatusMessage`, `Outcome`, `LaunchProblem`, `WizardProblem` and `WizardInput` grow a case per feature and keep a delegating or fallback arm in their switches, which the reflection facts rely on to spot a missing case: do not "fix" it to a throwing discard.
