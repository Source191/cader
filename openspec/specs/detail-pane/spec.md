# detail-pane Specification

## Purpose
A pane under the Sessions and Worktrees tables that shows the selected session or worktree in full, so the tables keep only what is worth scanning down and the user reads why a session waits and what a worktree holds without leaving the console.

## Requirements

### Requirement: The pane shows the selected session
On the Sessions tab the pane SHALL show what the selected row does not, titled with its name: what it waits for, when it started and was last prompted, the full folder, the session id, the worktree, the branch, its tab, the folder's changes, its memory and the account it belongs to. A fact that cannot be read SHALL show as unavailable without hiding the rest.

#### Scenario: A session at a permission prompt
- **WHEN** fix-123 is selected, listed as waiting for a permission prompt, started at 14:02 today in a Cader tab coloured yellow, in the worktree Northwind.Orders-fix-123 on branch fix-123 with three changed files
- **THEN** the pane is titled fix-123 and reads Waiting permission prompt, Started 14:02 (5m ago), Worktree fix-123, Branch fix-123, Tab open, yellow, and Changes 3 changed files · 0 unpushed commits with the last commit's age

#### Scenario: Facts in two columns of five rows
- **WHEN** a session is selected
- **THEN** the pane's left column reads, top to bottom, Waiting, Folder, Worktree, Branch and Changes, and its right column Started, Prompted, Session, Tab and Memory, each fact beside the one in the same row, and Account follows on a sixth row under the left column with nothing beside it

#### Scenario: A session never sent a prompt
- **WHEN** a session that needs input and has no transcript is selected
- **THEN** the pane's Waiting line reads a first prompt

#### Scenario: A working session
- **WHEN** a working session is selected
- **THEN** the pane's Waiting line reads nothing

#### Scenario: The main checkout
- **WHEN** a session running in C:\Users\me\source\repos\Northwind.Orders on branch main is selected
- **THEN** the pane reads Worktree main checkout and Branch main

#### Scenario: Memory in the pane
- **WHEN** a running session whose tree uses 903 MB is selected
- **THEN** the pane's Memory line reads 903 MB and names m for the breakdown

#### Scenario: A stopped session
- **WHEN** a stopped session is selected
- **THEN** the pane shows its folder and branch and reads nothing to measure for its memory

#### Scenario: A terminal session
- **WHEN** a plain claude session in its own terminal is selected
- **THEN** the pane shows its facts and its Tab line reads its own terminal

#### Scenario: A session under the current login
- **WHEN** a session owned by the account labelled work, me@northwind.example in Northwind Traders, is selected while that account is logged in
- **THEN** the pane's Account line reads work · me@northwind.example · Northwind Traders · logged in now

#### Scenario: A session under another account
- **WHEN** a session owned by the account labelled personal, me@home.example, is selected while another account is logged in
- **THEN** the pane's Account line reads personal · me@home.example, followed by its organisation and log in as me@home.example to resume it

#### Scenario: An owner not seen yet
- **WHEN** a session whose owner Cader has never seen logged in is selected
- **THEN** the pane's Account line says the account has not been seen logged in, and that logging in as it once lets Cader name it

#### Scenario: No recorded owner
- **WHEN** a session whose transcript has no `session-owner` line is selected
- **THEN** the pane's Account line says Claude Code did not record the account for this session

### Requirement: The last prompt is the transition Cader saw
The pane's Prompted line SHALL show the most recent time, while this Cader ran, that the session left needs input or idle for working, which is when a prompt or an answer reached it. When Cader has not seen such a transition for the session, the line SHALL say so rather than guess, and the transcript SHALL NOT be read for it.

#### Scenario: An answer reaches the session
- **WHEN** fix-123 was needs input at one refresh and working at the next, at 14:03
- **THEN** its Prompted line reads 14:03 and the minutes since

#### Scenario: Not seen
- **WHEN** a session has been idle since Cader started
- **THEN** its Prompted line reads not seen since the console started

### Requirement: The pane shows the selected worktree
On the Worktrees tab the pane SHALL show, for the selected row and titled with its folder's name: the health with its reason in full, the branch and its merge state against origin's default branch (or the branch's flag's reason in place of the merge state when the branch is flagged), the pull request, the full folder, whether and when Cader created it, and its changed files and unpushed commits.

