# worktree-management Specification

## Purpose
Shows every git worktree across the repository roots with its health and lets the user remove stale ones without losing uncommitted or unpushed work.

## Requirements

### Requirement: Worktrees are listed across all roots
The worktrees view SHALL list every linked worktree of every git repository found directly under the configured roots, wherever the worktree's folder is (under the worktree root, beside the repository or elsewhere), using git's machine-readable worktree listing, each as a row under its repository's heading whose WORKTREE cell names the folder, with its branch in the detail pane and its branch's flag in BRANCH.

#### Scenario: Two repositories with worktrees
- **WHEN** two repositories under the roots each have one linked worktree
- **THEN** the view shows two headings, each naming its repository, and under each one row whose WORKTREE cell reads its folder's slug, with its branch on the pane's Branch line when selected

#### Scenario: Old and new placements together
- **WHEN** Northwind.Orders has one worktree beside it from an earlier version and one under C:\Users\me\source\worktrees
- **THEN** the view shows both rows under the Northwind.Orders heading, each reading its slug

### Requirement: Leftover folders are detected
The view SHALL also show folders that git no longer recognises: a folder under a repository's .claude\worktrees that the listing omits is an orphan; a folder directly under a root or under the worktree root whose .git file points at a worktree registration that no longer exists is broken; an empty folder named with a .worktrees suffix directly under a root, and any empty folder directly under the worktree root, is empty.

#### Scenario: Orphan folder
- **WHEN** a folder exists under .claude\worktrees but is absent from git's listing
- **THEN** the view shows it with health orphan

#### Scenario: Broken registration
- **WHEN** a sibling folder's .git file names a worktree registration whose repository no longer exists
- **THEN** the view shows it with health broken

#### Scenario: Broken registration under the worktree root
- **WHEN** a folder under C:\Users\me\source\worktrees has a .git file naming a worktree registration that no repository under the roots lists
- **THEN** the view shows it with health broken

#### Scenario: Empty leftover
- **WHEN** an empty folder named Northwind.Orders.worktrees sits directly under a root
- **THEN** the view shows it with health empty

#### Scenario: Empty folder under the worktree root
- **WHEN** an empty folder C:\Users\me\source\worktrees\Northwind.Orders-fix-123 sits directly under the worktree root C:\Users\me\source\worktrees, which is not a repository root
- **THEN** the view shows it with health empty, under a heading naming the worktree root, its WORKTREE cell naming the folder, and the remove key deletes it without asking

#### Scenario: A folder under the worktree root that holds files but no .git file
- **WHEN** C:\Users\me\source\worktrees\Northwind.Notes holds a file and no .git file
- **THEN** the view does not show it

#### Scenario: Worktree of a repository outside the roots
- **WHEN** a folder under C:\Users\me\source\worktrees has a .git file naming the registration D:\other\Northwind.Ledger\.git\worktrees\fix, that registration exists, D:\other\Northwind.Ledger\.git exists, and D:\other is not a root
- **THEN** the view shows it with health elsewhere under the Northwind.Ledger heading, and the status line says its repository D:\other\Northwind.Ledger is not under the roots

### Requirement: Locked worktrees are marked
A worktree that git reports as locked SHALL show health locked, its HEALTH cell reading ◆ locked and its row in amber, and the detail pane's Health line SHALL show the lock reason when git provides one.

#### Scenario: Locked by another tool
- **WHEN** git reports a worktree as locked and it is selected
- **THEN** its row is amber, its HEALTH cell reads ◆ locked, and the pane reads locked and the lock reason

### Requirement: Each row shows work at risk
For every registered worktree the view SHALL show the age of the last commit and the name of any session whose working directory is that worktree in its row, and the count of changed or untracked files and the count of commits not on any remote branch in the detail pane's Changes line when the row is selected.

#### Scenario: Dirty worktree
- **WHEN** a worktree has five changed files and two unpushed commits and is selected
- **THEN** the pane's Changes line reads 5 changed files · 2 unpushed commits, in amber

### Requirement: Removal refuses to destroy work
Removal SHALL be refused when the worktree has changed or untracked files, unpushed commits, a lock, or a live session inside it, and the refusal SHALL name the reason.

#### Scenario: Unpushed commits
- **WHEN** the user presses remove on a worktree with unpushed commits
- **THEN** nothing is removed and the status line names the branch and the number of unpushed commits

#### Scenario: Clean worktree
- **WHEN** the user presses remove on a worktree with no changes, no unpushed commits, no lock and no session
- **THEN** the worktree is removed from git and from disc

### Requirement: Removal copes with a folder still in use
When git cannot delete the folder because a tab or program still has it as its working directory, git has already removed the worktree's registration and contents, so Cader SHALL try to delete what is left for about five seconds, then finish the removal as for any other (the branch dealt with as the branch rules say) whether or not the folder went, and SHALL name a folder it had to leave.

#### Scenario: Folder released while Cader waits
- **WHEN** the user presses remove on a clean worktree whose folder a closing program releases two seconds after git fails to delete it
- **THEN** the folder is gone, the branch is dealt with as for any removal, and the status line says the worktree was removed and does not mention a folder in use

#### Scenario: Folder in use by a shell tab
- **WHEN** the user presses remove on a clean worktree Northwind.Orders-fix-123 under the worktree root while a PowerShell tab opened from the worktrees view sits in it
- **THEN** after about five seconds the status line says Northwind.Orders-fix-123 was removed but its folder was left because a tab or program still has it open, the branch is dealt with as for any removal, and the worktrees view lists the empty folder with health empty, which the remove key deletes once that tab is closed

