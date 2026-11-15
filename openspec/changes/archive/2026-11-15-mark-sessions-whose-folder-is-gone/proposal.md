# Proposal

## Why

`claude agents --json --all` keeps listing a session after its folder has been deleted (most often a worktree removed by hand or by another tool). For each such session the Sessions tab asks git about the folder every 30 seconds, Windows refuses to start git in a folder that does not exist, and the diagnostic log records `not started git rev-parse` each time. In one user's `cader.log` this was 8,500 of 12,431 lines (68%), three lines every 30 seconds for as long as the console runs, and the 500-event in-memory log fills in under 90 minutes, pushing out the events a problem report needs. The row itself gives no hint that anything is wrong: it shows the folder's label as if the session were simply outside a repository, and Enter or resume-all would open a tab for a folder that has gone.

## What Changes

- Before asking git about a session's folder, the Sessions tab checks that the folder exists. A session whose folder has gone gets no git process at all, so no log line, on every refresh and when it is selected for the detail pane.
- Such a row is marked: its REPOSITORY cell reads `folder gone:` before the folder's label, the detail pane's Worktree and Changes read folder gone, and resting the mouse on the row shows a tooltip saying the folder no longer exists and that `d` deletes the session.
- No key resumes such a session, because each removes the job before resuming it and the resume cannot land in a missing folder: Enter on one without a process, and `r` on any, open nothing and the status line says the folder no longer exists and that `d` deletes the session; resume-all leaves such rows out; the key bar offers only `d` for one without a process. A running session (a drive gone offline) still switches to its tab on Enter.
- The REPOSITORY column's help and the user guide say what the mark means.

Out of scope:
- The diagnostic log's format and the once-per-command-and-folder recording of a failed start with its Windows error number (the parallel change `make-the-diagnostic-log-useful`); this change removes the commonest cause, that one makes any remaining one cheap.
- Checking for a missing folder inside `GitCli` for every caller (design.md, Decision 2), worktree scan speed, and clearing the log from the Config tab.
- Deleting such sessions automatically, or any change to what `d`, `x`, `b`, `c`, `f`, `s`, `p` and `m` do (`d` already copes, and the folder keys already refuse a folder that no longer exists).
- Multiple Claude accounts, which is a later change.

## Capabilities

### New Capabilities

(none)

### Modified Capabilities

- `session-listing`: a session whose folder no longer exists is marked in its row, its detail pane and a row tooltip, and starts no git process; the column-header tooltip requirement makes room for that one row tip.
- `session-attach`: Enter (without a process) and rename refuse a session whose folder no longer exists, the key bar offers only delete for it, and resume-all leaves it out.

## Impact

- `Source191.Cader.Core`: a third `SessionLocation` case, `FolderGone`, decided in `SessionRowEnricher` (which gains an `IFolders`) before the git cache; `SessionRow.Repository`, `DetailLoader`, `WordingOfDetail`, `SessionsView.Opening`, `Rename` and `ResumeAll` and `Wording.SessionsBar` handle it; a new `StatusMessage.FolderGone` with its words and tone; the cell, row-tip and column-help words in `WordingOfScreens`.
- `Source191.Cader.Tui`: `SessionsScreenDefinition` marks the REPOSITORY cell and answers `RowTip`.
- Wiring: `ConsoleComposition.Loads` and `tools\ReadmeScene\SceneHost` pass their `Folders` to `SessionRowEnricher`.
- `docs\user-guide.md`, `CHANGELOG.md`, and a version raise in `src\Source191.Cader\Source191.Cader.csproj` (new behaviour; while the version is a preview, `Step-Version.ps1` raises the prerelease number).
