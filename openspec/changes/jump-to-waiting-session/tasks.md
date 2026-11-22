# Tasks

**Implement with:** Sonnet 5.5 at medium effort: one reducer-only key that moves the selection, following the documented four-piece recipe for a new key (here with no handler, as it raises no effect), with `KeyHelpParityShould`, `StatusTonesShould` and the `Wording` facts failing for any missing piece.

Each task is red, green, refactor, with a commit per step. Reducer facts are sociable over real `SessionRow`s grouped by `TableLines.Grouped`.

## 1. Reducer

- [ ] 1.1 `SessionsViewShould.SelectTheNextSessionThatNeedsInput` (across a heading) and `WrapToTheFirstRow`. Commit.
- [ ] 1.2 `SessionsViewShould.SayNoSessionNeedsInput` and `RespectTheFilter`: the new `StatusMessage` case, its words (`WordingShould`), its tone (`StatusTonesShould`). Commit.

## 2. Keys and help

- [ ] 2.1 `w` in `WordingOfScreens.SessionsKeys` and the Sessions bar's second row; verify `KeyHelpParityShould` and the bar layout facts pass at 120 x 36. Update `docs\user-guide.md` and the README's key list. Verify with the headless smoke check (`dotnet run -- smoke <exe> <worktree-folder>` from `tools\HeadlessTerminal`, exit 0). Commit.

## 3. Release

- [ ] 3.1 Raise `Version` in `src\Source191.Cader\Source191.Cader.csproj` one preview step above the previous step of #379 and add a CHANGELOG `Unreleased` entry under Added (`w` moves to the next session that needs input). Verify `.github\scripts\Assert-VersionBumped.ps1`, `dotnet build -c Debug`, `dotnet test -c Debug --no-build`, `dotnet format --verify-no-changes` and `openspec validate jump-to-waiting-session`. Commit.

## Workflow follow-up

- Archive the change in the pull request that completes it.