#### Scenario: Git keeps the registration
- **WHEN** git fails to remove a clean worktree with "Permission denied" but still lists it as a worktree of its repository
- **THEN** nothing more is deleted, the branch and Cader's record of the worktree are left as they were, and the status line says the folder is still in use by a tab or program and to close it and remove again

#### Scenario: Branch of a worktree whose folder stayed in use
- **WHEN** the worktree whose folder stayed in use was created by Cader on the branch fix-123, which is merged
- **THEN** the branch fix-123 is deleted, and the status line says so as well as naming the folder that was left

### Requirement: Forced removal needs a typed confirmation
The force-remove key SHALL remove a worktree despite changes or unpushed commits only after the user enters the worktree's folder name to confirm (shown selected in the dialog so it can be copied and pasted; a wrong or empty answer keeps the dialog open and says what was typed), and SHALL never force-remove a locked worktree or one with a live session. A refusal that cannot be forced SHALL NOT suggest force-remove in the status line.

#### Scenario: Confirmed force
- **WHEN** the user presses force-remove and enters the exact folder name
- **THEN** the worktree is removed along with its changes

#### Scenario: Wrong answer
- **WHEN** the user enters anything else
- **THEN** nothing is removed, the dialog stays open and says what was typed, and Esc keeps the worktree

#### Scenario: Live session inside
- **WHEN** the user presses force-remove on a worktree that a live session is using
- **THEN** removal is refused and the status line names the session

#### Scenario: Refusal that cannot be forced
- **WHEN** plain remove is refused because the worktree is locked or a live session is using it
- **THEN** the status line names the reason and does not mention force-remove, whatever words the reason uses

### Requirement: Branch deletion follows ownership and merge state
After removing a worktree, Cader SHALL delete its branch automatically only when Cader created that branch and it is fully merged into the remote default branch, and SHALL otherwise ask before deleting. A worktree whose head is detached, or whose head git did not name, has no branch of its own: it SHALL be removed with no branch deleted and no question asked, and its folder's name SHALL never be taken for a branch.

#### Scenario: Cader-created merged branch
- **WHEN** the removed worktree's branch was created by Cader and is merged
- **THEN** the branch is deleted without asking

#### Scenario: Hand-made branch
- **WHEN** the removed worktree's branch was not created by Cader
- **THEN** Cader asks whether to delete the branch and keeps it unless the user agrees

#### Scenario: Detached head
- **WHEN** the user removes a clean worktree at C:\Users\me\source\repos\Northwind.Orders-fix-123 that git lists on a detached head, and a branch named Northwind.Orders-fix-123 exists in the repository
- **THEN** the worktree is removed, no branch is deleted, no question about a branch is asked, and the status line says the worktree was removed

#### Scenario: Head git could not name
- **WHEN** the user removes a clean worktree whose head git did not report
- **THEN** the worktree is removed and no branch is asked about or deleted

### Requirement: Leftover folders can be deleted as plain folders
For rows with health orphan, broken or empty, the remove key SHALL delete the folder from disc without git, and for broken rows SHALL also prune the stale registration from the owning repository when that repository still exists. When the folder cannot be deleted because a tab or program still has it open, the removal SHALL report the folder as in use, naming it, in the same words as a worktree git could not delete, and SHALL prune nothing.

#### Scenario: Delete empty leftover
- **WHEN** the user presses remove on an empty row
- **THEN** the folder is deleted and the row disappears

#### Scenario: Leftover folder with files
- **WHEN** the user presses remove on an orphan folder that still holds twelve files
- **THEN** removal is refused and the status line says twelve untracked files would be lost, and force-remove with the typed confirmation deletes it

#### Scenario: Leftover folder held open
- **WHEN** the user presses remove on an empty broken folder C:\Users\me\source\worktrees\Northwind.Orders-stale while a PowerShell tab sits in it, so the folder cannot be deleted
- **THEN** the status line says the folder Northwind.Orders-stale is still in use by a tab or program and to close it and remove again, no registration is pruned, and no exception message is shown

### Requirement: Stale registrations can be pruned
The prune key SHALL run git's worktree prune for the selected row's repository and report how many registrations were removed.

#### Scenario: Prune
- **WHEN** a repository has a registration whose folder is gone and the user presses prune
- **THEN** the registration is removed and the status line reports one pruned

### Requirement: A session or shell can be started in a worktree
From a worktree row whose folder exists and whose health is ok, locked or elsewhere, the user SHALL be able to start a new session in that worktree through the launch wizard with the directory preselected, with n, or open a plain PowerShell tab there, with o.

#### Scenario: Start session here
- **WHEN** the user presses n on a worktree row
- **THEN** the launch wizard opens with that worktree as the chosen directory and no new worktree option

#### Scenario: Shell in an elsewhere worktree
- **WHEN** the user presses the open-shell key on an elsewhere row
- **THEN** a PowerShell tab opens in that folder

### Requirement: Scanning is visible
The worktrees view SHALL show a spinner with the words "Scanning worktrees" in the middle of the table while it is shown and no scan has yet returned, and SHALL show a spinner beside the status line's working text while an action started from the view runs; neither spinner SHALL appear for the rescan when the tab is shown again or for the timed rescans. Typing a filter, Backspace or Escape before the first scan SHALL NOT end the waiting spinner.

#### Scenario: First scan pending
- **WHEN** the user opens the worktrees view and the scan of the repository roots has not yet returned
- **THEN** the table area shows a spinner and "Scanning worktrees", which disappear when the first rows arrive

