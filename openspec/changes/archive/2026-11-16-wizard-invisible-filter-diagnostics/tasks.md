# Tasks

## 1. Character kinds (Domain)

- [x] 1.1 Red `CharacterKindsShould.ClassifyEachKind` (letter, digit, space, tab, U+200B, U+001B, U+00A0, `-`, an emoji outside the basic plane as one `Other`), then green `Domain\CharacterKind` and `Domain\CharacterKinds.Of(Rune)` and `First(string)`; verify with the Core suite.
- [x] 1.2 Red `CharacterKindsShould.CallATextInvisibleOnlyWhenEveryCharacterIs` (empty is false, a space and U+200B are true, " o" is false), then green `AllInvisible(string)`; verify with the Core suite.

## 2. The wizard's problem line (Application)

- [x] 2.1 Red `WizardViewShould.ExplainAFilterOfInvisibleCharacters` (a space, then U+200B: `Problem` is `InvisibleFilter`, `Filter` still holds the character, `Choices` is empty), then green `WizardProblem.InvisibleFilter` and its rule in `WizardView.Filtered`; verify with the Core suite.
- [x] 2.2 Red `WizardViewShould.ClearTheInvisibleFilterProblemOnlyWhenTheFilterShowsSomething` (Backspace to empty clears it; " o" clears it; "zzz" never sets it; a `NoRepository` problem is kept by a filter change, as today), then green; verify with the Core suite.
- [x] 2.3 Red `WordingShould.DescribeAnInvisibleFilter` (the exact sentence, 76 cells or fewer), then green the case in `Wording.Describe(WizardProblem)`; verify with the Core suite, including the reflection facts that every problem case has words and `SpellBritish`.

## 3. The diagnostic events (Application)

- [x] 3.1 Red `DiagnosticLineShould.RenderTheWizardOpening` and `RenderTheWizardsFirstFilterChange` (the two lines in the design), then green `WizardOpened` and `WizardFilterStarted` and their arms in `DiagnosticLine.Body`; verify with the Core suite, including `DiagnosticEventsShould`.
- [x] 3.2 Red `WizardFilterWatchShould.RecordTheRepositoriesOfferedOnOpening` and `RecordNothingForAFixedFolder`, then green `Application\Diagnostics\WizardFilterWatch.Open` over a `DiagnosticLog` and the tests' fake clock; verify with the Core suite.
- [x] 3.3 Red `WizardFilterWatchShould.RecordTheFirstFilterChangeOnce` (the time since opening, the kind, the count, filled or not; a second change and an empty text record nothing), then green `FilterChanged` and `ListFilled`; verify with the Core suite.
- [x] 3.4 Red `DiagnosticsPrivacyShould.KeepTheWizardFilterOutOfTheLog` (the filter receives "Northwind.Orders" a character at a time, and neither "Northwind" nor "Orders" is in any rendered line), then green if the watch needs a change; verify with the Core suite.

## 4. The dialog and its wiring (Tui and composition)

- [x] 4.1 Give `DialogEffects` a `TimeProvider` and pass it to `LaunchWizardDialog.Show`. Build the watch there with `WizardFilterWatch.Open`, call `FilterChanged` from the filter's `TextChanging` before `Apply`, and call `ListFilled` from `Push`. Pass `TimeProvider.System` in `ConsoleComposition.Dialogs` and `tools\ReadmeScene\SceneHost`. Verify that `dotnet build -c Debug` passes with warnings as errors, and that the full `dotnet test -c Debug --no-build` passes.
- [x] 4.2 Add a smoke step that types a space into the wizard's filter and waits for the invisible-filter sentence (`SmokeScript`, with `SmokeScriptShould` updated first), then verify the smoke check passes on both architectures in CI's `check` jobs, which run it against the installed zip and the seeded worktree.

## 5. Release housekeeping

- [x] 5.1 Raise `Version` in `src\Source191.Cader\Source191.Cader.csproj` with `.github\scripts\Step-Version.ps1`, and add a `## Unreleased` line to `CHANGELOG.md` saying the wizard now explains a filter holding only blank or invisible characters. Verify that `Assert-VersionBumped.ps1` and the changelog check pass, and that `dotnet format --verify-no-changes` is clean.
- [x] 5.2 Run `openspec validate wizard-invisible-filter-diagnostics --strict` and verify it passes.

## Workflow follow-up

- Archive the change in the same pull request, once its tasks are done, so `openspec\specs` matches `main`.
- When the wizard next opens empty, read the two log lines from a problem report and open the follow-up change for the cause.
