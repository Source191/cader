# Spec Delta

## MODIFIED Requirements

### Requirement: The Config tab lists the settings
The console SHALL have a third tab, titled Config, whose table has the columns SETTING and VALUE and a row per setting in this order: one row per repository root, the first labelled Repository roots and the others with SETTING blank, then Worktree root, Open folder with, Default base, Before branching, Default place, Sessions refresh, Worktrees refresh, Check for updates and Diagnostic log. Each value SHALL be shown whole where it fits and elided in the middle where it does not.

#### Scenario: Default settings
- **WHEN** `config.json` does not exist and the user shows the Config tab
- **THEN** the rows read Repository roots C:\Users\me\source\repos, Worktree root C:\Users\me\source\worktrees, Open folder with VS Code, Default base origin's default branch, Before branching Fetch origin, Default place New worktree, Sessions refresh every 5 seconds, Worktrees refresh every 15 seconds, Check for updates on and Diagnostic log off

#### Scenario: Two roots
- **WHEN** the roots are C:\Users\me\source\repos and D:\work
- **THEN** the first row reads Repository roots C:\Users\me\source\repos and the second has SETTING blank and VALUE D:\work

#### Scenario: A custom editor
- **WHEN** the editor is the executable C:\Tools\subl.exe with arguments -n {folder}
- **THEN** the Open folder with row reads C:\Tools\subl.exe -n {folder}

### Requirement: A setting is edited a row at a time
Enter on a Worktree root, Open folder with, Default base, Sessions refresh or Worktrees refresh row SHALL open a dialog for that one value, and Enter on any repository root row a dialog holding every root, one per line, each showing the setting as saved; F5 or Save SHALL check it, name any problem and keep the dialog open, or write `config.json` and close; Esc SHALL close it unchanged. Enter on Check for updates or Diagnostic log SHALL turn it on or off and save at once.

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

#### Scenario: Change the worktrees refresh
- **WHEN** the user presses Enter on Worktrees refresh, which reads every 15 seconds, types 60 and presses F5
- **THEN** `config.json` holds `worktreesRefreshSeconds` of 60, the row reads every 60 seconds at once, and the status line says it was saved

#### Scenario: The interval dialog opens on the saved value
- **WHEN** Sessions refresh is off and the user presses Enter on it
- **THEN** the dialog's box holds 0

### Requirement: The Config tab's counts and keys
Beside the tab titles the Config tab SHALL count its settings and, when any is missing, the setup to do, the latter in amber. Its key bar SHALL read Enter Edit, then c Copy, f Open folder, d Clear log, / Filter, ? Help, ! Report and q Quit. c SHALL copy a path value whole; f SHALL open a folder value in the editor, the file itself for the Settings file and both diagnostic log file rows, and the folder holding any other file value; on a value that is not a path both SHALL say so.

#### Scenario: Counts
- **WHEN** there are two roots and the Windows Terminal profiles are not set up
- **THEN** the counts read 11 settings · 1 to set up, the second part in amber

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

#### Scenario: c on an interval
- **WHEN** the user presses c on Sessions refresh
- **THEN** nothing is copied and the status line says the value is not a path, as for Default base

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
- **THEN** the counts read 10 settings and the title counts 15 rows

## ADDED Requirements

### Requirement: The refresh rows read as intervals
The Sessions refresh and Worktrees refresh rows SHALL read every second, every N seconds, or off for 0. Each row's tooltip SHALL say what the tab refreshes and when, that 0 turns the timer off while showing the tab, an action and F5 still refresh it, and that Enter changes it. The Sessions refresh tooltip SHALL also say that, while it is off, the bell for a session that starts waiting rings only at the next refresh.

#### Scenario: One second
- **WHEN** `sessionsRefreshSeconds` is 1
- **THEN** the Sessions refresh row reads every second

#### Scenario: Off
- **WHEN** `worktreesRefreshSeconds` is 0
- **THEN** the Worktrees refresh row reads off

#### Scenario: Hover on Sessions refresh
- **WHEN** the mouse rests on the Sessions refresh row
- **THEN** the tooltip says how often the sessions are listed, that 0 turns the timer off, that a waiting session's bell then rings only at the next refresh, and that Enter changes it