#### Scenario: A filter typed before the first scan
- **WHEN** the worktrees view is shown, its first scan has not yet returned and the user presses / and types n
- **THEN** the table area still shows the spinner and "Scanning worktrees", and the title shows the filter being typed

#### Scenario: Rescan on show
- **WHEN** the user opens the worktrees view after its first scan has returned
- **THEN** the earlier rows show at once with no spinner while the rescan runs

#### Scenario: Action running
- **WHEN** the user removes a worktree
- **THEN** the status line shows a spinner before "Removing name" until the removal reports its outcome

### Requirement: Enter on a worktree row goes to its session by identity
Enter on a worktree row with a running session SHALL do what Enter does on that session's row on the Sessions tab, found by its session id so two sessions sharing a name cannot be confused: open the session in its own Windows Terminal tab, or switch to that tab when one is open, or say in the status line why it cannot. The Worktrees tab SHALL stay shown. With no session in the worktree, or one the sessions listing does not hold, the status line SHALL say so.

#### Scenario: Two sessions share a name
- **WHEN** two background sessions are both named fix-tests and one runs in the selected worktree
- **THEN** Enter opens or switches to the tab of the one whose working directory is that worktree

#### Scenario: The session's tab is open
- **WHEN** a Cader-opened tab for the worktree's background session is alive and the user presses Enter on the worktree row
- **THEN** Windows Terminal switches to that tab, no new tab opens, and the Worktrees tab stays shown in Cader

#### Scenario: The session has no tab
- **WHEN** the worktree's background session has no live tab and the user presses Enter on the worktree row
- **THEN** a new tab opens attached to that session, as Enter on the Sessions tab opens one

#### Scenario: A terminal session
- **WHEN** the session in the worktree is a plain claude running in its own terminal
- **THEN** nothing opens and the status line says, as on the Sessions tab, that it runs in its own terminal

#### Scenario: No session
- **WHEN** no session runs in the selected worktree and the user presses Enter
- **THEN** nothing opens and the status line says no session runs in that worktree

#### Scenario: Session gone from the listing
- **WHEN** the worktree's row names a session that the latest sessions listing no longer holds
- **THEN** nothing opens and the status line says the session is not listed any more

### Requirement: Counting a leftover folder's files is bounded
The worktree scan SHALL stop counting a leftover folder's files at a fixed limit, so a refresh never walks a whole dependencies tree, and a refusal for such a folder SHALL say that at least that many files would be lost.

#### Scenario: Large leftover
- **WHEN** an orphan folder holds many thousands of files under a dependencies folder
- **THEN** the refresh stops counting at the limit, and pressing remove says at least that many files would be lost

#### Scenario: Small leftover
- **WHEN** an orphan folder holds twelve files
- **THEN** pressing remove says twelve files would be lost

### Requirement: Deleting a leftover folder never follows links
When Cader deletes a leftover folder from disc, every junction or symbolic link inside it SHALL be removed as a link, without descending into it or changing anything it points at, and the rest of the folder SHALL then be deleted completely, including read-only files.

#### Scenario: Leftover folder holding a junction
- **WHEN** the user force-removes an orphan folder whose node_modules holds a junction to a folder outside it, and that outside folder holds a read-only file
- **THEN** the orphan folder is gone, and the outside folder and its file still exist and the file is still read-only

#### Scenario: Leftover folder with read-only git files
- **WHEN** the user removes a broken folder whose object files git left read-only
- **THEN** the folder is deleted completely

### Requirement: The worktrees table title counts rows and names the selection
The worktrees view's table title SHALL count the worktree rows and the branch and origin rows apart, leaving out a count of none, name the filter when one is set, and name the selected row's folder, or its branch when it has no folder.

#### Scenario: No filter
- **WHEN** twelve worktrees are listed with no filter and Northwind.Orders-fix-123 is selected
- **THEN** the table title reads 12 worktrees, selected: Northwind.Orders-fix-123

#### Scenario: With a filter
- **WHEN** a filter of "orp" matches one of twelve rows, the folder Northwind.Orders-orphaned
- **THEN** the table title reads 1 of 12 worktrees match "orp", selected: Northwind.Orders-orphaned

#### Scenario: Worktrees and branches
- **WHEN** ten worktrees, two branch rows and one origin row are listed with no filter and the branch row spike-refunds is selected
- **THEN** the table title reads 10 worktrees, 3 branches, selected: spike-refunds

### Requirement: The worktrees header and help match the sessions view
The worktrees view SHALL be the Worktrees tab of the console, under the same settings line as the Sessions tab, with its own counts beside the tabs and its own key bar grouped Worktree, Folder and view; its status line SHALL show only messages, never a standing hint or the selected row's health, which the detail pane shows.

#### Scenario: Worktrees help
- **WHEN** the user presses the help key on the Worktrees tab
- **THEN** the help names every column of the worktrees table with its meaning, and its Pane section names every line of the pane, including what each health word means

#### Scenario: Worktrees help on a short terminal
- **WHEN** the user presses the help key in the worktrees view on a 120 x 36 terminal
- **THEN** every line of the help is reachable with the arrow keys and PageDown

#### Scenario: Hover over a key hint
- **WHEN** the mouse pointer rests on P Prune in the key bar of the Worktrees tab
- **THEN** a tooltip says it prunes stale registrations in the row's repository

#### Scenario: An unhealthy row selected
- **WHEN** an elsewhere worktree is selected and no key has reported anything
- **THEN** the pane's Health line gives the reason and there is no status line

#### Scenario: Hover over a column header
- **WHEN** the mouse pointer rests on the LAST COMMIT header
- **THEN** a tooltip names the column and explains it