#### Scenario: A worktree with unpushed commits
- **WHEN** Northwind.Billing-invoice-pdf, created by Cader on 23 October, with twelve unpushed commits on invoice-pdf, is selected
- **THEN** the pane reads Health ok, Branch invoice-pdf, not merged into origin/main, Created by Cader, 23 Oct 09:14, Changes 0 changed files · 12 unpushed commits, lists the five newest commits with hash, age and subject, and ends with 7 more

#### Scenario: A broken leftover
- **WHEN** a broken leftover folder is selected
- **THEN** the pane's Health line gives the reason in full, and the git lines read unavailable

#### Scenario: A locked worktree
- **WHEN** a worktree git reports as locked with a reason is selected
- **THEN** the pane's Health line reads locked and the reason

#### Scenario: A flagged worktree
- **WHEN** the worktree Northwind.Orders-fix-123, whose branch's upstream is gone, is selected
- **THEN** the pane reads Branch fix-123 · origin no longer has it, as of the last fetch

### Requirement: The pane lists a worktree's unpushed commits or changed files
Beneath the worktree's details the pane SHALL list its unpushed commits, or its changed files when none are unpushed, as many as the pane's lines hold after the details and one line for how many more, then how many more.

#### Scenario: A worktree with changes and nothing unpushed
- **WHEN** a worktree with two changed files and no unpushed commits is selected
- **THEN** the pane lists the two changed files in place of commits

#### Scenario: A taller pane lists more
- **WHEN** the Worktrees pane has fifteen lines under its divider and a worktree with twelve unpushed commits is selected
- **THEN** the pane shows its four rows of details, lists the ten newest commits and ends with 2 more

#### Scenario: Everything fits
- **WHEN** the Worktrees pane has fifteen lines under its divider and a worktree with eight unpushed commits is selected
- **THEN** the pane lists all eight commits and no line of how many more

### Requirement: The pane shows a branch row or an origin row
For a branch row or an origin row the pane SHALL be titled with the branch, SHALL read no git, and SHALL show the health with its reason, the branch with its flag's reason, the pull request, the repository's folder in place of the folder and Created, and the branch's unpushed commits (or that an origin row has no local branch), with nothing listed beneath.

#### Scenario: A branch row
- **WHEN** the branch row spike-refunds of Northwind.Orders, with two commits on no remote branch and no pull request, is selected
- **THEN** the pane is titled spike-refunds and reads Branch spike-refunds · no pull request on its host, Repository C:\Users\me\source\repos\Northwind.Orders and Changes 2 unpushed commits, and lists no commits or changed files of the main checkout

### Requirement: The pane fits its facts
The pane SHALL sit under the table, behind a divider titled with the selected row's name, and SHALL keep the height its tab was last given, whatever row is selected, so the table's height never changes with the selection. Until the user changes it, the pane SHALL have six lines under its divider on the Sessions tab and ten on the Worktrees tab. A pane shorter than its facts SHALL show the facts that fit, from the top. The Config tab SHALL have no pane. The spinner shown while a table waits for its first rows SHALL sit in the table, above the pane.

#### Scenario: Moving between rows
- **WHEN** the user moves from a worktree with twelve unpushed commits to one with none
- **THEN** the table keeps the same number of rows on screen

#### Scenario: The first worktrees scan
- **WHEN** the Worktrees tab is first shown and its scan has not finished
- **THEN** the waiting spinner and its words are drawn in the middle of the table's area, not in the pane

#### Scenario: Never resized
- **WHEN** Cader starts with no pane height in `config.json`
- **THEN** the Sessions pane has six lines under its divider and the Worktrees pane ten

#### Scenario: A pane shorter than its facts
- **WHEN** the Sessions pane has four lines under its divider and a session is selected
- **THEN** the pane shows its first four rows of facts, from Waiting and Started down to Branch and Tab, and not Changes, Memory or Account

### Requirement: Detail never delays the listing
Reading a row's detail SHALL run off the UI thread, for the selected row only and only while its tab is shown, when the selection changes and after each refresh, and SHALL NOT delay a refresh of the table; a detail that arrives after the selection has moved on, or after a newer detail has been shown, SHALL be dropped.

#### Scenario: Slow git
- **WHEN** the merge check for the selected worktree takes four seconds
- **THEN** the table refreshes on time and the pane's Branch line fills in when the answer arrives

#### Scenario: Selection moves while loading
- **WHEN** the user moves the selection before the previous row's detail has arrived
- **THEN** the pane shows the new row's detail only

### Requirement: The pane's times are local
Every time of day and date the pane shows (Started, Prompted and Created) SHALL be in the user's local time zone, using the offset that zone had at that moment, not the offset it has now. A time SHALL be shown as today, by hour and minute alone, only when it falls on the current local date. How long ago a time was SHALL NOT depend on the time zone.

