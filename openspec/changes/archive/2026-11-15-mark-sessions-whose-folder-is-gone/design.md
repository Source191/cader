# Design

## Context

See proposal.md, Why. The facts that shape the approach:

- **Where git is asked:** `Application\SessionRowEnricher.LocateAsync` asks `IGit.RepositoryRootAsync` (then `MainCheckoutOfAsync` and `CurrentBranchAsync`) per working directory, cached for 30 s (`CacheWindow`). `Detail\DetailLoader.SessionAsync` asks `WorktreeInspector.CountAsync` for the selected row unless its location `is SessionLocation.Outside`.
- **Why it is logged:** `Adapters\Git\GitCli.RunAsync` catches the `Win32Exception` with `ERROR_DIRECTORY` (267) and answers exit -1, so behaviour is already right (the row is `Outside` under the folder's name). But `Application\Diagnostics\RecordedProcessRunner` wraps the one process runner and records `ProcessFailedToStart` before `GitCli` sees the exception.
- **The model:** `Application\Views\SessionLocation` is an abstract record with two cases, `Outside(string Folder)` and `InRepository(string Name, FolderPath Checkout)`. Core is held at C# 14, so its three switches (`SessionRow.Repository`, `WordingOfDetail.PlaceOf`, `Tui\SessionsScreenDefinition.Repository`) end in a discard that throws `UnreachableException`: a new case compiles and throws at run time until each switch names it.
- **Existence is already a port:** `Ports\IFolders.DirectoryExists`, implemented by `Adapters\Folders` and faked by `TestDoubles\FakeFolders`. Both composition roots (`ConsoleComposition.Loads` and `tools\ReadmeScene\SceneHost`) already hold a `Folders`. The scene builds real repositories, so its sessions' folders exist.
- **Row tooltips exist:** `IScreenDefinition.RowTip` (empty by default) is drawn by `TableToolTips` over the row, as the Config tab's descriptions are.
- **Delete already copes:** `d` on a background row runs `DeleteSessionHandler`, which checks the created worktree's folder with `IFolders` and passes `folderExists: false` to `WorktreeInspector.HealthAsync`, which asks git only in the repository. The folder keys already refuse a folder that no longer exists (`OpenInEditorOutcome.FolderMissing`).

## Goals / Non-Goals

**Goals:**
- No git process, and so no diagnostic line, for a session whose folder has gone, on the refresh and in the detail pane.
- One model value, decided once, that every screen reads.

**Non-Goals:**
- Changing `RecordedProcessRunner` or the log's format (the parallel `make-the-diagnostic-log-useful` change).
- A missing-folder check inside `GitCli` (Decision 2).
- Verifying what Windows Terminal or `claude --bg --resume` do with a missing folder; the refusal in Decision 5 does not depend on it.

## Decisions

### 1. A third location case, `SessionLocation.FolderGone(string Folder)`, decided before the cache

`SessionRowEnricher` takes an `IFolders` and, in `LocateAsync`, asks `DirectoryExists(directory.Value)` first, on every call and before the cache is read. A missing folder answers `FolderGone(directory.Name)` with `Head.Unknown` and is not cached, so a restored folder is read by git on the next refresh, and a folder deleted inside the 30 s window is marked at once rather than when the cached answer expires. `Folder` mirrors `Outside.Folder`, so `SessionRow.Repository` answers the folder's name and the filter keeps matching it.

- **Alternative, a flag on `SessionRow`:** rejected. Every switch over `Location` would still need to know, and the house rule is a case, not a flag.
- **Alternative, `Outside` with a reason:** rejected. `DetailLoader`'s and the pane's `Outside` handling would then need a second test everywhere, and "not in a repository" would be wrong for it.
- **Cost:** one file-system stat per session per refresh (every 5 s), against three process starts per 30 s today.

### 2. The check lives in the enricher, not in `GitCli`

Putting `Directory.Exists` before every start in `GitCli` would cover every caller. It was rejected for this change:

- **Callers:** the log shows that the enricher's `rev-parse` (three lines per refresh window, per session) is the whole of the noise. The detail pane is covered by Decision 3. The worktree and delete paths already check folders through `IFolders` before asking git about them.
- **Tests:** `GitCli` takes only a process runner and an executable. Sixteen test files build `new GitCli(fakeRunner, "git")` over made-up `C:\repos\...` paths, which a real `Directory.Exists` would call missing. Giving `GitCli` an `IFolders` would change every construction and every fixture for no observed gain.
- **The marking:** the row needs to know the folder has gone anyway. A guard in `GitCli` would still answer exit -1, which the enricher cannot tell from "outside a repository".

`GitCli`'s `ERROR_DIRECTORY` catch stays, for a folder deleted between the check and the start. Any rarer case that remains is bounded by the parallel change, which records a failed start once per command and folder.

### 3. The detail pane asks nothing for a gone folder

`DetailLoader.SessionAsync` answers `DetailFact<WorktreeCounts>.NotApplicable` for `FolderGone` as for `Outside`. The pane's words come from the location: `PlaceOf` answers "folder gone" and the Changes line reads "folder gone" for a `FolderGone` row, never "not in a repository".

### 4. The mark: a prefix in the REPOSITORY cell, a row tip and the column's help

- **The cell:** `SessionsScreenDefinition.Repository` answers `WhereLabel.Outside(Wording.FolderGone(label))`, the words `folder gone: ` before the folder's label. `Outside` cuts at its end, so the prefix survives a narrow column where a suffix would be cut off.
- **The row tip:** `SessionsScreenDefinition.RowTip` answers `Wording.SessionRowTip(row)`, empty for every row except a gone folder's. A background session's tip names `d`. An interactive one's says only that the folder has gone, because `d` refuses a terminal session that has a conversation. An interactive session always has a running process, and Windows is expected to refuse to delete a folder that a running process uses as its current directory, so the case should be rare. This was read, not observed.
- **The help:** `Wording.RepositoryColumn`'s help gains the mark, so the help dialog and the header tooltip say it, and `docs\user-guide.md`'s REPOSITORY line follows.
- **No colour change:** the row keeps its state's colour (`RowColour`), so the STATE rules are untouched.

### 5. No key resumes a session whose folder has gone

Enter on a session without a process, `r` and `R` each run `claude rm` and then resume, and a resume in a missing folder would leave the job removed from the listing (rename's fallback resumes under the old name in the same folder, so it fails too). One status message, `StatusMessage.FolderGone(ListedName Name)`, with the words "<name>'s folder no longer exists, so nothing was resumed. d deletes it." and a Notice tone like `NeverSentAMessage`, explains each refusal.

- **Enter:** `SessionsView.Opening` gains a first arm for a `FolderGone` row whose session is a `Background` with `JobProcess.None`. A running session (a drive that has gone offline, say) still attaches or switches to its tab as today, so the open-in-tab and live-tab requirements are untouched. `ConsoleView.OpenSession` uses the same decision, so the Worktrees tab's Enter agrees.
- **Rename:** `SessionsView.Rename` refuses any `FolderGone` background row with the same message, before its other rules.
- **Resume-all:** `ResumeAll` filters `FolderGone` rows out before choosing the jobs. When only such rows are shown, the existing "nothing to resume" message answers.
- **Key bar:** `Wording.SessionsBar` gains an arm before the background ones. A `FolderGone` background row without a process offers only `d`. A running one keeps its keys, and `r` on it explains.
- **Alternative, leave both keys alone:** rejected. The mark would point at a row whose Enter opens a broken tab, and whose resume-all could lose the listed job.
- **Alternative, open the tab somewhere else:** rejected. It is unverified, and `claude --bg --resume` finds the transcript under a folder named after the working directory.

## Risks / Trade-offs

- [A folder on a drive that is offline (a disconnected network share or a removed disc) reads as gone] → The mark is what the user would want to see. The row comes back by itself once the drive returns, because the answer is never cached.
- [The new case compiles before every switch names it] → The tasks name each switch with its test. `SessionsViewShould` (the filter, through `SessionRow.Repository`), `WordingOfDetailShould`, `SessionsScreenDefinitionShould` and `DetailLoaderShould` each get a `FolderGone` fact.
- [A tooltip on rows breaks "nothing on a row" for the Sessions tab] → It is limited to the one case and written into the column-headers requirement.

## Migration Plan

None: nothing is stored. Rolling back restores today's rows and log lines.
