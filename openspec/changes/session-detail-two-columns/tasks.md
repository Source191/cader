# Tasks

## 1. Set up

- [x] 1.1 Raise the version with `.github\scripts\Step-Version.ps1 -Preview` and add a Changed line to `CHANGELOG.md`; verify `Assert-VersionBumped.ps1 -Base main -Head HEAD` passes.

## 2. The session's facts in two columns of five rows

- [x] 2.1 Red: `WordingOfDetailShould.DescribeASessionAtAPermissionPrompt` expects five rows (Waiting and Started, Folder and Prompted, Worktree and Session, Branch and Tab, Changes and Memory, every left fact two columns wide), and `DetailSourcesShould` and the fact-row count fact expect five rows for a session and four for a worktree; verify they fail on the old arrangement.
- [x] 2.2 Green: `Wording.Detail(SessionDetail ...)` builds the five rows, `Wording.SessionFactRows` (5) and `Wording.WorktreeFactRows` (4) replace `DetailFactRows`, and `SessionDetails.Rows` answers five; verify the Core suite passes.

## 3. Documents

- [x] 3.1 The user guide's Sessions detail pane sentence and `console-screen.md`'s Detail pane section name the new arrangement.

## 4. Prove

- [x] 4.1 Run `dotnet build -c Debug`, `dotnet test -c Debug --no-build` and `dotnet format --verify-no-changes`; verify all pass and `openspec validate session-detail-two-columns` passes.

## Workflow follow-up

- Archive the change in the pull request that completes it.
