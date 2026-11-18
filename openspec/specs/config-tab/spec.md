# config-tab Specification

## Purpose
The Config tab: a table of Cader's settings and of the setup it depends on, where each setting is edited in a dialog of its own, the repository roots together, and each piece of setup says what to run when it is missing.

## Requirements

### Requirement: The Config tab lists the settings
The console SHALL have a third tab, titled Config, whose table has the columns SETTING and VALUE and a row per setting in this order: one row per repository root, the first labelled Repository roots and the others with SETTING blank, then Worktree root, Open folder with, Default base, Default place, Check for updates and Diagnostic log. Each value SHALL be shown whole where it fits and elided in the middle where it does not.

#### Scenario: Default settings
- **WHEN** `config.json` does not exist and the user shows the Config tab
- **THEN** the rows read Repository roots C:\Users\me\source\repos, Worktree root C:\Users\me\source\worktrees, Open folder with VS Code, Default base origin's default branch, Default place New worktree, Check for updates on and Diagnostic log off

#### Scenario: Two roots
- **WHEN** the roots are C:\Users\me\source\repos and D:\work
- **THEN** the first row reads Repository roots C:\Users\me\source\repos and the second has SETTING blank and VALUE D:\work

#### Scenario: A custom editor
- **WHEN** the editor is the executable C:\Tools\subl.exe with arguments -n {folder}
- **THEN** the Open folder with row reads C:\Tools\subl.exe -n {folder}