#### Scenario: The worktree bar
- **WHEN** an ok worktree with a session inside it is selected
- **THEN** the first bar row reads d Remove, D Force remove, P Prune, n New session, o Open shell and Enter Open session, and the second c Copy path, f Open folder, s Open solution, p Open PR, / Filter, ? Help, ! Report and q Quit

### Requirement: Deleting a leftover folder never deletes a root
When Cader deletes a leftover folder from disc, it SHALL refuse a path that is not fully qualified, or that is a drive root or a network share root, before changing anything. The status line SHALL report the refusal as a failure naming the path.

#### Scenario: A drive root reaches the deletion
- **WHEN** a leftover folder's path is C:\
- **THEN** nothing on the drive is deleted and the status line reports a failure naming C:\

#### Scenario: A share root or a relative path reaches the deletion
- **WHEN** a leftover folder's path is \\server\share\ or Northwind.Orders-fix
- **THEN** nothing is deleted and the status line reports a failure naming that path

#### Scenario: An ordinary leftover folder with a trailing separator
- **WHEN** the user removes an orphan folder whose path is written C:\Users\me\source\worktrees\Northwind.Orders-fix\
- **THEN** the folder is deleted as before

### Requirement: A worktree of a repository outside the roots is never deleted as a leftover
For a row with health elsewhere, the remove and force-remove keys SHALL refuse, even after a typed confirmation, with a reason saying the worktree's repository is not under the roots and to add that root in settings or remove the worktree with git there.

#### Scenario: Remove an elsewhere row
- **WHEN** the user presses remove or force-remove on an elsewhere row
- **THEN** nothing is deleted and the status line says the repository is not under the roots, without offering force-remove

