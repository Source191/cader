## MODIFIED Requirements

### Requirement: The Config tab shows the setup
After the settings, under the Setup heading row, the table SHALL have a grey row for each piece of setup Cader reads but does not edit: Settings file (the full path of `config.json`), Diagnostic log file (the full path of `cader.log`, whether the Diagnostic log setting is on or off, and whether the file exists yet), Windows Terminal profiles (installed, with the fragment's path, or not set up), Claude bell (on, the other channel named, or not set) and Last update check (when, and the newest release found, or never, or off).

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
- **WHEN** the local application data folder is C:\Users\me\AppData\Local, the Diagnostic log setting is off and the user shows the Config tab
- **THEN** the Diagnostic log file row reads off, C:\Users\me\AppData\Local\Cader\cader.log

#### Scenario: The log is on but nothing is written yet
- **WHEN** the Diagnostic log setting is on and `cader.log` does not exist
- **THEN** the Diagnostic log file row reads on, not written yet, then the full path

#### Scenario: The log is on and written
- **WHEN** the Diagnostic log setting is on and `cader.log` exists
- **THEN** the Diagnostic log file row reads on, then the full path

### Requirement: The Config tab's counts and keys
Beside the tab titles the Config tab SHALL count its settings and, when any is missing, the setup to do, the latter in amber. Its key bar SHALL read Enter Edit, then c Copy, f Open folder, / Filter, ? Help, ! Report and q Quit. c SHALL copy a path value whole; f SHALL open a folder value in the editor, the file itself for the Settings file and Diagnostic log file rows, and the folder holding any other file value; on a value that is not a path both SHALL say so.

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

### Requirement: The Diagnostic log row names its file
The Diagnostic log row's tooltip SHALL say what the setting does and that Enter turns it on or off, and SHALL NOT repeat the log file's path, which the Diagnostic log file row shows. Its VALUE SHALL read on or off and nothing else.

#### Scenario: Hover on the row
- **WHEN** the mouse rests on the Diagnostic log row
- **THEN** the tooltip says the diagnostic log is written to a file, that the Diagnostic log file row below names it and that Enter turns the log on or off, with no path

#### Scenario: The value stays short
- **WHEN** the setting is on
- **THEN** the row's VALUE reads on, with no path

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