### Requirement: The Config tab shows the setup
After the settings, under the Setup heading row, the table SHALL have a grey row for each piece of setup Cader reads but does not edit: Settings file (the full path of `config.json`), Diagnostic log file (on or off, the size of `cader.log` or not written yet, and its full path), Windows Terminal profiles (installed, with the fragment's path, or not set up), Claude bell (on, the other channel named, or not set) and Last update check (when, and the newest release found, or never, or off).

#### Scenario: Everything set up
- **WHEN** the fragment exists, Claude's settings say terminal_bell and the update source was asked at 09:12 today and named 0.18.0
- **THEN** the setup rows read the settings file's path, the diagnostic log file's path, installed with the fragment's path, on and 09:12 today, newest 0.18.0

#### Scenario: Nothing set up
- **WHEN** the fragment does not exist and Claude's settings file has no preferredNotifChannel
- **THEN** Windows Terminal profiles reads not set up, run cader setup-terminal, and Claude bell reads not set, run cader setup-terminal

#### Scenario: Another channel
- **WHEN** Claude's settings say iterm2
- **THEN** Claude bell reads iterm2, not the terminal bell

#### Scenario: The log file's path is always shown
- **WHEN** the local application data folder is C:\Users\me\AppData\Local, the Diagnostic log setting is off, `cader.log` does not exist and the user shows the Config tab
- **THEN** the Diagnostic log file row reads off, C:\Users\me\AppData\Local\Cader\cader.log

#### Scenario: The log is on but nothing is written yet
- **WHEN** the Diagnostic log setting is on and `cader.log` does not exist
- **THEN** the Diagnostic log file row reads on, not written yet, then the full path

#### Scenario: The log is on and written
- **WHEN** the Diagnostic log setting is on and `cader.log` holds 2,516,582 bytes
- **THEN** the Diagnostic log file row reads on, 2.4 MB, then the full path

#### Scenario: The log is off and a file is left
- **WHEN** the Diagnostic log setting is off and `cader.log` holds 512,000 bytes from when it was on
- **THEN** the Diagnostic log file row reads off, 500 KB, then the full path

#### Scenario: A small file
- **WHEN** the Diagnostic log setting is on and `cader.log` holds 300 bytes
- **THEN** the Diagnostic log file row reads on, 1 KB, then the full path

#### Scenario: A full file
- **WHEN** `cader.log` holds 10,485,700 bytes
- **THEN** the Diagnostic log file row shows 10.0 MB

### Requirement: A setting is edited a row at a time
Enter on a Worktree root, Open folder with or Default base row SHALL open a dialog for that one value, and Enter on any repository root row a dialog holding every root, one per line, each showing the setting as saved; F5 or Save SHALL check it, name any problem and keep the dialog open, or write `config.json` and close; Esc SHALL close it unchanged. Enter on Check for updates or Diagnostic log SHALL turn it on or off and save at once.

#### Scenario: Change the worktree root
- **WHEN** the user selects Worktree root, presses Enter, types D:\worktrees and presses F5
- **THEN** `config.json` holds D:\worktrees, the row reads it at once, and the status line says it was saved

#### Scenario: Cancel an edit
- **WHEN** the user adds D:\work to the repository roots and presses Esc
- **THEN** `config.json` and the row are unchanged

#### Scenario: Turn the update check off
- **WHEN** Check for updates is on and the user presses Enter on it
- **THEN** it reads off, `config.json` says so, and the Last update check row reads off

#### Scenario: Turn the diagnostic log on
- **WHEN** Diagnostic log is off and the user presses Enter on it
- **THEN** it reads on, `config.json` says so, and the status line says the log is now written to the full path of `cader.log`

### Requirement: Repository roots are added and removed in their dialog
The repository roots dialog SHALL list every root, one per line; a line added SHALL add a root after the others and a line deleted SHALL remove that root when the dialog saves. A dialog left with no root, or with a root listed twice, SHALL be refused in the dialog. No other key SHALL add or remove a root.

#### Scenario: Add a root
- **WHEN** the user presses Enter on a repository root row, adds the line D:\work, which exists, and presses F5
- **THEN** D:\work is the last repository root row and the next worktree scan includes it

#### Scenario: Remove a root
- **WHEN** the roots are C:\Users\me\source\repos and D:\work and the user deletes the D:\work line and presses F5
- **THEN** only C:\Users\me\source\repos is left and `config.json` says so

#### Scenario: No root left
- **WHEN** the user deletes every line and presses F5
- **THEN** nothing is saved and the dialog asks for a repository root

#### Scenario: A root twice
- **WHEN** the user adds a second line C:\Users\me\source\repos, already a root
- **THEN** nothing is saved and the dialog says it is already a repository root

### Requirement: A setup row says how to change it
Enter on a setup row SHALL change nothing and the status line SHALL say how that setup is changed: `cader setup-terminal` for the profiles and the bell, Check for updates for the update check, and that the settings file is written by this tab.

#### Scenario: Profiles not set up
- **WHEN** the user presses Enter on Windows Terminal profiles
- **THEN** the status line says to run cader setup-terminal and restart Windows Terminal

### Requirement: The Config tab's counts and keys
Beside the tab titles the Config tab SHALL count its settings and, when any is missing, the setup to do, the latter in amber. Its key bar SHALL read Enter Edit, then c Copy, f Open folder, d Clear log, / Filter, ? Help, ! Report and q Quit. c SHALL copy a path value whole; f SHALL open a folder value in the editor, the file itself for the Settings file and both diagnostic log file rows, and the folder holding any other file value; on a value that is not a path both SHALL say so.

#### Scenario: Counts
- **WHEN** there are two roots and the Windows Terminal profiles are not set up
- **THEN** the counts read 9 settings · 1 to set up, the second part in amber

#### Scenario: Copy a long root
- **WHEN** a repository root row's value is elided and the user presses c
- **THEN** the clipboard holds the full path

#### Scenario: Open the settings file's folder
- **WHEN** the user presses f on Settings file
- **THEN** `config.json` itself opens in the editor chosen in settings, where it used to open the folder holding it

#### Scenario: Open the diagnostic log
- **WHEN** `cader.log` exists and the user presses f on Diagnostic log file
- **THEN** `cader.log` opens in the editor chosen in settings, whether or not the Diagnostic log setting is on

#### Scenario: Copy the log file's path
- **WHEN** the user presses c on Diagnostic log file
- **THEN** the clipboard holds the full path of `cader.log` and the status line says it was copied

#### Scenario: The log file does not exist
- **WHEN** `cader.log` does not exist and the user presses f on Diagnostic log file
- **THEN** no editor is started and the status line says the log file does not exist yet and that Diagnostic log turns it on

#### Scenario: The editor cannot be found
- **WHEN** the chosen editor cannot be found and the user presses f on Settings file
- **THEN** nothing is started and the status line says which editor could not be found and that Open folder with chooses another, as it does for a folder

#### Scenario: Another file value keeps opening its folder
- **WHEN** the user presses f on Windows Terminal profiles while it reads installed
- **THEN** the folder holding the fragment opens in the editor

#### Scenario: The key bar names the clear
- **WHEN** the user shows the Config tab
- **THEN** the key bar's second row reads c Copy, f Open folder, d Clear log, / Filter, ? Help, ! Report and q Quit

### Requirement: The Config tab reads the setup when it is shown
The setup rows SHALL be read each time the Config tab is shown and after each save, off the UI thread, and the setting rows SHALL never wait for them; a setup row whose read fails SHALL say it could not be read.

#### Scenario: Profiles installed elsewhere meanwhile
- **WHEN** the user runs cader setup-terminal in another tab and then shows the Config tab
- **THEN** Windows Terminal profiles reads installed

#### Scenario: Unreadable Claude settings
- **WHEN** Claude's settings file is not valid JSON
- **THEN** Claude bell reads could not be read, and every other row is shown as usual

### Requirement: Each setting describes itself
Every Config row SHALL have a description of what the setting does or what the setup is for, shown as a tooltip when the mouse rests on the row. The status line SHALL NOT show the description; on the Config tab it SHALL show only messages, as on the other tabs.

#### Scenario: Status line for the selected row
- **WHEN** the user selects Worktree root and no key has reported anything
- **THEN** there is no status line

#### Scenario: The description follows the selection
- **WHEN** the user moves from Worktree root to Check for updates
- **THEN** the status line stays empty

#### Scenario: Tooltip on hover
- **WHEN** the mouse rests on the Claude bell row
- **THEN** a tooltip says the bell rings when a session finishes or needs permission while you are away

#### Scenario: A message wins
- **WHEN** a setting has just been saved and the user then moves the selection
- **THEN** the status line says it was saved, and goes when the selection moves

### Requirement: The repository roots dialog browses for folders
The repository roots dialog SHALL offer Browse (F3), which opens a folder picker inside the terminal that can choose several folders; each folder chosen SHALL be added as a new line after the roots already listed, and the dialog SHALL check the list on F5 as it does for typed roots. Cancelling the picker SHALL add nothing.

#### Scenario: Add a root by browsing
- **WHEN** the roots are C:\Users\me\source\repos and the user presses F3, chooses D:\work and confirms
- **THEN** the roots box reads C:\Users\me\source\repos and D:\work on its own line, and nothing is saved until F5

#### Scenario: Two folders at once
- **WHEN** the user chooses D:\work and E:\personal in the picker
- **THEN** both are added, each on a line of its own, in the order chosen

#### Scenario: Cancel the picker
- **WHEN** the user presses F3 and then Esc in the picker
- **THEN** the roots box is unchanged

#### Scenario: A folder already listed
- **WHEN** the user browses to C:\Users\me\source\repos, already a root, and presses F5
- **THEN** nothing is saved and the dialog says it is already a repository root

### Requirement: The worktree root dialog browses for a folder
The worktree root dialog SHALL offer Browse (F3), which opens the same folder picker for one folder, starting in the folder holding the current worktree root; the folder chosen SHALL replace the path in the box, and the dialog SHALL check it on F5 as it does for a typed path. Cancelling the picker SHALL change nothing.

#### Scenario: Choose the worktree root by browsing
- **WHEN** the worktree root is C:\Users\me\source\worktrees and the user presses F3, chooses D:\trees and confirms
- **THEN** the box reads D:\trees, and nothing is saved until F5

#### Scenario: Cancel the picker for the worktree root
- **WHEN** the user presses F3 in the worktree root dialog and then Esc in the picker
- **THEN** the box is unchanged

#### Scenario: The picker starts beside the worktree root
- **WHEN** the worktree root is C:\Users\me\source\worktrees and the user presses F3
- **THEN** the picker opens in C:\Users\me\source

### Requirement: Default place is a setting row
The Config tab SHALL show a Default place row after Before branching, reading New worktree or Current branch; Enter on it SHALL open a dialog to choose between them, and saving SHALL write `config.json` and apply to the next wizard at once.

#### Scenario: Default
- **WHEN** `config.json` has no default place
- **THEN** the Default place row reads New worktree

#### Scenario: Choose the current branch
- **WHEN** the user presses Enter on Default place, chooses Current branch and presses F5
- **THEN** the row reads Current branch and the next wizard opened with n starts on Current branch

### Requirement: The last update check's time is local
The Last update check row SHALL show when the update source was last asked in the user's local time zone, using the offset that zone had at that moment, and SHALL say today only when that moment falls on the current local date.

#### Scenario: A check this morning in summer
- **WHEN** the local time zone is the UK's, it is 15 July and the update source was asked at 08:12 UTC that day and named 0.18.0
- **THEN** the row reads 09:12 today, newest 0.18.0

#### Scenario: A check before the clocks went back
- **WHEN** the local time zone is the UK's, it is 1 November and the update source was last asked at 08:12 UTC on 24 October
- **THEN** the row reads 24 October 2026 09:12

### Requirement: The setup sits under its own heading
Between the settings and the setup rows the Config tab's table SHALL have a heading row reading Setup in SETTING and nothing in VALUE. The selection SHALL never rest on it: moving up or down steps over it, and a load never selects it. While a filter is set it SHALL be hidden. It SHALL count neither as a setting in the counts nor as a row in the table's title.

#### Scenario: The heading between the two parts
- **WHEN** the user shows the Config tab with the default settings
- **THEN** the row after Diagnostic log reads Setup, and the row after that Settings file, drawn grey

#### Scenario: Down steps over the heading
- **WHEN** Diagnostic log is selected and the user presses Down
- **THEN** Settings file is selected

#### Scenario: Up steps over the heading
- **WHEN** Settings file is selected and the user presses Up
- **THEN** Diagnostic log is selected

#### Scenario: A filter hides the heading
- **WHEN** the user filters by bell
- **THEN** the table shows only Claude bell, with no Setup heading

#### Scenario: The heading is not counted
- **WHEN** there is one root and every piece of setup is in place
- **THEN** the counts read 8 settings and the title counts 13 rows

### Requirement: The Diagnostic log row names its file
The Diagnostic log row's tooltip SHALL say what the setting does and that Enter turns it on or off, and SHALL NOT repeat the log file's path, which the Diagnostic log file row shows. Its VALUE SHALL read on or off and nothing else.

#### Scenario: Hover on the row
- **WHEN** the mouse rests on the Diagnostic log row
- **THEN** the tooltip says the diagnostic log is written to a file, that the Diagnostic log file row below names it and that Enter turns the log on or off, with no path

#### Scenario: The value stays short
- **WHEN** the setting is on
- **THEN** the row's VALUE reads on, with no path

### Requirement: Before branching is a setting row
The Config tab SHALL show a Before branching row directly after Default base, reading Fetch origin, Pull default branch or Nothing; Enter on it SHALL open a dialog to choose one of the three, where F5 or Save writes `config.json` and Esc closes it unchanged. Its tooltip SHALL say that it is what Cader does before a new worktree branches from origin's default branch.

#### Scenario: Default
- **WHEN** `config.json` has no Before branching and the user shows the Config tab
- **THEN** the row after Default base reads Before branching Fetch origin

#### Scenario: Choose Nothing
- **WHEN** the user presses Enter on Before branching, chooses Nothing and presses F5
- **THEN** the row reads Nothing, `config.json` holds `beforeBranching` of `None`, the status line says it was saved, and the next new worktree is branched without a fetch

#### Scenario: Cancel
- **WHEN** the user presses Enter on Before branching, chooses Pull default branch and presses Esc
- **THEN** `config.json` and the row are unchanged

### Requirement: The Default base row says the base is fetched
The Default base row SHALL read origin's default branch or HEAD, and its tooltip SHALL say that a new worktree branches from origin's default branch after the update Before branching chooses, or from HEAD, never that Cader makes no fetch.

#### Scenario: Default base tooltip
- **WHEN** the user hovers over the Default base row
- **THEN** the tooltip names Before branching and does not say that no fetch is made

### Requirement: The profiles row says when Windows Terminal hides them
While the fragment exists and Windows Terminal is hiding either Cader profile as deleted, as the setup command reports it, the Windows Terminal profiles row SHALL read hidden by Windows Terminal, run cader setup-terminal, and SHALL count as setup to do. When Terminal's files are missing or cannot be read, the row SHALL read as it does without them.

#### Scenario: Deleted in Windows Terminal
- **WHEN** the fragment exists and the user has deleted the "Cader session" profile in Windows Terminal's settings
- **THEN** Windows Terminal profiles reads hidden by Windows Terminal, run cader setup-terminal

#### Scenario: Counted as setup to do
- **WHEN** every other piece of setup is in place and Windows Terminal is hiding the "Cader" profile
- **THEN** the counts end 1 to set up, in amber

#### Scenario: Shown again
- **WHEN** the user runs cader setup-terminal in another tab, agrees to show the hidden profiles again, and then shows the Config tab
- **THEN** Windows Terminal profiles reads installed, with the fragment's path

#### Scenario: Windows Terminal has never run
- **WHEN** the fragment exists and Terminal's state file does not exist
- **THEN** Windows Terminal profiles reads installed, with the fragment's path

#### Scenario: No fragment
- **WHEN** the fragment does not exist and Terminal's state file lists both Cader profiles
- **THEN** Windows Terminal profiles reads not set up, run cader setup-terminal

### Requirement: The setup names the Claude folder
The Setup section SHALL have a grey Claude folder row, directly after Settings file, naming the Claude folder Cader reads and saying whether `CLAUDE_CONFIG_DIR` chose it. A `CLAUDE_CONFIG_DIR` that is not a usable full path SHALL NOT be guessed at: the row SHALL say the variable was not used and that the default folder is read instead. The Claude bell row SHALL read the same folder's `settings.json`.

#### Scenario: The default folder
- **WHEN** Cader was started without `CLAUDE_CONFIG_DIR`
- **THEN** the Claude folder row reads C:\Users\me\.claude

#### Scenario: A folder from CLAUDE_CONFIG_DIR
- **WHEN** Cader was started with `CLAUDE_CONFIG_DIR` set to `C:\Users\me\.claude-personal`, whose `settings.json` says terminal_bell while `C:\Users\me\.claude\settings.json` has no preferredNotifChannel
- **THEN** the Claude folder row reads C:\Users\me\.claude-personal, from CLAUDE_CONFIG_DIR, and the Claude bell row reads on

#### Scenario: A value Cader cannot use
- **WHEN** Cader was started with `CLAUDE_CONFIG_DIR` set to `claude-personal`, which is not a full path
- **THEN** the Claude folder row reads C:\Users\me\.claude and says CLAUDE_CONFIG_DIR was not a full path and was not used, and nothing exits

### Requirement: The older log file has its own row
While `cader.log.1` exists, the Setup section SHALL have a grey Diagnostic log file (older) row directly after the Diagnostic log file row, reading the older file's size and full path, whether the Diagnostic log setting is on or off. While it does not exist the row SHALL NOT be shown. It SHALL count neither as a setting nor as setup to do.

#### Scenario: A rollover has happened
- **WHEN** `cader.log.1` holds 10,485,700 bytes in C:\Users\me\AppData\Local\Cader
- **THEN** the row after Diagnostic log file reads Diagnostic log file (older) and 10.0 MB, C:\Users\me\AppData\Local\Cader\cader.log.1

#### Scenario: No rollover yet
- **WHEN** `cader.log.1` does not exist
- **THEN** the row after Diagnostic log file is Windows Terminal profiles

#### Scenario: Open the older file
- **WHEN** the user presses f on Diagnostic log file (older)
- **THEN** `cader.log.1` itself opens in the editor chosen in settings

#### Scenario: Copy the older file's path
- **WHEN** the user presses c on Diagnostic log file (older)
- **THEN** the clipboard holds the full path of `cader.log.1`

#### Scenario: Not counted
- **WHEN** there is one root, every piece of setup is in place and `cader.log.1` exists
- **THEN** the counts read 8 settings

### Requirement: d clears the diagnostic log after confirming
d on the Diagnostic log file row or the Diagnostic log file (older) row SHALL ask to confirm, naming each file that exists and its size. On Delete it SHALL delete `cader.log` and `cader.log.1`, the status line SHALL say how many files and how much was deleted, and the rows SHALL be read again. Cancel SHALL delete nothing and say so. The diagnostic events kept in memory for a problem report SHALL be kept.

#### Scenario: Clear both files
- **WHEN** `cader.log` holds 2.4 MB and `cader.log.1` 10.0 MB, the user presses d on Diagnostic log file and chooses Delete
- **THEN** both files are deleted, the status line says 2 files (12.4 MB) were deleted, and the Diagnostic log file (older) row is gone

#### Scenario: The question names the files
- **WHEN** `cader.log` holds 2.4 MB and `cader.log.1` does not exist and the user presses d on Diagnostic log file
- **THEN** the dialog asks to delete cader.log (2.4 MB), says the events kept for a problem report stay, and does not name cader.log.1

#### Scenario: From the older row
- **WHEN** the user presses d on Diagnostic log file (older) and chooses Delete
- **THEN** both files are deleted, as from the Diagnostic log file row

#### Scenario: Cancel
- **WHEN** the user presses d on Diagnostic log file and chooses Cancel
- **THEN** both files are as they were and the status line says they were kept

#### Scenario: The log is on
- **WHEN** the Diagnostic log setting is on and the user clears the log
- **THEN** `cader.log` holds only lines recorded after the clear, and the Diagnostic log file row shows its new, small size

#### Scenario: The log is off
- **WHEN** the Diagnostic log setting is off, `cader.log` exists and the user clears the log
- **THEN** no `cader.log` or `cader.log.1` is left, and the Diagnostic log file row reads off, then the full path

#### Scenario: The report keeps its events
- **WHEN** the user clears the log and then presses !
- **THEN** the report holds the same diagnostic events from before the clear as it would have without it

#### Scenario: Nothing to clear
- **WHEN** neither `cader.log` nor `cader.log.1` exists and the user presses d on Diagnostic log file
- **THEN** no dialog opens and the status line says there is no log file to delete

#### Scenario: Gone before Delete
- **WHEN** another program deletes both files after the dialog opens and the user chooses Delete
- **THEN** the status line says there was no log file to delete

#### Scenario: A file in use
- **WHEN** another program holds `cader.log.1` open so that it cannot be deleted, and the user clears the log
- **THEN** `cader.log` is deleted, `cader.log.1` is kept, the status line says one file could not be deleted because another program may have it open and to try again, and nothing fails

#### Scenario: Every file in use
- **WHEN** neither file can be deleted and the user clears the log
- **THEN** both are kept and the status line says no log file could be deleted and to try again, without saying anything was deleted

### Requirement: d elsewhere on the Config tab changes nothing
d on any Config row other than the two diagnostic log file rows SHALL change nothing and the status line SHALL say that d clears the diagnostic log from its file rows.

#### Scenario: d on a setting
- **WHEN** the user presses d on Worktree root
- **THEN** nothing changes and the status line says d clears the diagnostic log on the Diagnostic log file row

#### Scenario: d on the Diagnostic log setting
- **WHEN** the user presses d on Diagnostic log
- **THEN** the setting is unchanged and the status line says the same

### Requirement: A repository root that is not found is marked
When the Config tab reads its setup, it SHALL check whether each repository root's folder exists, and a root whose folder is not found SHALL read `<path> (not found)` in VALUE, with a tooltip saying to create the folder or remove the root. While the setup is being read, every root SHALL read as its path alone.

#### Scenario: A root that does not exist
- **WHEN** the repository roots are C:\Users\me\source\repos and C:\temp\source\repos, and only the first exists
- **THEN** the second root's VALUE reads C:\temp\source\repos (not found) and the first reads C:\Users\me\source\repos

#### Scenario: Its tooltip
- **WHEN** the user hovers over the root that is not found
- **THEN** the tooltip says the folder was not found and to create it or remove the root, and that Enter edits every root

#### Scenario: Before the setup is read
- **WHEN** the Config tab is shown and its setup is still being read
- **THEN** every root reads as its path alone