### Requirement: One WORKTREE column names the worktree
The worktrees table SHALL show the folder in one WORKTREE column, under its repository's heading, as the folder's part after the repository (the slug of a folder named `<repository>-<slug>`, else the folder's label) without the repository's name, elastic in width and cut at its end when too long, followed by HEALTH, LAST COMMIT and SESSION.

#### Scenario: A worktree Cader made
- **WHEN** Northwind.Billing has the worktree C:\Users\me\source\worktrees\Northwind.Billing-invoice-pdf on branch invoice-pdf
- **THEN** its row is under the Northwind.Billing heading, its WORKTREE cell reads invoice-pdf, and the pane reads Branch invoice-pdf

#### Scenario: A leftover folder
- **WHEN** a broken leftover C:\Users\me\source\worktrees\Northwind.Sales-republish once belonged to Northwind.Sales
- **THEN** its row is under the Northwind.Sales heading, its WORKTREE cell reads republish, its HEALTH cell reads ✗ broken and its row is red

#### Scenario: A folder that does not follow the naming
- **WHEN** Northwind.Orders has a hand-made worktree at C:\Users\me\source\worktrees\spike on branch spike
- **THEN** its row is under the Northwind.Orders heading and its WORKTREE cell reads spike

#### Scenario: Filter by health
- **WHEN** the user filters by broken
- **THEN** the broken rows stay in the table under their headings, each with its HEALTH cell reading ✗ broken

#### Scenario: The help names the column
- **WHEN** the help is shown on the worktrees view
- **THEN** it lists WORKTREE with its meaning, saying the row's repository is its heading, and the header shows the same words on hover

### Requirement: A heading too long for the column loses its middle
A heading too long for the column SHALL lose its middle, marked with an ellipsis, and SHALL never widen a column.

#### Scenario: A long heading
- **WHEN** a repository's name is too long for the WORKTREE column
- **THEN** its heading loses its middle, marked with an ellipsis, and the column keeps its width

### Requirement: LAST COMMIT is right-aligned
LAST COMMIT SHALL be right-aligned, as AGE is on the Sessions tab, so its durations line up on their last character.

#### Scenario: Last commits line up at the right
- **WHEN** Northwind.Billing-invoice-pdf last committed five minutes ago, Northwind.Orders-fix-123 three days and four hours ago, and an empty leftover folder has no commit
- **THEN** the LAST COMMIT cells 5m, 3d4h and - each end at the column's right edge, and the WORKTREE cells start two cells in from its left edge, under their headings

### Requirement: A worktree of a repository outside the roots is elsewhere
A folder directly under a root or under the worktree root whose .git file points at a folder that exists, and that no repository under the roots lists, SHALL be shown with health elsewhere, and its row SHALL name the repository the pointer leads to: the one holding the registration, a bare repository included, or the git directory itself when the pointer names no registration. A relative pointer SHALL be read from the .git file's own folder.

#### Scenario: Directly under a repository root
- **WHEN** a folder directly under C:\Users\me\source\repos has a .git file naming the existing registration D:\other\Northwind.Ledger\.git\worktrees\spike, and D:\other is not a root
- **THEN** the view shows it with health elsewhere and its repository as Northwind.Ledger

#### Scenario: A worktree of a bare repository
- **WHEN** a folder under C:\Users\me\source\worktrees has a .git file naming the existing registration D:\bare\Northwind.Ledger.git\worktrees\spike
- **THEN** the view shows it with health elsewhere and its repository as Northwind.Ledger.git, and force-remove is refused

#### Scenario: A checkout with a separate git directory
- **WHEN** a folder directly under C:\Users\me\source\repos has a .git file naming the existing git directory D:\gitdirs\Northwind.Ledger, which holds no worktrees segment
- **THEN** the view shows it with health elsewhere, not orphan, and force-remove is refused

#### Scenario: A worktree made with relative paths
- **WHEN** a folder under C:\Users\me\source\worktrees has a .git file whose gitdir line is ../../other/Northwind.Ledger/.git/worktrees/spike, and that registration exists relative to the folder
- **THEN** the view shows it with health elsewhere and its repository as Northwind.Ledger, whatever folder Cader was started from

### Requirement: Leftover work is protected from removal
An orphan or broken folder that still contains files SHALL be refused unless forced, because git no longer tracks that work, and a leftover folder with a live session inside SHALL be refused even when forced.

#### Scenario: A live session in a broken folder
- **WHEN** the user force-removes a broken folder that a live session is running in
- **THEN** removal is refused and the folder is kept

### Requirement: An elsewhere worktree is never deleted as a leftover
A row with health elsewhere is not a leftover and SHALL NOT be deleted as a plain folder.

#### Scenario: Remove on an elsewhere row
- **WHEN** the user presses remove on a row with health elsewhere
- **THEN** nothing is deleted and the status line says its repository is not under the roots

### Requirement: The worktrees view explains itself
The worktrees view's help SHALL explain each of its columns and each line of its detail pane; each item of its key bar SHALL show the key's tooltip while the mouse pointer rests on it, and each of its column headers SHALL show what the column shows.

#### Scenario: Hover over the SESSION header
- **WHEN** the mouse pointer rests on the SESSION header of the worktrees table
- **THEN** a tooltip names the column and explains it

### Requirement: A worktree's details are in the pane
The branch, the pull request, the changed files and the unpushed commits SHALL be shown in the detail pane, and the health's reason in full on its Health line; each row SHALL keep its health's colour beside its HEALTH cell. The filter SHALL still match the repository, the folder, the branch and the health.

#### Scenario: Filter by a branch no cell shows
- **WHEN** Northwind.Orders has the worktree C:\Users\me\source\worktrees\spike on branch feature/refund-retry and the user filters by refund
- **THEN** the row stays in the table though no cell shows the branch, and the pane reads Branch feature/refund-retry

### Requirement: The worktrees are scanned as the console opens
The console SHALL start one worktrees scan as it opens, whichever tab is shown, and that scan SHALL wait until the first sessions listing has returned, whether it succeeded, failed or was ended at its time limit.

#### Scenario: Opened during the first scan
- **WHEN** the user shows the Worktrees tab before the start-up scan has returned
- **THEN** the Worktrees tab waits for that scan, and no second scan starts

#### Scenario: The sessions listing failed
- **WHEN** the first sessions listing fails as the console opens
- **THEN** the start-up scan still runs and lists the worktrees with no sessions inside them

#### Scenario: The sessions listing hangs
- **WHEN** the first sessions listing has not exited after the child program time limit
- **THEN** the listing is ended and the start-up scan then runs, listing the worktrees with no sessions inside them

### Requirement: The worktrees are rescanned while their tab is shown
After the first scan, the worktrees SHALL be rescanned each time their tab is shown, on F5 while it is shown, and at the Worktrees refresh interval while it is shown (every 15 seconds by default, and never on a timer when it is 0), never on a timer while it is hidden, and never while another scan is running.

#### Scenario: Never opened
- **WHEN** the console has been open for ten minutes on the Sessions tab
- **THEN** the worktrees have been scanned exactly once

#### Scenario: A slow scan
- **WHEN** the Worktrees tab is shown and a scan of fifty repositories takes forty seconds
- **THEN** the 15-second timer starts no other scan until it returns

#### Scenario: Shown again
- **WHEN** the user leaves the Worktrees tab, runs `git worktree remove` on one of its worktrees in a shell and shows the Worktrees tab again
- **THEN** the tab shows its earlier rows at once and drops the removed worktree when the rescan returns

#### Scenario: A longer interval
- **WHEN** Worktrees refresh is 60 seconds and the Worktrees tab is shown
- **THEN** the timer starts a scan 60 seconds after the last one started, not 15

#### Scenario: The timer off
- **WHEN** Worktrees refresh is 0 and the Worktrees tab has been shown for five minutes with no key pressed
- **THEN** the worktrees were scanned once, when the tab was shown

#### Scenario: F5 rescans now
- **WHEN** the user runs `git worktree add` in a shell while the Worktrees tab is shown, then presses F5
- **THEN** a scan starts at once with no spinner, and the new worktree is listed when it returns

#### Scenario: F5 during a scan
- **WHEN** a scan is running and the user presses F5
- **THEN** no second scan starts

### Requirement: A scan's rows never replace those of a later scan
A scan's rows SHALL never replace those of a later scan.

#### Scenario: A reload after an action overlaps the timer's scan
- **WHEN** the 15-second scan is running, the user removes a worktree, the rescan after the removal returns without it, and then the earlier scan returns with the worktree still listed
- **THEN** the removed worktree stays gone

### Requirement: A worktrees scan reuses the sessions listing
A worktrees scan SHALL take the sessions inside each worktree from the latest sessions listing the console already holds, and SHALL NOT run a Claude Code listing of its own. When no listing has succeeded yet, the scan SHALL treat every worktree as having no session.

#### Scenario: One listing per refresh
- **WHEN** the Worktrees tab is shown and its 15-second rescan runs
- **THEN** no `claude agents` process starts for the rescan, and the SESSION cells match the Sessions tab's latest listing

### Requirement: A worktree's health reads without colour
The worktrees table SHALL show each row's health in a HEALTH column as a glyph and the health's word, in the row's colour: ✓ ok, ◆ locked, △ orphan, ✗ broken, □ empty, ↗ elsewhere, ⑂ branch (dim), ○ origin (amber); no two healths SHALL share a glyph. The help and the HEALTH header's tooltip SHALL list every glyph with its word.

#### Scenario: Every health in the table
- **WHEN** the table lists one worktree of each health, one branch row and one origin row
- **THEN** their HEALTH cells read ✓ ok, ◆ locked, △ orphan, ✗ broken, □ empty, ↗ elsewhere, ⑂ branch and ○ origin

#### Scenario: Two amber healths
- **WHEN** a locked worktree and an elsewhere worktree are listed
- **THEN** both rows are amber, one HEALTH cell reads ◆ locked and the other ↗ elsewhere

#### Scenario: The help lists the glyphs
- **WHEN** the help is shown on the worktrees view
- **THEN** its HEALTH line lists ✓ ok, ◆ locked, △ orphan, ✗ broken, □ empty, ↗ elsewhere, ⑂ branch and ○ origin, and the header's tooltip shows the same words

### Requirement: A prunable worktree whose folder exists is a leftover
When git lists a worktree as prunable but its folder still exists, the view SHALL show it with health broken and a reason saying its .git file is gone, and SHALL count its files as for any leftover folder, so the leftover protection applies: a folder holding files is refused unless forced, and a folder with none is deleted and its registration pruned.

#### Scenario: A worktree that lost its .git file
- **WHEN** C:\Users\me\source\worktrees\Northwind.Orders-fix still holds twelve files but its .git file has been deleted, so git lists it as prunable
- **THEN** the view shows it with health broken and the status line says its .git file is gone, not that the folder is gone

#### Scenario: Remove on it while it holds files
- **WHEN** the user presses remove on that row
- **THEN** nothing is deleted and the status line says twelve untracked files would be lost, and force-remove with the typed folder name deletes the folder and prunes the registration

#### Scenario: A half-finished remove left an empty folder
- **WHEN** git lists a worktree as prunable and its folder exists but holds no files
- **THEN** remove deletes the folder and prunes the registration, as before

### Requirement: A leftover deletion reads the disc when the key is pressed
Whether a leftover folder exists and how many files it holds SHALL be read when the remove key is pressed, not taken from the last scan, and the removal rules SHALL decide on those facts.

#### Scenario: Files written after the scan
- **WHEN** an empty leftover row was scanned and a program has since written three files into its folder
- **THEN** remove deletes nothing and the status line says three untracked files would be lost

### Requirement: A leftover folder is deleted only inside the roots
Before Cader deletes a leftover folder from disc, it SHALL refuse a path that is not under a repository root or the worktree root, changing nothing, and the status line SHALL say the folder is outside the roots and name it. Force-remove SHALL NOT be offered for this refusal.

#### Scenario: A registration that points outside the roots
- **WHEN** a repository under C:\Users\me\source\repos registers a worktree at D:\Northwind.Archive, git lists it as prunable, and the user presses remove or force-remove on its row
- **THEN** D:\Northwind.Archive is not deleted and the status line says the folder is outside the roots, naming D:\Northwind.Archive

#### Scenario: A leftover inside the roots
- **WHEN** the user removes an empty leftover folder C:\Users\me\source\repos\Northwind.Orders.worktrees
- **THEN** the folder is deleted as before

### Requirement: Counting a leftover folder's files never follows links or fails
When the scan or a removal key counts a leftover folder's files, it SHALL NOT enter a junction or symbolic link inside the folder, and SHALL skip a subfolder it cannot read rather than fail, so one such folder never blanks the worktrees view or stops a removal.

#### Scenario: A junction inside a leftover
- **WHEN** an orphan folder holds three files and a node_modules junction pointing at a folder with thousands of files
- **THEN** the row counts three files, and removing it says three untracked files would be lost

#### Scenario: An unreadable subfolder inside a leftover
- **WHEN** a broken folder holds two files the user can read and one subfolder the user cannot
- **THEN** the worktrees view still lists every row, that row counts two files, and no failure is shown

### Requirement: A branch checked out nowhere has a row
The worktrees view SHALL list, for each repository under the roots, every local branch that is checked out neither in the main checkout nor in any linked worktree, as a row with the health ⑂ branch under the repository's heading, whose WORKTREE cell names the branch. The local branch whose upstream is origin's default branch SHALL NOT have such a row. Each such row SHALL be one row of its own, never merged with another row of the same repository.

#### Scenario: A branch with no worktree
- **WHEN** Northwind.Orders's main checkout is on main, its one linked worktree is on fix-123, and it also has the local branch spike-refunds
- **THEN** the view shows a row reading spike-refunds with ⑂ branch in HEALTH under the Northwind.Orders heading, after the fix-123 worktree's row

#### Scenario: The default branch checked out nowhere
- **WHEN** Northwind.Orders's main checkout is on fix-123 and its local main tracks origin/main, origin's default branch
- **THEN** no row is shown for main, and none for fix-123, which the main checkout has

#### Scenario: Two branch rows in one repository
- **WHEN** Northwind.Orders has the branches spike-refunds and old-idea checked out nowhere, and the user selects old-idea
- **THEN** both rows are shown, the selection stays on old-idea after the next scan, and the pane describes old-idea

### Requirement: A flagged branch only on origin has a row
The worktrees view SHALL list a branch that exists on origin, as of the last fetch, with no local branch of that name, only when its tip was authored with the repository's git user email and it is flagged left behind or closed. Its row SHALL read ○ origin in HEALTH. Origin's default branch SHALL never have such a row.

#### Scenario: Left behind on origin
- **WHEN** origin/c-new-bug has no local branch, its tip is yours, and its merged pull request #298's head is one commit behind it
- **THEN** the view shows a row reading c-new-bug with ○ origin in HEALTH and ▲ left behind in BRANCH, under the Northwind.Orders heading

#### Scenario: Someone else's branch
- **WHEN** origin/dependabot/nuget/bump has no local branch, its only pull request was closed, and its tip was authored by another email
- **THEN** no row is shown for it

#### Scenario: Merged with nothing left behind
- **WHEN** origin/fix-88 has no local branch, its tip is yours, and its merged pull request's head is its tip
- **THEN** no row is shown for it

### Requirement: Every row shows its branch's flag
The worktrees table SHALL show a BRANCH column on every row that names a branch, with the most urgent flag that applies as a glyph and a word in the flag's colour: ▲ left behind (red), ✗ gone (amber), ✗ closed (amber), · no PR (dim). It SHALL be blank when none applies, while the branch's pull request has not been looked up, when it could not be looked up, and on a row that names no branch.

#### Scenario: Left behind wins over gone
- **WHEN** a worktree's branch has a merged pull request whose head is two commits behind the branch, and its upstream has been deleted on origin
- **THEN** its BRANCH cell reads ▲ left behind

#### Scenario: Gone after a merge
- **WHEN** a branch row's upstream has been deleted on origin and its merged pull request's head is its tip
- **THEN** its BRANCH cell reads ✗ gone

#### Scenario: Closed without merging
- **WHEN** a worktree's branch has only an abandoned Azure DevOps pull request and its upstream still exists
- **THEN** its BRANCH cell reads ✗ closed

#### Scenario: No pull request
- **WHEN** a branch row's branch has never been pushed and the host lists no pull request for it
- **THEN** its BRANCH cell reads · no PR

#### Scenario: An open pull request
- **WHEN** a worktree's branch has an open pull request and its upstream exists
- **THEN** its BRANCH cell is blank

#### Scenario: The lookup is unavailable
- **WHEN** the GitHub CLI is not signed in
- **THEN** no BRANCH cell reads · no PR or ✗ closed, and a gone upstream still reads ✗ gone

#### Scenario: No origin
- **WHEN** a repository has no origin remote
- **THEN** none of its rows reads · no PR

### Requirement: Left behind is judged against the merged head
A branch SHALL be left behind when the pull request its rows show (the open one, otherwise the newest) has merged, records a head commit, and the branch has commits that head does not contain whose changes origin's default branch does not have. A merge commit, a commit on origin's default branch, or one whose changes match one there SHALL NOT count. A branch whose merged pull request's head commit is unknown to git, or not recorded, SHALL NOT be flagged left behind.

#### Scenario: A squash merge
- **WHEN** a branch's pull request was squash-merged, so none of its commits is on main, and its tip is the pull request's head
- **THEN** it is not flagged left behind

#### Scenario: A head git has not fetched
- **WHEN** a merged pull request's head commit is not in the repository
- **THEN** the branch is not flagged left behind

#### Scenario: Rebased before merging
- **WHEN** a pull request was rebased before it merged, so its merged head and main hold rebased copies, and the local branch still holds the commits from before the rebase
- **THEN** the branch is not flagged left behind

#### Scenario: Reused after merging
- **WHEN** a branch's pull request merged and the branch was later moved forward to main, so every commit on it is on main
- **THEN** the branch is not flagged left behind

#### Scenario: Work pushed after the merge
- **WHEN** a branch has one commit after its pull request merged that is not on main and whose changes main does not have
- **THEN** the branch is flagged left behind and the pane reads 1 commit after the pull request merged

#### Scenario: Origin's default branch is not recorded
- **WHEN** the repository records no default branch for origin, and a branch has two commits its merged pull request's head does not contain
- **THEN** the branch is flagged left behind with 2 commits, as when nothing is compared with main

### Requirement: Branches are judged without reaching the network
A worktrees scan SHALL judge origin's branches and each upstream's state from git's own records as of the last fetch, and SHALL NOT fetch or prune.

#### Scenario: Deleted on origin since the last fetch
- **WHEN** a branch's upstream was deleted on GitHub and no fetch has run since
- **THEN** its BRANCH cell does not read ✗ gone, and no git process that reaches the network is started

### Requirement: The pane explains the branch's flag
For a row whose branch is flagged, the detail pane's Branch line SHALL follow the branch with the flag's reason in full: the number of commits after the pull request merged, that the upstream is gone as of the last fetch, which pull request was closed, or that there is none.

#### Scenario: Left behind
- **WHEN** the c-new-bug row is selected and its branch has one commit after pull request #298 merged
- **THEN** the pane reads Branch c-new-bug · 1 commit after #298 merged

#### Scenario: Gone
- **WHEN** a row whose upstream is gone is selected
- **THEN** the pane's Branch line says origin no longer has the branch as of the last fetch

### Requirement: A branch row is deleted only when no work is lost
On a branch row, d SHALL ask before deleting the local branch when every commit on it is on a remote branch or inside its merged pull request's head, and SHALL otherwise refuse and say how many commits would be lost. D SHALL delete the local branch whatever it holds after the user types its name. Both SHALL refuse on an origin row, and nothing SHALL be deleted on origin.

#### Scenario: A pushed branch
- **WHEN** the user presses d on a branch row whose every commit is on origin and answers Delete branch
- **THEN** the local branch is deleted, the row goes on the next scan, and the status line says the branch was deleted

#### Scenario: Kept
- **WHEN** the user presses d on that row and answers Keep
- **THEN** nothing is deleted

#### Scenario: A squash-merged branch gone from origin
- **WHEN** the user presses d on a branch row whose upstream is gone and whose tip is its merged pull request's head
- **THEN** Cader asks, and deletes the local branch on Delete branch

#### Scenario: Unpushed commits
- **WHEN** the user presses d on a branch row with two commits on no remote branch and no merged pull request containing them
- **THEN** nothing is asked or deleted and the status line says two commits would be lost and D forces it

#### Scenario: Forced
- **WHEN** the user presses D on that row and types spike-refunds
- **THEN** the local branch is deleted

#### Scenario: An origin row
- **WHEN** the user presses d or D on an origin row
- **THEN** nothing is deleted and the status line says Cader never deletes a branch on origin

### Requirement: Keys on a row with no folder
On a branch row or an origin row, p SHALL open its pull request, c SHALL copy the repository's folder and P SHALL prune the repository, as on any row; n, o, f, s and Enter SHALL do nothing but say the branch has no worktree.

#### Scenario: New session on a branch row
- **WHEN** the user presses n on the spike-refunds branch row
- **THEN** no session starts and the status line says spike-refunds has no worktree

#### Scenario: Pull request of an origin row
- **WHEN** the user presses p on the c-new-bug origin row
- **THEN** pull request #298 opens in the browser

### Requirement: The worktrees are rescanned when git records a change
Whether or not the Worktrees tab is shown, Cader SHALL rescan within about two seconds of a listed repository's git folder gaining or losing a worktree's record, of a checkout changing a branch in its main or a linked worktree, and of a folder appearing in or leaving a repository root's top level. Changes in a burst SHALL start at most one scan a second, never two at once, and a change seen during a scan SHALL start one more after it.

#### Scenario: Up to date when shown
- **WHEN** the Sessions tab is shown and the user runs `git worktree add` in a shell, waits three seconds and shows the Worktrees tab
- **THEN** the new worktree is listed at once, before the rescan for the show returns

#### Scenario: Removed in a shell
- **WHEN** the Worktrees tab is shown and the user runs `git worktree remove` on one of its worktrees in a shell
- **THEN** the worktree's row goes within about two seconds

#### Scenario: A worktree made by Claude Code
- **WHEN** a session started with `claude --worktree` adds a worktree under `.claude\worktrees` in a listed repository
- **THEN** it is listed within about two seconds

#### Scenario: A branch checked out
- **WHEN** the user runs `git switch feature/refund-retry` in a listed worktree
- **THEN** within about two seconds a rescan runs, and with the worktree selected the detail pane's Branch line reads feature/refund-retry

#### Scenario: A repository cloned into a root
- **WHEN** the user clones Northwind.Billing into C:\Users\me\source\repos
- **THEN** Northwind.Billing is listed within about two seconds of the clone's folder appearing, and its worktrees are watched from then on

#### Scenario: A root changed in settings
- **WHEN** the user saves D:\work as a second repository root
- **THEN** after the next scan, folders appearing in D:\work and worktrees added to its repositories start a rescan too

#### Scenario: A watched repository deleted
- **WHEN** a listed repository's folder is deleted
- **THEN** a rescan drops it, no error is shown, and it is no longer watched

### Requirement: Git's housekeeping starts no scan
Changes to a repository's index, objects, references, logs or lock files SHALL NOT start a scan, so git's everyday work in a shell and Cader's own git reads never make the worktrees rescan.

#### Scenario: Git's own housekeeping
- **WHEN** the user runs `git status`, `git fetch` or `git commit` in a listed worktree
- **THEN** no scan starts because of it

#### Scenario: Cader's own scan
- **WHEN** a scan reads every listed repository's status
- **THEN** that scan starts no other scan

### Requirement: Rows are grouped by repository
The worktrees table SHALL group its rows by the repository each names (for a leftover, the repository it belonged to; for an empty leftover, the root it sits under), headed by a row reading the repository's name in bold orange, with a faint rule running across the rest of the row; the group's rows SHALL follow it with their WORKTREE cells indented by two cells.

#### Scenario: A group's heading
- **WHEN** Northwind.Orders has the worktrees fix-123 and spike
- **THEN** one bold orange row reading Northwind.Orders, followed by a faint rule, comes before both rows, whose WORKTREE cells start two cells in

### Requirement: Groups run A to Z
Groups SHALL run A to Z by repository name ignoring case, two of one name in folder order. Within a group the worktree folders SHALL come before the branch and origin rows, each A to Z by name ignoring case.

#### Scenario: Two repositories
- **WHEN** Northwind.Orders has the worktree fix-123 and the branch spike-refunds checked out nowhere, and Northwind.Billing has the worktree invoice-pdf
- **THEN** the table reads a Northwind.Billing heading and invoice-pdf, then a Northwind.Orders heading, fix-123 and spike-refunds

#### Scenario: Case is ignored
- **WHEN** the repositories cader and Northwind.Orders each have a worktree
- **THEN** the cader group comes first

#### Scenario: Repositories under two roots
- **WHEN** Northwind.Zeta is under C:\Users\me\source\repos and Northwind.Alpha under D:\work
- **THEN** the Northwind.Alpha group comes first

### Requirement: A heading is never selected
A heading SHALL never be selected: the arrow keys, a scan and a filter SHALL step over it as over no row, and the title and counts SHALL count no heading. A filter SHALL keep a heading only while at least one of its rows matches, and SHALL match rows as before, the repository's name included. A heading SHALL show no tooltip.

#### Scenario: The selection steps over a heading
- **WHEN** invoice-pdf, the last row of the Northwind.Billing group, is selected and the user presses Down
- **THEN** fix-123, the first row under the Northwind.Orders heading, is selected

#### Scenario: A filter by repository
- **WHEN** the user filters by billing
- **THEN** the Northwind.Billing heading and its rows stay, and no other heading shows
