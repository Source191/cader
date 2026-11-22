# Tasks

**Implement with:** Opus 5.5 at medium effort: layout arithmetic in pure Application code with sociable tests, then Terminal.Gui wiring checked in the headless terminal at several sizes.

Each task is red, green, refactor, with a commit per step. Never press a saving key when driving a real cader exe headless (it writes the real config.json).

## 1. Wizard fit

- [x] 1.1 Measure the overview wizard's chrome (the rows a 90%-high dialog leaves the list, less the form's 21 and the filter's 2) in the headless terminal at 120 by 36 and 120 by 40 with the current build, and record the figure and date in `.claude\rules\terminal-gui.md` beside the Dialog facts. Commit.
- [x] 1.2 `WizardFitShould.FitTheOverviewWizardAt120By36WithFourListRows`, `FitAt120By30WithOneListRow`, `KeepAMarginOfUpToTwoRows`, `NeverShrinkAsTheScreenGrows` (a theory over 30 to 50 rows) and `RefuseBelowItsMinimum` (79 columns; one row short): `Application\Views\WizardFit` (closed hierarchy `Fits`/`TooSmall`, design 1). Commit.
- [x] 1.3 `WizardFitShould.FitTheFixedFolderWizardAt84By23` and `RefuseTheFixedFolderWizardBelow84By23`. Commit.
- [x] 1.4 `StatusMessage.WizardTooSmall` with its words in `Wording` (`WordingShould.SayTheSizeTheWizardNeeds`) and its tone in `Wording.ToneOf` (a line in `StatusTonesShould`). Commit.
- [x] 1.5 `DialogEffectsShould.RefuseTheWizardOnATooSmallScreen`: `DialogEffects.StartSession` asks `WizardFit` with the screen's size and shows `WizardTooSmall` instead of opening; `LaunchWizardDialog` takes the fit's height, and `WizardControls.Repositories` becomes `Dim.Fill(FormRows)` with no minimum. Commit.

## 2. Title and key bar

- [x] 2.1 `KeyBarShould.LeaveARowThatFitsUnchanged`, `DropItemsFromTheEndUntilTheRowFits`, `KeepHelpAndQuitAndMoveThemLeft` and `ShowWhatFitsWhenEvenHelpAndQuitDoNot`: `KeyBar.FittedTo(int width)` (design 4). Commit.
- [ ] 2.2 `ConsoleScreen.ShowBar` draws `Bar(...).FittedTo(ScreenWidth)`, and `ShowFrameTitle` draws nothing when under two cells are left after the counts (design 3). Commit.

## 3. Check and release

- [ ] 3.1 In the headless terminal (Drive with `HARNESS_COLUMNS` and `HARNESS_ROWS`), open and cancel the wizard at 120 by 30, 120 by 36 and 120 by 40, press n at 70 by 24 and n on a worktree row at 70 by 24, and show Sessions at 60 by 20; check each against the spec's scenarios with `grid`. Run the smoke check (`dotnet run -- smoke <exe> <worktree-folder>` from `tools\HeadlessTerminal`, exit 0). Commit any fix found.
- [ ] 3.2 `.claude\rules\console-screen.md`, Dialogs: the wizard's height comes from `WizardFit` and a too-small terminal is refused; Layout: the key bar is fitted to the width and the title is left out when it has no room. Commit.
- [ ] 3.3 Raise `Version` in `src\Source191.Cader\Source191.Cader.csproj` one preview step above the previous step of the implementation plan, and add a CHANGELOG `Unreleased` entry under Fixed: the new-session wizard fits Windows Terminal's default 120 by 30 (or says the terminal is too small), and narrow panes keep the counts readable and ? Help and q Quit on the key bar. Verify `.github\scripts\Assert-VersionBumped.ps1`, `dotnet build -c Debug`, `dotnet test -c Debug --no-build` and `dotnet format --verify-no-changes`. Commit.

## Workflow follow-up

- Archive the change in the pull request that completes it.
