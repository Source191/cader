# Tasks

**Implement with:** Sonnet 5.5 at medium effort: a changed default and two time windows in `AccountCatalogue`, each covered by existing sociable tests with `FakeTimeProvider`; most of the work is the spec scenarios that quote the old default.

Each task is red, green, refactor, with a commit per step.

## 1. Sessions refresh default

- [ ] 1.1 `SettingsFileShould.ReadAMissingSessionsRefreshAsThirtySeconds` and `KeepASavedFiveSeconds`: `Settings.SessionsRefreshWhenUnset` becomes 30. Update every fact that assumed 5 for an unset value (`SettingsShould`, `ConfigRows`, `WordingOfConfig`, `TestSettings`), keeping facts that set 5 on purpose. Commit.
- [ ] 1.2 `README.md`, `docs\user-guide.md` and `.claude\rules\console-screen.md`, Refreshing: the default and why the folder watches make it safe. Verify by searching the docs for "5 seconds" and "five seconds" next to Sessions refresh. Commit.

## 2. Login read less often

- [ ] 2.1 `AccountCatalogueShould.WaitFifteenMinutesAfterAnAnswer` and `TryAgainAfterAMinuteWhenTheReadFailed`: `RefreshStaleAsync` picks its window from the last result. Commit.
- [ ] 2.2 `AccountCatalogueShould.ReadWithinAMinuteWhileAnOwnerIsUnseen`: `RefreshStaleAsync(bool anyUnseen)`. `SessionRowEnricherShould.AskForAFreshLoginWhenARowsOrganisationIsUnseen`: the enricher passes whether any row's account is `RowAccount.Unseen`. Commit.

## 3. Release

- [ ] 3.1 Raise `Version` in `src\Source191.Cader\Source191.Cader.csproj` one preview step above what `main` holds when this merges and add a CHANGELOG `Unreleased` entry under Changed (sessions listed every 30 seconds by default, a saved interval kept; the login checked every 15 minutes). Verify `.github\scripts\Assert-VersionBumped.ps1`, `dotnet build -c Debug`, `dotnet test -c Debug --no-build`, `dotnet format --verify-no-changes` and `openspec validate fewer-background-polls`. Commit.

## Workflow follow-up

- Archive the change in the pull request that completes it.
