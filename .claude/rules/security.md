---
paths:
  - "src/Source191.Cader.Infrastructure/Processes/**"
  - "src/Source191.Cader.Infrastructure/PullRequests/**"
  - "src/Source191.Cader.Infrastructure/Terminal/**"
  - "src/Source191.Cader.Infrastructure/Diagnostics/**"
  - "src/Source191.Cader.Core/Application/Diagnostics/**"
  - "src/Source191.Cader.Core/Application/Reporting/**"
  - "**/TabScripts*.cs"
  - "**/PowerShellScript*.cs"
  - "**/BatchFileArgument*.cs"
  - "**/RemoteRepository*.cs"
  - "**/Editor*.cs"
  - "**/WorktreeRemover*.cs"
  - "**/LeftoverFiles*.cs"
  - "**/RemoveWorktreeHandler*.cs"
  - "**/DeleteSessionHandler*.cs"
  - "**/ClearDiagnosticLog*.cs"
  - "**/SessionId*.cs"
  - "**/ConfirmedProcess*.cs"
  - "**/ProgramLabel*.cs"
  - "**/PathFolders*.cs"
  - "**/ProgramLocat*.cs"
  - "**/*Executable*.cs"
  - "**/SettingsProblem*.cs"
  - "**/LaunchPlanBuilder*.cs"
  - "**/HereCommand*.cs"
  - "**/GitPointer*.cs"
  - "**/WorktreeHealth*.cs"
  - "**/WorktreeInventory*.cs"
  - "**/TerminalTab*.cs"
  - "**/TabRegistration*.cs"
  - "**/*Privacy*.cs"
---

# Security and diagnostics

The detail behind the security and privacy invariants in `CLAUDE.md`.

## Programs and arguments