#### Scenario: A summer start in the UK
- **WHEN** the local time zone is the UK's, it is 14:37 local time on 15 July and fix-123 started at 13:32 UTC that day
- **THEN** the pane reads Started 14:32 (5m ago)

#### Scenario: Before the clocks went back
- **WHEN** the local time zone is the UK's, it is 1 November and a worktree was created by Cader at 08:14 UTC on 23 October
- **THEN** the pane reads Created by Cader, 23 Oct 09:14

#### Scenario: Today is the local date
- **WHEN** the local time zone is Nairobi's (UTC+3), it is 03:30 local time on 2 November (00:30 UTC) and a session started at 21:30 UTC on 1 November
- **THEN** the pane reads Started 00:30 (3h00m ago), as a time today, though the two times fall on different UTC dates

### Requirement: Changed files are listed by their names
When the pane lists a worktree's changed files, each SHALL be shown by its path as named, whatever letters it holds, never as git's quoted escape of the name.

#### Scenario: An accented file name
- **WHEN** a worktree with no unpushed commits has the new file naïve.txt
- **THEN** the pane lists naïve.txt

### Requirement: The pane's divider can be dragged
On the Sessions and Worktrees tabs the user SHALL be able to drag the pane's divider up or down with the mouse; the pane SHALL take the rows the table gives up, and give back the rows it gives up, within the pane's limits.

#### Scenario: Dragging up
- **WHEN** the Worktrees pane has ten lines under its divider and the user drags the divider up four rows
- **THEN** the pane has fourteen lines, the table four rows fewer, and the pane lists four more commits or changed files where the worktree has them

#### Scenario: Dragging past a limit
- **WHEN** the user drags the Worktrees pane's divider to the top of the table
- **THEN** the pane stops where the table keeps its heading and three rows

#### Scenario: Dragging the Sessions pane up
- **WHEN** the Sessions pane has six lines under its divider and the user drags the divider up
- **THEN** the pane keeps its six lines, because it has no more facts to show

### Requirement: The pane can be resized from the keyboard
On the Sessions and Worktrees tabs `+` SHALL grow the shown tab's pane by one line and `-` SHALL shrink it by one, within the pane's limits, except while a filter is being typed, when both are typed into the filter. A press at a limit SHALL change nothing. The help SHALL list both keys, and they SHALL NOT take room in the key bar.

#### Scenario: Growing by a key
- **WHEN** the Worktrees pane has ten lines under its divider and the user presses `+`
- **THEN** the pane has eleven lines and the table one row fewer

#### Scenario: Shrinking by a key
- **WHEN** the Sessions pane has six lines under its divider and the user presses `-` twice
- **THEN** the pane has four lines and the table two rows more

#### Scenario: Typing a filter
- **WHEN** the user has pressed `/` on the Worktrees tab and types `fix-1-2`
- **THEN** the filter reads fix-1-2 and the pane keeps its height

#### Scenario: The help
- **WHEN** the user presses `?` on the Sessions or the Worktrees tab
- **THEN** the help lists `+` as making the detail pane taller and `-` as making it shorter

### Requirement: Each tab's pane has its own height
The Sessions and Worktrees panes SHALL each keep their own height: resizing one SHALL NOT change the other.

#### Scenario: Resizing one tab
- **WHEN** the user makes the Worktrees pane fifteen lines tall and shows the Sessions tab
- **THEN** the Sessions pane has the height it had before

### Requirement: The pane keeps within its limits
The pane SHALL keep at least one line under its divider, and the table SHALL keep its heading and at least three rows. The Sessions pane SHALL grow no taller than its six rows of facts. A terminal too short for a tab's height SHALL show the pane as tall as the limits allow, and SHALL return to that height when the terminal is tall enough again.

#### Scenario: The smallest pane
- **WHEN** the Worktrees pane has one line under its divider and the user presses `-`
- **THEN** the pane keeps its one line

#### Scenario: The tallest Sessions pane
- **WHEN** the Sessions pane has six lines under its divider and the user presses `+`
- **THEN** the pane keeps its six lines

#### Scenario: A shorter terminal
- **WHEN** the Worktrees pane is twenty lines tall and the user makes the terminal too short to leave the table three rows
- **THEN** the pane shrinks so that the table keeps its heading and three rows, and grows back to twenty lines when the terminal is made tall again
