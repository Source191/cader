# Tasks

Each task is red, green, refactor with a commit per step; "verify" means the Core suite (`dotnet test test\Source191.Cader.Core.Tests -c Debug --no-build` after `dotnet build -c Debug`) passes, reflection facts included, unless the task says otherwise.

## 1. The location case and its words (Application)

- [x] 1.1 Red `SessionsViewShould.FilterASessionWhoseFolderHasGoneByItsFolderName` (a row with `new SessionLocation.FolderGone("Northwind.Orders-fix-123")` stays under the filter `fix-123`), then green `SessionLocation.FolderGone(string Folder)` and its arm in `SessionRow.Repository`; verify.
- [x] 1.2 Red `WordingOfDetailShould.SayFolderGoneForASessionWhoseFolderHasGone` (the pane's Worktree and Changes lines both read `folder gone`, never `not in a repository`), then green the `FolderGone` arm in `PlaceOf` and the Changes choice in `Wording.Detail(SessionDetail, ...)`; verify.
- [x] 1.3 Red `WordingOfScreensShould.MarkAGoneFolderBeforeItsLabel` (`Wording.FolderGone(@"~\source\worktrees\Northwind.Orders-fix-123")` reads `folder gone: ~\source\worktrees\Northwind.Orders-fix-123`) and the updated REPOSITORY column help fact (names the folder gone mark), then green both in `WordingOfScreens`; verify, including `SpellBritish` and the help-height facts.
- [x] 1.4 Red `WordingOfScreensShould.TipOnlyARowWhoseFolderHasGone` (`Wording.SessionRowTip`: a background row whose folder has gone names `d` and is at most 80 characters, an interactive one says only that the folder has gone, every other row is empty), then green; verify.

## 2. Reading the folder before git (Application)

- [x] 2.1 Give `SessionRowEnricherShould` a `FakeFolders` and register every directory its existing facts use (`Session(...)` and `AnswerGit(...)` add it), keeping every existing fact green. Then red `SessionRowEnricherShould.MarkASessionWhoseFolderHasGoneWithoutStartingGit` (`Location` is `FolderGone("Northwind.Orders-fix-123")`, `Head` is `Unknown` and the fake runner captured no request), then green `IFolders` on `SessionRowEnricher`'s constructor and the `DirectoryExists` check at the top of `LocateAsync`, before the cache. Pass the existing `Folders` in `ConsoleComposition.Loads` and `tools\ReadmeScene\SceneHost` in the same commit so the solution builds. Verify that `dotnet build -c Debug` passes with warnings as errors.
- [x] 2.2 Red `SessionRowEnricherShould.MarkAFolderDeletedInsideTheCacheWindow` (read in its repository, folder removed from the fake, a second enrich ten seconds later answers `FolderGone`) and `ReadAFolderThatComesBackWithGit` (gone, then registered again: git is asked and the row is `InRepository`), then green if the check is not yet ahead of the cache or a gone answer is cached; verify.
- [x] 2.3 Red `DetailLoaderShould.AskGitNothingForASessionWhoseFolderHasGone` (the counts are `NotApplicable` and the fake runner captured no request), then green the `FolderGone` arm in `DetailLoader.SessionAsync`; verify.

## 3. Enter, rename, resume-all and the key bar (Application reducers)

No new effect or handler: every refusal answers from the reducer.

- [x] 3.1 Red `WordingShould.DescribeAFolderGone` (`fix-123's folder no longer exists, so nothing was resumed. d deletes it.`) and its line in `StatusTonesShould` (Notice), then green `StatusMessage.FolderGone(ListedName Name)`, its words in `Wording.Describe` and its tone in `Wording.ToneOf`; verify, including the facts that every status message case has words and a tone.
- [x] 3.2 Red `SessionsViewShould.RefuseToOpenAStoppedSessionWhoseFolderHasGone` (Enter on a stopped background row with a transcript and `FolderGone` raises no effect and shows `StatusMessage.FolderGone`) and `OpenARunningSessionWhoseFolderReadsAsGone` (a running one still raises `OpenTabForSession`), then green the first arm of `SessionsView.Opening`; verify, including `ConsoleViewShould`, whose worktree Enter uses the same decision.
- [x] 3.3 Red `SessionsViewShould.RefuseToRenameASessionWhoseFolderHasGone` (stopped and running alike: no `AskForNewName`, `StatusMessage.FolderGone`), then green the first rule of `SessionsView.Rename`; verify.
- [x] 3.4 Red `SessionsViewShould.LeaveSessionsWhoseFolderHasGoneOutOfResumeAll` (two stopped rows, one `FolderGone`: `ResumeSessions` carries only the other; only a gone row shown: `NothingToResume`), then green the filter in `ResumeAll`; verify.
- [x] 3.5 Red `WordingOfKeyBarShould.OfferOnlyDeleteForAStoppedSessionWhoseFolderHasGone` (no Enter or r for the row; a running gone row keeps its keys), then green the arm in `Wording.SessionsBar`; verify, including `KeyHelpParityShould`.

## 4. The Sessions tab (Tui)

- [x] 4.1 Red `SessionsScreenDefinitionShould.MarkTheRepositoryCellOfASessionWhoseFolderHasGone` (the cell reads `folder gone: ` and the label, and at a narrow width keeps the prefix and cuts the label's end), then green the `FolderGone` arm in `SessionsScreenDefinition.Repository`; verify.
- [ ] 4.2 Red `SessionsScreenDefinitionShould.AnswerARowTipOnlyForASessionWhoseFolderHasGone`, then green `RowTip` returning `Wording.SessionRowTip(row)`; verify, then run the full `dotnet test -c Debug --no-build` and `dotnet format --verify-no-changes`.
- [ ] 4.3 Check by hand with `tools\install-local.ps1`, started from a terminal with `NO_COLOR`, `CLAUDECODE`, `CLAUDE_PID` and `CLAUDE_CODE_*` cleared, and the Diagnostic log on. Start a session in a new throwaway worktree of a repository you already trust, send it one message, stop it with `x`, then remove the worktree's folder outside Cader. Verify these, then delete the session with `d`:
  - **The row:** it reads `folder gone:`, and the key bar offers only `d` for it.
  - **The tips:** hovering the row shows its tip, and the REPOSITORY header's tooltip names the mark.
  - **The keys:** Enter and `r` refuse with the status line, and `R` leaves the row out.
  - **The log:** after two minutes, `cader.log` holds no `not started git` line for that folder.

## 5. Documentation and release

- [ ] 5.1 Update the REPOSITORY line in `docs\user-guide.md`'s sessions table to name the folder gone mark, and say in its keys section that Enter, rename and resume-all leave such a session alone and `d` deletes it. Verify the wording against `Wording.RepositoryColumn` and the status message.
- [ ] 5.2 Raise `Version` in `src\Source191.Cader\Source191.Cader.csproj` with `.github\scripts\Step-Version.ps1` (a preview raises its prerelease number). Add one `CHANGELOG.md` line under `## Unreleased`, `### Fixed`, ending with the pull request's number: a session whose folder has been deleted now reads folder gone, with `d` to delete it, and no longer fills the diagnostic log with git lines every 30 seconds. Verify that `Assert-VersionBumped.ps1 -Base main -Head HEAD` and `Assert-ChangelogUpdated.ps1 -Base main -Head HEAD` pass.
- [ ] 5.3 Run `openspec validate mark-sessions-whose-folder-is-gone --strict` and verify it passes.

## Workflow follow-up

- Archive the change in the same pull request once its tasks are done, so `openspec\specs` matches `main`.
- The parallel `make-the-diagnostic-log-useful` change bounds any remaining failed git start to one line per command and folder; check after both merge that a deleted folder leaves no line at all.