- **Full paths only:** every program Cader starts is found once as it starts and run by its full path. `Adapters\Processes\ProgramLocator` searches the PATH's full-path folders only (`PathFolders.Searchable`, the one filter every PATH search applies, az.cmd's and the VS Code shim's included) for the `.exe` only.
- **Why:** CreateProcess would otherwise look in the parent's current directory before the PATH for a bare name, so a `git.exe` planted where Cader was started would run (`ProgramLocatorShould`; security review of 2026-10-29).
- **Claude and git:** `ClaudeExecutable.Locate` answers a `ClaudeLocation` (`Found`, `BatchFileOnly`, `Missing`); `Application\RequiredPrograms.Check` turns it and git's location into both paths or `Wording.ClaudeIsABatchFile`, `ClaudeNotFound` or `GitNotFound` (Claude Code's first), and `Composition\LaunchParts` prints it and exits 1.
- **Other programs:** `ProgramLocator.Locate` answers a `Ports\ProgramLocation` (`Found`, `Missing`). `GitHubPullRequests`, `AzureDevOpsPullRequests` (through `AzureCliExecutable.Locate`), `WindowsTerminal` (wt.exe and the tab's pwsh.exe, named by full path in `WindowsTerminalArguments`) and `Desktop` take one and report a `Missing` one where it is needed.
- **Editors:** a folder reaches an editor as one element of an argument list built by `EditorCommand`, never through a command line or cmd.exe, and the locator never answers a `.cmd` shim. `IEditorLocator` finds a preset's real executable as a `ProgramLocation` (Visual Studio's through `vswhere -latest -property productPath`, run by its full path).
- **Opening files:** `IDesktop.OpenAsync` opens a file with its associated application; it throws `NoApplicationException` for a file type with none (never for an address) and `ProgramNotStartedException` when PowerShell or the script fails. The two handlers that open an address catch the latter into `NotOpened`; the others let it reach the screen as `Failed:`.
- **Starting programs:** `IDesktop.StartAsync` starts a program detached through ShellExecute from the same cleaned `pwsh` child, executable and arguments as Base64 (`StartProgramScript`), so it inherits none of the console's handles or Claude Code markers; a refusal is reported by Windows' sentence for its native error code alone.
- **No batch file is given outside text:** never text from a person, a repository or a listing. A custom editor's executable must be a full path (`SettingsProblem.ExecutableNotFullPath`).
- **`Domain\BatchFileArgument.IsSafe`** (letters, digits, space, dot, underscore, hyphen, slash) is the one rule for what `az.cmd` may be given, applied to the remote names in `RemoteRepository` and the branch in `AzureDevOpsPullRequests` (`PullRequestGap.BranchNotGivenToAz`).
- **Pull request heads:** a head commit comes from the host's listing and reaches git only through `Domain\CommitId.TryParse` (40 or 64 hex digits). `GitHubPullRequests` reads `headRefOid` from `gh pr list --json`; a missing, blank or unreadable one is `HeadCommit.NotRecorded` and the pull request is still listed.
- **Azure DevOps heads:** `AzureDevOpsPullRequests` reads `lastMergeSourceCommit.commitId`, documented as "the commit at the head of the source branch at the time of the last pull request merge" ([GitPullRequest](https://learn.microsoft.com/javascript/api/azure-devops-extension-api/gitpullrequest), 15 November 2026). Read from the documentation, not yet seen on a real repository (track-branches task 6.3).
- **PowerShell source:** user text is never interpolated into it. A tab's script reaches `wt.exe` Base64-encoded for [`pwsh -EncodedCommand`](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_pwsh), and every value inside (paths, ids, folder names, messages) is built by `TabScripts` through `PowerShellScript.Quote`.
- **Quoting:** `Quote` doubles all five characters [PowerShell reads as a single quote](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_quoting_rules) (`'` and U+2018 to U+201B); `PowerShellQuotingShould` checks it against real `pwsh`. Every semicolon reaching `wt.exe` is escaped by `WindowsTerminalArguments` (`windows.md`).
- **Prompts:** an opening prompt that starts with `-` is `LaunchProblem.PromptLooksLikeAnOption`.
- **Release notes** come from outside the program (the changelog at a release's tag, the one beside the program): `Domain\ReleaseNotes.TryParse` drops every control character and keeps only headings and one-line entries, shown as text; the release page's URL is built from the version alone (`Application\ReleasePage`). Versions are winget-pkgs' folder names that parse as stable.

## Windows Terminal's settings.json

- **Changed only by** `Adapters\Terminal\TerminalSettings`, through one guarded write, only after `cader setup-terminal` has shown the entries and the user agreed (or passed `--yes`). Its `state.json` is only ever read.
- **Only two edits:** add the entry of a Cader profile Terminal hides as deleted (`HiddenTerminalProfiles.Of`), or with `--remove` take out an entry that is exactly the four-property entry setup adds (`TerminalProfileRecords.Read.Added`); an entry with any other property is the user's and stays.
- **Refused files:** a symbolic link or other reparse point (replacing the file would replace the link) and a read-only file, each a `TerminalSettingsEdit` case; setup then gives the entries to change by hand.
- **Only the profile list's text changes:** every other byte (comments, a byte order mark) stays. It re-parses the new bytes with the same comment-tolerant reader and writes nothing unless the list is exactly the original elements plus the added ones (or minus the removed), and unless the file as read holds the list the text walker found (a file naming `profiles` twice does not).
- **The write:** it writes beside the file, reads the file again and writes nothing when its bytes changed (most likely Terminal saving; a save between that read and the replace is still lost, as Terminal does not lock the file), then puts the new file in place with `File.Replace`, keeping the original as `settings.json.<yyyy-MM-ddTHH-mm-ss>.cader.backup` (UTC), which setup names.

## Processes and identities

- **Ending a process:** only as a `Domain\ConfirmedProcess`, which `IProcessRegistry.Terminate` and `EndCleanly` require and only three doors make.
- **The doors:** `TryConfirm` for a listed id (started no later than the session's listed start plus five seconds; close-empty lists the others in `Gone` and adopt answers `ProcessGone`); `TryConfirmClaim` for a tab's shell (the recorded start exactly, as `TabRegistration.TryConfirmShell` and `IsLive` check); `TryConfirmParent` for a step up to Windows Terminal (a parent started no later than its confirmed child).
- **Finding it:** `ProcessRegistry.WithProcess` uses .NET 11's `Process.TryGetProcessById`, which answers false for a gone id; a protected process refuses the action with a `Win32Exception`, which stays caught. An id of 0 or less never reaches it, because each door needs a start time the registry read.
- **No `KillOnParentExit`:** its job would end, with the console, the programs Cader starts to outlive it (`windows-processes.md`, 14 November 2026).
- **Command lines carry secrets,** so `ProcessRegistry.UsageOf` reduces one with `ProgramLabel` to a package or executable name and answers only that `ProcessUsage.Label`; the command line never leaves the method.
- **Session ids:** a `SessionId` is ASCII letters, digits and hyphens with a letter or digit first, so no listed id can be read as an option or hold a path separator. `TerminalTab` walks up only through parents `TryConfirmParent` accepts, so a reused parent id never names another tab's shell.

## Deleting folders

- **Only through `IFolderDeleter`,** which only `WorktreeRemover` holds (`LayeringShould.HandTheFolderDeleterOnlyToWorktreeRemover`), only strictly inside a repository root or the worktree root (`RemovalReason.OutsideTheRoots`, never forced). `RemoveWorktreeHandler` and `DeleteSessionHandler` read a leftover's existence and files (`Application\LeftoverFiles`) from the disc as the key is pressed.
- **Live checkouts are never leftovers:** a worktree whose `.git` file points at a live registration of a repository outside the roots is `WorktreeHealthKind.Elsewhere` (`HealthReason.RepositoryOutsideRoots`), a real worktree to the keys but refused removal (`RemovalReason.RepositoryOutsideRoots`).
- So is any `.git` file whose target exists (a bare repository's worktree, a `--separate-git-dir` checkout), its repository read from the gitdir's shape and a relative gitdir from the `.git` file's folder.

## Diagnostics

- **The log:** `Application\Diagnostics` holds one `DiagnosticLog`, the latest 500 events in memory, each also written to `%LOCALAPPDATA%\Cader\cader.log` by `Adapters\Diagnostics\DiagnosticFile` while the `diagnosticLog` setting is on (its `DiagnosticLogSwitch` asked on every line), rolling to `cader.log.1` at 10 MiB. `CrashRecorder` writes `last-crash.log`.
- **The counts:** `ProcessCounts` (`RecordedProcessRunner` records a program that cannot start once per command and folder, the folder only an in-memory key), `RefreshCounts`, `StallCounts` (`Heartbeat`'s freeze line names the longest `Heartbeat.Mark` as a `CodeName` and a duration) and `OwnProcessMeter`, in one `PerformanceCounts`, written every 10 minutes by `PeriodicCounts` and at exit (`docs\performance-monitoring.md`).
- **Clearing:** the log files are deleted only by `IDiagnosticLogFiles.Clear` (`DiagnosticLogFiles`), which takes no path and deletes only `cader.log` then `cader.log.1` at the path it was built with (`SharedParts`: `%LOCALAPPDATA%\Cader`; `SceneHost`: its scene folder). A file another program holds open is kept and counted.
- Only `ClearDiagnosticLogHandler` calls it, after `d` and its confirmation, recording `DiagnosticLogCleared` (files, bytes and files kept, never a path).
- **A new fact** is a new `DiagnosticEvent` case of numbers, durations, versions, enums, `CodeName` (only from a `Type` or `MethodBase`), `ExceptionSummary` (types, a `Win32Exception`'s error number and frames, never the message) or `ChildCommand` (words from `ChildCommandWords` only); `DiagnosticEventsShould` enforces it.
- **Privacy:** `DiagnosticsPrivacyShould` runs the daily flows over Northwind fixtures and fails on any fixture value in the output. Never log an effect's `ToString()`, an exception's message, program output or anything typed.
- **The rendered lines** (`DiagnosticLine`) are the one text outside `Wording`, a maintainer's format. Each line, in the file, the crash file and a problem report, carries Cader's own process id after its UTC time (`Environment.ProcessId`), the one id a line may hold, so several Cader processes sharing `cader.log` can be told apart.
- **Terminal.Gui's logger** is `TerminalGuiBridge`, which keeps only the level, the code name of the raising method and an exception summary, Warning and above, because its messages can quote typed or pasted input.
